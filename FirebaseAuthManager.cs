using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro; // TextMeshPro namespace
using System;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance;

    [Header("UI Elements")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText; // TextMeshPro feedback text

    private FirebaseAuth auth;
    private FirebaseUser user;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    // -------------------
    // SIGN UP
    // -------------------
    public void SignUp()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email or Password cannot be empty!";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignUp canceled.");
                feedbackText.text = "Sign Up canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignUp error: " + task.Exception);
                feedbackText.text = "Sign Up error: " + task.Exception.InnerExceptions[0].Message;
                return;
            }

            user = task.Result.User;
            Debug.Log("SignUp successful: " + user.Email);
            feedbackText.text = "Sign Up Successful!";
        });
    }

    // -------------------
    // LOGIN
    // -------------------
    public void Login()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email or Password cannot be empty!";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Login canceled.");
                feedbackText.text = "Login canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Login error: " + task.Exception);
                feedbackText.text = "Login error: " + task.Exception.InnerExceptions[0].Message;
                return;
            }

            user = task.Result.User;
            Debug.Log("Login successful: " + user.Email);
            feedbackText.text = "Login Successful!";
            
            // TODO: Load next scene or dashboard
        });
    }

    // -------------------
    // SIGN OUT
    // -------------------
    public void SignOut()
    {
        auth.SignOut();
        feedbackText.text = "Signed Out";
        Debug.Log("User signed out.");
    }

    // Get the current user
    public FirebaseUser GetUser()
    {
        return auth.CurrentUser;
    }
}