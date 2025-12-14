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

    [Header("Stats (INT)")]
    [SerializeField] private int hunger = 50;
    [SerializeField] private int happiness = 50;
    [SerializeField] private string growthStage = "egg";

    [Header("Firebase")]
    [SerializeField] private string fallbackUserId = "testUser";
    private DatabaseReference pangolinRef;
    private bool loaded;

    [Header("Win / Fail (scene indexes)")]
    [SerializeField] private int successThreshold = 100;
    [SerializeField] private int failThreshold = 0;
    [SerializeField] private int successSceneIndex = 1;
    [SerializeField] private int failSceneIndex = 2;

    [Header("Growth Scaling")]
    [SerializeField] private float maxScaleMultiplier = 1.8f;
    private Vector3 baseScale;
    private float currentScaleMultiplier = 1f;

    private Coroutine glowRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        baseScale = transform.localScale;
    }

    private void Start()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.User != null)
        {
            InitFirebase(FirebaseManager.Instance.User.UserId);
        }
        else
        {
            InitFirebase(fallbackUserId);
        }

        LoadFromFirebase();
    }

    private void InitFirebase(string uid)
    {
        pangolinRef = FirebaseDatabase.DefaultInstance
            .RootReference
            .Child("pangolins")
            .Child(uid);
    }

    private void LoadFromFirebase()
    {
        pangolinRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            loaded = true;
        });
    }

    // ============================
    // 🔥 MAIN ENTRY POINT
    // ============================
    public void ApplyItem(string itemName, int hungerDelta, int happinessDelta)
    {
        hunger = Mathf.Clamp(hunger + hungerDelta, 0, 100);
        happiness = Mathf.Clamp(happiness + happinessDelta, 0, 100);
        growthStage = ComputeStage(hunger, happiness);

        CheckEndConditions();

        if (!loaded) return;

        // Update core stats
        var update = new Dictionary<string, object>
        {
            { "hunger", hunger },
            { "happiness", happiness },
            { "growthStage", growthStage }
        };

        pangolinRef.UpdateChildrenAsync(update);

        // 🔥 LOG EVERY ITEM
        LogItem(itemName, hungerDelta, happinessDelta);
    }

    // ============================
    // 📝 ITEM LOGGING (ALWAYS)
    // ============================
    private void LogItem(string itemName, int hungerDelta, int happinessDelta)
    {
        string key = pangolinRef.Child("feedHistory").Push().Key;

        var entry = new Dictionary<string, object>
        {
            { "item", itemName },
            { "hungerDelta", hungerDelta },
            { "happinessDelta", happinessDelta },
            { "hungerAfter", hunger },
            { "happinessAfter", happiness },
            { "growthStageAfter", growthStage },
            { "timestampUtc", DateTime.UtcNow.ToString("o") }
        };

        pangolinRef.Child("feedHistory").Child(key).SetValueAsync(entry);
    }

    private void CheckEndConditions()
    {
        if (hunger >= successThreshold || happiness >= successThreshold)
        {
            SceneManager.LoadScene(successSceneIndex);
        }
        else if (hunger <= failThreshold || happiness <= failThreshold)
        {
            SceneManager.LoadScene(failSceneIndex);
        }
    }

    private string ComputeStage(int h, int happy)
    {
        int sum = h + happy;
        if (sum >= 150) return "adult";
        if (sum >= 80) return "teen";
        if (sum >= 50) return "baby";
        return "egg";
    }

    // ============================
    // ✨ GLOW (PANGOLIN ONLY)
    // ============================
    public void PulseGlow(Color color, float intensity, float duration)
    {
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(GlowRoutine(color, intensity, duration));
    }

    private IEnumerator GlowRoutine(Color color, float intensity, float duration)
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        List<Material> mats = new List<Material>();

        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                if (m.HasProperty("_EmissionColor"))
                {
                    m.EnableKeyword("_EMISSION");
                    mats.Add(m);
                }
            }
        }

        float half = duration / 2f;

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            SetEmission(mats, color * Mathf.Lerp(0, intensity, t / half));
            yield return null;
        }

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            SetEmission(mats, color * Mathf.Lerp(intensity, 0, t / half));
            yield return null;
        }

        SetEmission(mats, Color.black);
    }

    private void SetEmission(List<Material> mats, Color color)
    {
        foreach (var m in mats)
            m.SetColor("_EmissionColor", color);
    }

    public void AddScale(float add)
    {
        currentScaleMultiplier = Mathf.Clamp(currentScaleMultiplier + add, 1f, maxScaleMultiplier);
        transform.localScale = baseScale * currentScaleMultiplier;
    }
}
