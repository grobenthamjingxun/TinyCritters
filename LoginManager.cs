using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Tooltip("Optional feedback text on screen")]
    public TMP_Text feedbackText;

    [Header("Scene Flow")]
    [Tooltip("Scene index to load after successful login.")]
    public int sceneToLoadOnSuccess = 2;

    private bool _subscribed;
    private bool _loadingScene;

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private IEnumerator SubscribeWhenReady()
    {
        // Wait until FirebaseManager exists
        while (FirebaseManager.Instance == null)
            yield return null;

        // Wait until Firebase is initialised
        while (!FirebaseManager.Instance.IsReady)
            yield return null;

        if (_subscribed) yield break;
        _subscribed = true;

        FirebaseManager.Instance.OnLoginCompleted += HandleLoginCompleted;
        FirebaseManager.Instance.OnLoginFailed += HandleLoginFailed;

        SetFeedback("Ready to login.");
    }

    private void OnDisable()
    {
        if (FirebaseManager.Instance != null && _subscribed)
        {
            FirebaseManager.Instance.OnLoginCompleted -= HandleLoginCompleted;
            FirebaseManager.Instance.OnLoginFailed -= HandleLoginFailed;
        }

        _subscribed = false;
    }

    private void SetFeedback(string message)
    {
        Debug.Log("[LoginManager] " + message);
        if (feedbackText != null) feedbackText.text = message;
    }

    public void Login()
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

        if (_loadingScene)
        {
            SetFeedback("Already loading...");
            return;
        }

        SetFeedback("Logging in...");
        FirebaseManager.Instance.LoginUser(email, password);
    }

    private void HandleLoginCompleted()
    {
        if (_loadingScene) return;
        _loadingScene = true;

        SetFeedback("Login successful!");

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoadOnSuccess))
        {
            Debug.LogError("[LoginManager] Scene index not in build settings: " + sceneToLoadOnSuccess);
            SetFeedback("Scene not in Build Settings: " + sceneToLoadOnSuccess);
            _loadingScene = false;
            return;
        }

        Debug.Log("[LoginManager] Loading scene " + sceneToLoadOnSuccess + "...");
        SceneManager.LoadScene(sceneToLoadOnSuccess);
    }

    private void HandleLoginFailed(string error)
    {
        Debug.LogError("[LoginManager] Login failed: " + error);
        SetFeedback("Login failed: " + error);
        _loadingScene = false;
    }
}
