using UnityEngine;
using TMPro;

public class QuitManager : MonoBehaviour
{
    [Header("Optional UI Feedback")]
    [Tooltip("Optional. Leave empty if you don't want on-screen feedback.")]
    public TMP_Text feedbackText;

    private void SetFeedback(string message)
    {
        Debug.Log("[QuitManager] " + message);
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    /// <summary>
    /// Call this from a UI Button to quit the game.
    /// Works in build. Stops Play Mode in the editor.
    /// </summary>
    public void QuitApplication()
    {
        SetFeedback("Quitting application...");

#if UNITY_EDITOR
        // If running in the Unity editor, stop play mode.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a built game, quit the application.
        Application.Quit();
#endif
    }
}
