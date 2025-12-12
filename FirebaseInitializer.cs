using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    // Optional: Expose an event or flag other scripts can check
    public static bool IsFirebaseReady { get; private set; } = false;

    private void Awake()
    {
        // Ensure only one FirebaseInitializer exists
        if (FindObjectsOfType<FirebaseInitializer>().Length > 1)
        {
            Debug.LogWarning("Duplicate FirebaseInitializer found. Destroying extra instance.");
            Destroy(gameObject);
            return;
        }

        // Keep this object alive when loading new scenes
        DontDestroyOnLoad(gameObject);

        // Start Firebase initialization
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        Debug.Log("Initializing Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Firebase dependency check failed: {task.Exception}");
                return;
            }

            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase is ready to use
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log($"Firebase successfully initialized! App name: {app.Name}");

                IsFirebaseReady = true;

                // Optional: Trigger scene load or enable UI here
                // Example: SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.LogError($"Firebase initialization failed: {dependencyStatus}");
            }
        });
    }
}