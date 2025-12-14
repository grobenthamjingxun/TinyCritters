using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class PangolinManager : MonoBehaviour
{
    public static PangolinManager Instance;

    [Header("Stats (INT 0–100)")]
    [SerializeField] private int hunger = 50;
    [SerializeField] private int happiness = 50;
    [SerializeField] private string growthStage = "egg";

    [Header("Firebase")]
    [Tooltip("If true: only use Firebase when logged in. No guest fallback UID.")]
    [SerializeField] private bool requireAuthenticatedUser = true;

    [Tooltip("Only used if requireAuthenticatedUser = false.")]
    [SerializeField] private string fallbackUserId = "testUser";

    private DatabaseReference pangolinRef;
    private bool loadedFromFirebase;

    [Header("Win / Fail (scene indexes)")]
    [SerializeField] private int successThreshold = 100;
    [SerializeField] private int failThreshold = 0;
    [SerializeField] private int successSceneIndex = 7;
    [SerializeField] private int failSceneIndex = 6;

    [Header("Scale Growth (applied on feeding)")]
    [SerializeField] private float maxScaleMultiplier = 1.8f;
    private Vector3 baseScale;
    private float currentScaleMultiplier = 1f;

    // ===== GLOW =====
    private Coroutine glowRoutine;
    private Renderer[] cachedRenderers;
    private MaterialPropertyBlock mpb;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    // Scene-load guard
    private bool endTriggered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        baseScale = transform.localScale;

        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
    }

    private void Start()
    {
        // Don’t touch Firebase until FirebaseManager is ready (prevents null-user fallback issues)
        StartCoroutine(InitWhenFirebaseReady());
    }

    private IEnumerator InitWhenFirebaseReady()
    {
        // If you don’t have FirebaseManager in this scene, stop gracefully
        if (FirebaseManager.Instance == null)
        {
            Debug.LogWarning("[PangolinManager] No FirebaseManager found. Running local-only.");
            UIManager.Instance?.UpdateUI(hunger, happiness, growthStage);
            yield break;
        }

        while (!FirebaseManager.Instance.IsReady)
            yield return null;

        string uid = null;

        if (FirebaseManager.Instance.User != null)
        {
            uid = FirebaseManager.Instance.User.UserId;
        }
        else
        {
            if (requireAuthenticatedUser)
            {
                Debug.LogWarning("[PangolinManager] No authenticated user. Running local-only (no scene auto-fail).");
                UIManager.Instance?.UpdateUI(hunger, happiness, growthStage);
                yield break;
            }

            uid = fallbackUserId;
        }

        pangolinRef = FirebaseDatabase.DefaultInstance.RootReference
            .Child("pangolins")
            .Child(uid);

        Debug.Log("[PangolinManager] Firebase path: /pangolins/" + uid);

        LoadFromFirebase();
    }

    private void LoadFromFirebase()
    {
        if (pangolinRef == null) return;

        pangolinRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("[PangolinManager] LoadFromFirebase failed/canceled. Running with local defaults.");
                UIManager.Instance?.UpdateUI(hunger, happiness, growthStage);
                return;
            }

            var snap = task.Result;
            if (snap == null || !snap.Exists)
            {
                Debug.Log("[PangolinManager] No existing pangolin data. Using local defaults.");
                UIManager.Instance?.UpdateUI(hunger, happiness, growthStage);
                loadedFromFirebase = true; // allow saving later
                return;
            }

            hunger = ToInt(snap.Child("hunger").Value, hunger);
            happiness = ToInt(snap.Child("happiness").Value, happiness);
            if (snap.Child("growthStage").Value != null)
                growthStage = snap.Child("growthStage").Value.ToString();

            loadedFromFirebase = true;

            UIManager.Instance?.UpdateUI(hunger, happiness, growthStage);

            // Only check end conditions AFTER we have stable values
            CheckEndConditions();
        });
    }

    // =========================================================
    // Backwards compatibility
    // =========================================================
    public void ApplyDelta(string itemName, int hungerDelta, int happinessDelta, bool setLastFed)
    {
        ApplyItem(itemName, hungerDelta, happinessDelta, setLastFed);
    }

    // =========================================================
    // MAIN update
    // =========================================================
    public void ApplyItem(string itemName, int hungerDelta, int happinessDelta, bool setLastFed = false)
    {
        hunger = Mathf.Clamp(hunger + hungerDelta, 0, 100);
        happiness = Mathf.Clamp(happiness + happinessDelta, 0, 100);
        growthStage = ComputeStage(hunger, happiness);

        UIManager.Instance?.UpdateUI(hunger, happiness, growthStage);

        CheckEndConditions();

        if (!loadedFromFirebase || pangolinRef == null)
        {
            Debug.Log($"[PangolinManager] Local-only update: item={itemName} hΔ={hungerDelta} happyΔ={happinessDelta} fed={setLastFed}");
            return;
        }

        var update = new Dictionary<string, object>
        {
            { "hunger", hunger },
            { "happiness", happiness },
            { "growthStage", growthStage }
        };

        if (setLastFed)
            update["lastFed"] = DateTime.UtcNow.ToString("o");

        // ✅ Properly log the write result
        pangolinRef.UpdateChildrenAsync(update).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[PangolinManager] ❌ UpdateChildrenAsync failed: " + task.Exception);
            }
            else
            {
                Debug.Log("[PangolinManager] ✅ Updated stats in Firebase: item=" + itemName +
                          " hungerDelta=" + hungerDelta + " happinessDelta=" + happinessDelta +
                          " setLastFed=" + setLastFed);
            }
        });

        LogItemToFirebase(itemName, hungerDelta, happinessDelta, setLastFed);
    }

    private void LogItemToFirebase(string itemName, int hungerDelta, int happinessDelta, bool setLastFed)
    {
        if (pangolinRef == null) return;

        string key = pangolinRef.Child("feedHistory").Push().Key;
        if (string.IsNullOrEmpty(key)) return;

        var entry = new Dictionary<string, object>
        {
            { "item", itemName },
            { "timestampUtc", DateTime.UtcNow.ToString("o") },
            { "hungerDelta", hungerDelta },
            { "happinessDelta", happinessDelta },
            { "hungerAfter", hunger },
            { "happinessAfter", happiness },
            { "growthStageAfter", growthStage },
            { "countsAsFed", setLastFed }
        };

        // ✅ Properly log the write result
        pangolinRef.Child("feedHistory").Child(key).SetValueAsync(entry).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[PangolinManager] ❌ feedHistory write failed: " + task.Exception);
            }
            else
            {
                Debug.Log("[PangolinManager] ✅ feedHistory logged: " + key +
                          " item=" + itemName +
                          " hungerDelta=" + hungerDelta +
                          " happinessDelta=" + happinessDelta +
                          " countsAsFed=" + setLastFed);
            }
        });
    }

    // =========================================================
    // SCALE
    // =========================================================
    public void AddScale(float addMultiplier)
    {
        currentScaleMultiplier = Mathf.Clamp(currentScaleMultiplier + addMultiplier, 1f, maxScaleMultiplier);
        transform.localScale = baseScale * currentScaleMultiplier;
    }

    // =========================================================
    // WIN/FAIL (guarded, validated)
    // =========================================================
    private void CheckEndConditions()
    {
        if (endTriggered) return;

        bool success = hunger >= successThreshold || happiness >= successThreshold;
        bool fail = hunger <= failThreshold || happiness <= failThreshold;

        if (!success && !fail) return;

        int target = success ? successSceneIndex : failSceneIndex;

        if (!Application.CanStreamedLevelBeLoaded(target))
        {
            Debug.LogError("[PangolinManager] Target scene not in Build Settings: " + target);
            return;
        }

        endTriggered = true;
        Debug.Log("[PangolinManager] End condition met. Loading scene: " + target);
        SceneManager.LoadScene(target);
    }

    private string ComputeStage(int h, int happy)
    {
        int sum = h + happy;
        if (sum >= 150) return "adult";
        if (sum >= 80) return "teen";
        if (sum >= 50) return "baby";
        return "egg";
    }

    private static int ToInt(object value, int fallback)
    {
        if (value == null) return fallback;
        if (value is long l) return (int)l;
        if (value is int i) return i;
        if (value is double d) return (int)d;
        return int.TryParse(value.ToString(), out int parsed) ? parsed : fallback;
    }

    // =========================================================
    // GLOW
    // =========================================================
    public void PulseGlow(Color glowColor, float intensity, float duration)
    {
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(PulseGlowRoutine(glowColor, intensity, duration));
    }

    private IEnumerator PulseGlowRoutine(Color glowColor, float intensity, float duration)
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0) yield break;

        float half = Mathf.Max(0.01f, duration * 0.5f);

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float v = Mathf.Lerp(0f, intensity, t / half);
            SetEmissionForAll(glowColor * v);
            yield return null;
        }

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float v = Mathf.Lerp(intensity, 0f, t / half);
            SetEmissionForAll(glowColor * v);
            yield return null;
        }

        ClearEmissionOverride();
        glowRoutine = null;
    }

    private void SetEmissionForAll(Color emission)
    {
        foreach (var r in cachedRenderers)
        {
            if (r == null) continue;

            bool supports = false;
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].HasProperty(EmissionColorId))
                {
                    supports = true;
                    break;
                }
            }
            if (!supports) continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionColorId, emission);
            r.SetPropertyBlock(mpb);
        }
    }

    private void ClearEmissionOverride()
    {
        foreach (var r in cachedRenderers)
        {
            if (r == null) continue;
            r.SetPropertyBlock(null);
        }
    }
}
