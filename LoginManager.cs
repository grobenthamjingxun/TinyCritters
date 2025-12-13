using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Tooltip("Optional feedback text on screen")]
    public TMP_Text feedbackText;

    private void Start()
    {
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("[LoginManager] No FirebaseManager in scene.");
            SetFeedback("Firebase not set up.");
            return;
        }

        FirebaseManager.Instance.OnLoginCompleted += HandleLoginCompleted;
        FirebaseManager.Instance.OnLoginFailed += HandleLoginFailed;
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnLoginCompleted -= HandleLoginCompleted;
            FirebaseManager.Instance.OnLoginFailed -= HandleLoginFailed;
        }
    }

    private void SetFeedback(string message)
    {
        Debug.Log("[LoginManager] " + message);
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void Login()
    {
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("[LoginManager] Cannot Login: FirebaseManager.Instance is null.");
            SetFeedback("Firebase not ready.");
            return;
        }

        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("[LoginManager] Email or password input not assigned.");
            SetFeedback("Missing input fields.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetFeedback("Please enter email and password.");
            return;
        }

        SetFeedback("Logging in...");
        FirebaseManager.Instance.LoginUser(email, password);
    }

    private void HandleLoginCompleted()
    {
        SetFeedback("Login successful!");

        // Load your main game scene (change index/name as needed)
        Debug.Log("[LoginManager] Login successful, loading scene 5...");
        SceneManager.LoadScene(5);
    }

    private void HandleLoginFailed(string error)
    {
        Debug.LogError("[LoginManager] Login failed: " + error);
        SetFeedback("Login failed: " + error);
    }
}
