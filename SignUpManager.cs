using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SignUpManager : MonoBehaviour
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
            Debug.LogError("[SignUpManager] No FirebaseManager in scene.");
            SetFeedback("Firebase not ready.");
            return;
        }

        FirebaseManager.Instance.OnSignUpCompleted += HandleSignUpCompleted;
        FirebaseManager.Instance.OnSignUpFailed += HandleSignUpFailed;
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnSignUpCompleted -= HandleSignUpCompleted;
            FirebaseManager.Instance.OnSignUpFailed -= HandleSignUpFailed;
        }
    }

    private void SetFeedback(string message)
    {
        Debug.Log("[SignUpManager] " + message);
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void SignUp()
    {
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("[SignUpManager] FirebaseManager missing.");
            SetFeedback("Firebase not ready.");
            return;
        }

        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("[SignUpManager] Input fields not assigned.");
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

        if (password.Length < 6)
        {
            SetFeedback("Password must be at least 6 characters.");
            return;
        }

        SetFeedback("Creating account...");
        FirebaseManager.Instance.RegisterUser(email, password);
    }

    private void HandleSignUpCompleted()
    {
        Debug.Log("[SignUpManager] Sign-Up successful!");
        SetFeedback("Account created!");

        // Proceed to next scene
        SceneManager.LoadScene(2);  // change if needed
    }

    private void HandleSignUpFailed(string error)
    {
        Debug.LogError("[SignUpManager] Sign-Up failed: " + error);
        SetFeedback("Sign-Up failed: " + error);
    }
}
