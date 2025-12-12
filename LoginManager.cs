using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Tooltip("Optional. Leave empty if you don't want on-screen feedback.")]
    public TMP_Text feedbackText;   // <-- now optional

    private FirebaseAuth auth;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        Debug.Log("LoginManager Awake: auth = " + (auth == null ? "NULL" : "OK"));
    }

    // Helper: safely set feedback (logs to Console even if no TMP_Text)
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
        // Check required refs only (email & password)
        if (emailInput == null)
        {
            Debug.LogError("LoginManager ERROR: emailInput is NOT assigned in the Inspector.");
            return;
        }

        if (passwordInput == null)
        {
            Debug.LogError("LoginManager ERROR: passwordInput is NOT assigned in the Inspector.");
            return;
        }

        if (auth == null)
        {
            SetFeedback("Auth not ready. Check Firebase setup.");
            Debug.LogError("LoginManager ERROR: auth is NULL. Firebase not initialised?");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetFeedback("Email and password cannot be empty.");
            return;
        }

        SetFeedback("Logging in...");
        Debug.Log("Attempting login with: " + email);

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread((Task<AuthResult> task) =>
            {
                if (task.IsCanceled)
                {
                    SetFeedback("Login canceled.");
                    Debug.LogError("Login canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string errorMsg = "Login failed.";

                    if (task.Exception != null)
                    {
                        var baseException = task.Exception.GetBaseException();
                        errorMsg += " " + baseException.Message;
                        Debug.LogError("Raw login exception: " + task.Exception);
                    }

                    SetFeedback(errorMsg);
                    return;
                }

                // Success
                AuthResult result = task.Result;
                FirebaseUser user = result.User;

                SetFeedback("Login successful");
                Debug.Log("Login successful: " + user.UserId + " (" + user.Email + ")");
            });
    }
}
