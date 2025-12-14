using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARHardRestartOnSceneLoad : MonoBehaviour
{
    [SerializeField] private float maxWaitSeconds = 6f;

    private IEnumerator Start()
    {
        // If a previous scene paused the game, AR can stay black.
        Time.timeScale = 1f;

        // Wait a couple frames so objects enable
        yield return null;
        yield return null;

        var session = FindObjectOfType<ARSession>(true);
        if (session == null)
        {
            Debug.LogError("[ARHardRestart] No ARSession found in scene.");
            yield break;
        }

        // Some devices need availability check to finish after a scene switch
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            ARSession.CheckAvailability();
        }

        // Hard restart
        session.enabled = false;
        yield return null;
        session.enabled = true;
        yield return null;

        session.Reset();

        // Wait until tracking or timeout (prevents “black sometimes”)
        float t = 0f;
        while (t < maxWaitSeconds &&
               ARSession.state != ARSessionState.SessionTracking &&
               ARSession.state != ARSessionState.Ready)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log("[ARHardRestart] ARSession.state = " + ARSession.state);

        // If still not tracking, log a strong hint
        if (ARSession.state == ARSessionState.Unsupported)
            Debug.LogError("[ARHardRestart] Device/config unsupported (or AR services missing).");
        if (ARSession.state == ARSessionState.NeedsInstall)
            Debug.LogWarning("[ARHardRestart] NeedsInstall (Google Play Services for AR missing).");
    }
}
