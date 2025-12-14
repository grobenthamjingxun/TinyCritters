using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class ARSceneInitializer : MonoBehaviour
{
    private ARSession arSession;

    private IEnumerator Start()
    {
        // Ensure time isn't paused from previous scene
        Time.timeScale = 1f;

        // Wait one frame for scene load
        yield return null;

        arSession = FindObjectOfType<ARSession>();

        if (arSession == null)
        {
            Debug.LogError("[ARSceneInitializer] ❌ No ARSession found in scene!");
            yield break;
        }

        Debug.Log("[ARSceneInitializer] ARSession found. State = " + ARSession.state);

        // If already tracking, no need to reset
        if (ARSession.state == ARSessionState.SessionTracking)
        {
            Debug.Log("[ARSceneInitializer] AR already tracking.");
            yield break;
        }

        Debug.Log("[ARSceneInitializer] Resetting ARSession...");
        arSession.Reset();

        // Wait until AR becomes ready or tracking
        while (ARSession.state != ARSessionState.SessionTracking &&
               ARSession.state != ARSessionState.Ready)
        {
            Debug.Log("[ARSceneInitializer] Waiting for ARSession... " + ARSession.state);
            yield return null;
        }

        Debug.Log("[ARSceneInitializer] ✅ ARSession active: " + ARSession.state);
    }
}
