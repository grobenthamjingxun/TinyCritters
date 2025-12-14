using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class SignUpManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Tooltip("Optional feedback text on screen")]
    public TMP_Text feedbackText;

    [Header("Scene Flow")]
    [Tooltip("Scene index to load after successful sign-up.")]
    public int sceneToLoadOnSuccess = 2;

    private bool _subscribed;
    private bool _loadingScene;

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (FirebaseManager.Instance == null)
            yield return null;

        while (!FirebaseManager.Instance.IsReady)
            yield return null;

        if (_subscribed) yield break;
        _subscribed = true;

        FirebaseManager.Instance.OnSignUpCompleted += HandleSignUpCompleted;
        FirebaseManager.Instance.OnSignUpFailed += HandleSignUpFailed;

        SetFeedback("Ready to sign up.");
    }

    private void OnDisable()
    {
        if (FirebaseManager.Instance != null && _subscribed)
        {
            FirebaseManager.Instance.OnSignUpCompleted -= HandleSignUpCompleted;
            FirebaseManager.Instance.OnSignUpFailed -= HandleSignUpFailed;
        }

        _subscribed = false;
    }

    private void SetFeedback(string message)
    {
        Debug.Log("[SignUpManager] " + message);
        if (feedbackText != null) feedbackText.text = message;
    }

    public void SignUp()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
        {
            SetFeedback("Firebase not ready yet...");
            return;
        }

        if (emailInput == null || passwordInput == null)
        {
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

        if (_loadingScene)
        {
            SetFeedback("Already loading...");
            return;
        }

        SetFeedback("Creating account...");
        FirebaseManager.Instance.RegisterUser(email, password);
    }

    private void HandleSignUpCompleted()
    {
        if (_loadingScene) return;
        _loadingScene = true;

        SetFeedback("Account created!");

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoadOnSuccess))
        {
            Debug.LogError("[SignUpManager] Scene index not in build settings: " + sceneToLoadOnSuccess);
            SetFeedback("Scene not in Build Settings: " + sceneToLoadOnSuccess);
            _loadingScene = false;
            return;
        }

        Debug.Log("[SignUpManager] Loading scene " + sceneToLoadOnSuccess + "...");
        SceneManager.LoadScene(sceneToLoadOnSuccess);
    }

    private void HandleSignUpFailed(string error)
    {
        Debug.LogError("[SignUpManager] Sign-Up failed: " + error);
        SetFeedback("Sign-Up failed: " + error);
        _loadingScene = false;
    }
}
