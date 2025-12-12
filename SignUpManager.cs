using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using System.Threading.Tasks;

public class SignUpManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        Debug.Log("SignUpManager Awake: auth = " + (auth == null ? "NULL" : "OK"));
    }

    // ⚠️ This is the ONLY method your Sign Up button should call
    public void SignUp()
    {
        // 1) Check all refs first so we don't crash
        if (emailInput == null)
        {
            Debug.LogError("SignUpManager ERROR: emailInput is NOT assigned in the Inspector.");
            return;
        }

        if (passwordInput == null)
        {
            Debug.LogError("SignUpManager ERROR: passwordInput is NOT assigned in the Inspector.");
            return;
        }

        if (feedbackText == null)
        {
            Debug.LogError("SignUpManager ERROR: feedbackText is NOT assigned in the Inspector.");
            return;
        }

        if (auth == null)
        {
            Debug.LogError("SignUpManager ERROR: auth is NULL. Check Firebase initialisation.");
            feedbackText.text = "Auth not ready. Check Firebase setup.";
            return;
        }

        // 2) Safe to use them now
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email and password cannot be empty.";
            Debug.LogError("Sign-Up failed: Empty input.");
            return;
        }

        if (password.Length < 6)
        {
            feedbackText.text = "Password must be at least 6 characters.";
            Debug.LogError("Sign-Up failed: Password too short.");
            return;
        }

        feedbackText.text = "Creating account...";
        Debug.Log("Attempting sign-up with: " + email);

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread((Task<AuthResult> task) =>
            {
                if (task.IsCanceled)
                {
                    feedbackText.text = "Sign-Up canceled.";
                    Debug.LogError("Sign-Up canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    string errorMsg = "Sign-Up failed.";

                    if (task.Exception != null)
                    {
                        var baseException = task.Exception.GetBaseException();
                        errorMsg += " " + baseException.Message;
                        Debug.LogError("Raw sign-up exception: " + task.Exception);
                    }

                    feedbackText.text = errorMsg;
                    return;
                }

                AuthResult result = task.Result;
                FirebaseUser newUser = result.User;

                feedbackText.text = "Sign-Up successful! Welcome " + newUser.Email;
                Debug.Log("Sign-Up successful: " + newUser.UserId);
            });
    }
}
