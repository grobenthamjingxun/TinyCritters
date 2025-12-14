using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARSceneFreshStart : MonoBehaviour
{
    private ARSession arSession;
    private ARTrackedImageManager trackedImageManager;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        // Wait 1–2 frames so all AR objects are instantiated/enabled
        yield return null;
        yield return null;

        arSession = FindObjectOfType<ARSession>();
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        if (arSession == null)
        {
            Debug.LogError("[ARSceneFreshStart] No ARSession found in this scene.");
            yield break;
        }

        // Step 1: temporarily disable tracking managers to clear old state
        if (trackedImageManager != null)
            trackedImageManager.enabled = false;

        // Step 2: restart ARSession
        arSession.enabled = false;
        yield return null;

        arSession.enabled = true;
        yield return null;

        // Step 3: reset session (instance method)
        arSession.Reset();

        // Step 4: re-enable image tracking
        if (trackedImageManager != null)
            trackedImageManager.enabled = true;

        Debug.Log("[ARSceneFreshStart] ✅ AR scene restarted cleanly.");
    }
}
