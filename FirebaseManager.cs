using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions; // For ContinueWithOnMainThread
using System;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }
    public DatabaseReference DbRef { get; private set; }

    public bool IsReady { get; private set; }

    // Events for UI to subscribe to
    public event Action OnLoginCompleted;
    public event Action<string> OnLoginFailed;
    public event Action OnSignUpCompleted;
    public event Action<string> OnSignUpFailed;

    public event Action<Dictionary<string, object>> OnPangolinDataLoaded;
    public event Action<string> OnPangolinDataLoadFailed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            IsReady = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("[FirebaseManager] Checking Firebase dependencies...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[FirebaseManager] CheckAndFixDependenciesAsync failed: " + task.Exception);
                return;
            }

            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("[FirebaseManager] Firebase dependencies available. Initialising...");
                InitFirebase();
            }
            else
            {
                Debug.LogError("[FirebaseManager] Could not resolve all Firebase dependencies: " + status);
            }
        });
    }

    private void InitFirebase()
    {
        Auth = FirebaseAuth.DefaultInstance;
        DbRef = FirebaseDatabase.DefaultInstance.RootReference;
        IsReady = true;

        Debug.Log("[FirebaseManager] Firebase initialised. Auth + Realtime Database ready.");

        // Optional: keep user in sync
        Auth.StateChanged += OnAuthStateChanged;
        OnAuthStateChanged(this, null);
    }

    private void OnDestroy()
    {
        if (Auth != null)
        {
            Auth.StateChanged -= OnAuthStateChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (Auth == null) return;

        if (Auth.CurrentUser != User)
        {
            bool signedIn = Auth.CurrentUser != null;
            if (!signedIn && User != null)
            {
                Debug.Log("[FirebaseManager] Signed out: " + User.UserId);
            }

            User = Auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("[FirebaseManager] Signed in: " + User.UserId);
            }
        }
    }

    // ===========================
    // AUTH: REGISTER / LOGIN
    // ===========================

    public void RegisterUser(string email, string password)
    {
        if (!IsReady || Auth == null)
        {
            Debug.LogError("[FirebaseManager] RegisterUser called before Firebase is ready.");
            OnSignUpFailed?.Invoke("Firebase not ready.");
            return;
        }

        Debug.Log("[FirebaseManager] RegisterUser() with email: " + email);

        Auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[FirebaseManager] RegisterUser was canceled.");
                    OnSignUpFailed?.Invoke("Registration canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("[FirebaseManager] RegisterUser error: " + task.Exception);
                    OnSignUpFailed?.Invoke(task.Exception != null ? task.Exception.Message : "Unknown error.");
                    return;
                }

                User = task.Result.User;
                Debug.Log("[FirebaseManager] ✅ Sign-Up successful: " + User.UserId);

                // Create default pangolin data for this new user
                CreatePangolinProfile();

                // Fire events
                OnSignUpCompleted?.Invoke();
                OnLoginCompleted?.Invoke(); // Because user is also logged in after sign-up
            });
    }

    public void LoginUser(string email, string password)
    {
        if (!IsReady || Auth == null)
        {
            Debug.LogError("[FirebaseManager] LoginUser called before Firebase is ready.");
            OnLoginFailed?.Invoke("Firebase not ready.");
            return;
        }

        Debug.Log("[FirebaseManager] LoginUser() with email: " + email);

        Auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[FirebaseManager] LoginUser was canceled.");
                    OnLoginFailed?.Invoke("Login canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("[FirebaseManager] LoginUser error: " + task.Exception);
                    OnLoginFailed?.Invoke(task.Exception != null ? task.Exception.Message : "Unknown error.");
                    return;
                }

                User = task.Result.User;
                Debug.Log("[FirebaseManager] ✅ Logged in: " + User.UserId);
                OnLoginCompleted?.Invoke();
            });
    }

    // ===========================
    // PANGOLIN DATA
    // ===========================

    /// <summary>
    /// Creates default pangolin data at /pangolins/{userId}
    /// </summary>
    public void CreatePangolinProfile()
    {
        if (User == null)
        {
            Debug.LogWarning("[FirebaseManager] Cannot create pangolin: User is null.");
            return;
        }

        if (DbRef == null)
        {
            Debug.LogError("[FirebaseManager] DbRef is null, cannot write pangolin data.");
            return;
        }

        var pangolinData = new Dictionary<string, object>
        {
            { "growthStage", "baby" },
            { "happiness", 50 },
            { "hunger", 100 },
            { "lastFed", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ") }
        };

        DbRef.Child("pangolins").Child(User.UserId)
            .SetValueAsync(pangolinData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[FirebaseManager] CreatePangolinProfile canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("[FirebaseManager] CreatePangolinProfile error: " + task.Exception);
                    return;
                }

                Debug.Log("[FirebaseManager] ✅ Pangolin profile created for UID: " + User.UserId);
            });
    }

    /// <summary>
    /// Loads pangolin data for current user and returns in event.
    /// </summary>
    public void LoadPangolinData()
    {
        if (User == null)
        {
            Debug.LogWarning("[FirebaseManager] Cannot load pangolin: User is null.");
            OnPangolinDataLoadFailed?.Invoke("User is null.");
            return;
        }

        if (DbRef == null)
        {
            Debug.LogError("[FirebaseManager] DbRef is null, cannot read pangolin data.");
            OnPangolinDataLoadFailed?.Invoke("Database reference is null.");
            return;
        }

        DbRef.Child("pangolins").Child(User.UserId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("[FirebaseManager] LoadPangolinData canceled.");
                    OnPangolinDataLoadFailed?.Invoke("Canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("[FirebaseManager] LoadPangolinData error: " + task.Exception);
                    OnPangolinDataLoadFailed?.Invoke(task.Exception != null ? task.Exception.Message : "Unknown error.");
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogWarning("[FirebaseManager] No pangolin data exists for this user yet.");
                    OnPangolinDataLoadFailed?.Invoke("No pangolin data found.");
                    return;
                }

                var dict = new Dictionary<string, object>();

                foreach (var child in snapshot.Children)
                {
                    dict[child.Key] = child.Value;
                }

                Debug.Log("[FirebaseManager] ✅ Pangolin data loaded for UID: " + User.UserId);
                OnPangolinDataLoaded?.Invoke(dict);
            });
    }

    // ===========================
    // SIGN OUT
    // ===========================

    public void SignOut()
    {
        if (Auth != null && Auth.CurrentUser != null)
        {
            Debug.Log("[FirebaseManager] Signing out user: " + Auth.CurrentUser.UserId);
            Auth.SignOut();
            User = null;
            Debug.Log("[FirebaseManager] 👋 User signed out.");
        }
        else
        {
            Debug.Log("[FirebaseManager] SignOut called but no user is signed in.");
        }
    }
}
