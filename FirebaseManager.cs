using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }
    public DatabaseReference DbRef { get; private set; }

    public bool IsReady { get; private set; }

    // Events for UI to subscribe to
    public event Action OnFirebaseReady;

    public event Action OnLoginCompleted;
    public event Action<string> OnLoginFailed;
    public event Action OnSignUpCompleted;
    public event Action<string> OnSignUpFailed;

    public event Action<Dictionary<string, object>> OnPangolinDataLoaded;
    public event Action<string> OnPangolinDataLoadFailed;

    private bool _initialising;

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
        if (_initialising || IsReady) return;
        _initialising = true;

        Debug.Log("[FirebaseManager] Checking Firebase dependencies...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            _initialising = false;

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

        Auth.StateChanged += OnAuthStateChanged;
        OnAuthStateChanged(this, null);

        IsReady = true;
        Debug.Log("[FirebaseManager] Firebase initialised. Auth + Realtime Database ready.");

        OnFirebaseReady?.Invoke();
    }

    private void OnDestroy()
    {
        if (Auth != null)
            Auth.StateChanged -= OnAuthStateChanged;

        if (Instance == this)
            Instance = null;
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (Auth == null) return;

        if (Auth.CurrentUser != User)
        {
            bool signedIn = Auth.CurrentUser != null;

            if (!signedIn && User != null)
                Debug.Log("[FirebaseManager] Signed out: " + User.UserId);

            User = Auth.CurrentUser;

            if (signedIn)
                Debug.Log("[FirebaseManager] Signed in: " + User.UserId);
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
            .ContinueWithOnMainThread(async task =>
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

                // Ensure profile exists (for new users it will create it)
                try
                {
                    await EnsurePangolinProfileExistsAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[FirebaseManager] EnsurePangolinProfileExistsAsync error: " + ex);
                }

                // IMPORTANT: don't fire OnLoginCompleted here (avoid scene load race)
                OnSignUpCompleted?.Invoke();
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
            .ContinueWithOnMainThread(async task =>
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

                // ✅ Create profile if missing
                try
                {
                    await EnsurePangolinProfileExistsAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[FirebaseManager] EnsurePangolinProfileExistsAsync error: " + ex);
                    // We can still continue, but data load may fail if DB is unreachable.
                }

                OnLoginCompleted?.Invoke();
            });
    }

    // ===========================
    // PLAYER / PANGOLIN PROFILE
    // ===========================

    private async Task EnsurePangolinProfileExistsAsync()
    {
        if (User == null)
        {
            Debug.LogWarning("[FirebaseManager] Ensure profile: User is null.");
            return;
        }

        if (DbRef == null)
        {
            Debug.LogError("[FirebaseManager] Ensure profile: DbRef is null.");
            return;
        }

        var pangolinNode = DbRef.Child("pangolins").Child(User.UserId);
        DataSnapshot snapshot = await pangolinNode.GetValueAsync();

        if (snapshot == null || !snapshot.Exists)
        {
            Debug.Log("[FirebaseManager] No pangolin profile found. Creating default profile...");
            await CreatePangolinProfileAsync();
        }
        else
        {
            Debug.Log("[FirebaseManager] Pangolin profile exists.");
        }
    }

    private async Task CreatePangolinProfileAsync()
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
            { "hunger", 50 },

            // Use separate createdAt (better than setting lastFed immediately)
            { "createdAtUtc", DateTime.UtcNow.ToString("o") },

            // Optional defaults so your UI/logic never sees missing keys:
            { "lastFedAtUtc", "" },
            { "lastFedItem", "" },
            { "lastFedHungerDelta", 0 },
            { "lastFedHappinessDelta", 0 }
        };

        await DbRef.Child("pangolins").Child(User.UserId).SetValueAsync(pangolinData);
        Debug.Log("[FirebaseManager] ✅ Pangolin profile created for UID: " + User.UserId);
    }

    // ===========================
    // LOAD PANGOLIN DATA
    // ===========================

    public void LoadPangolinData()
    {
        _ = LoadPangolinDataAsync();
    }

    private async Task LoadPangolinDataAsync()
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

        // ✅ Safety: if profile is missing, create it first
        try
        {
            await EnsurePangolinProfileExistsAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("[FirebaseManager] Ensure profile before load error: " + ex);
        }

        try
        {
            DataSnapshot snapshot = await DbRef.Child("pangolins").Child(User.UserId).GetValueAsync();

            if (snapshot == null || !snapshot.Exists)
            {
                Debug.LogWarning("[FirebaseManager] No pangolin data exists for this user yet (even after ensure).");
                OnPangolinDataLoadFailed?.Invoke("No pangolin data found.");
                return;
            }

            var dict = new Dictionary<string, object>();
            foreach (var child in snapshot.Children)
                dict[child.Key] = child.Value;

            Debug.Log("[FirebaseManager] ✅ Pangolin data loaded for UID: " + User.UserId);
            OnPangolinDataLoaded?.Invoke(dict);
        }
        catch (Exception ex)
        {
            Debug.LogError("[FirebaseManager] LoadPangolinData exception: " + ex);
            OnPangolinDataLoadFailed?.Invoke(ex.Message);
        }
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
