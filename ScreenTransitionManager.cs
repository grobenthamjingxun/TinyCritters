using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Tooltip("These object names will NOT be destroyed when switching scenes.")]
    public string[] persistentRootNames = { "FirebaseManager" };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadGameSceneFresh(int gameSceneBuildIndex)
    {
        StartCoroutine(LoadFreshRoutine(gameSceneBuildIndex));
    }

    private IEnumerator LoadFreshRoutine(int buildIndex)
    {
        // Make sure nothing is paused
        Time.timeScale = 1f;

        // Load the target scene
        SceneManager.LoadScene(buildIndex);

        // Wait for scene to actually load
        yield return null;
        yield return null;

        // Clean up any lingering DontDestroy objects you don't want
        DestroyNonPersistentDontDestroyObjects();
    }

    private void DestroyNonPersistentDontDestroyObjects()
    {
        // This finds objects living in the special DontDestroyOnLoad scene
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            if (go.transform.parent != null) continue; // only root objects
            if (go.scene.name != "DontDestroyOnLoad") continue;

            // Keep whitelisted roots
            bool keep = false;
            foreach (var name in persistentRootNames)
            {
                if (!string.IsNullOrEmpty(name) && go.name == name)
                {
                    keep = true;
                    break;
                }
            }
            if (keep) continue;

            // Destroy everything else that was kept alive accidentally
            Debug.Log("[SceneTransitionManager] Destroying leftover DontDestroy object: " + go.name);
            Destroy(go);
        }
    }
}
