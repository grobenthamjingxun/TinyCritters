using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class EnsurePlayerProfile : MonoBehaviour
{
    [Header("Database Path")]
    [Tooltip("Where you store player data. If your game uses pangolins/<uid>, leave as 'pangolins'.")]
    public string rootNode = "pangolins"; // or "players"

    [Header("Default Values")]
    public string defaultGrowthStage = "baby";
    public int defaultHunger = 50;
    public int defaultHappiness = 50;

    [Header("Behaviour")]
    [Tooltip("If true, will keep waiting a bit for a signed-in user before giving up.")]
    public bool waitForUser = true;

    [Tooltip("Seconds to wait for Auth.CurrentUser (only used if waitForUser is true).")]
    public float userWaitTimeoutSeconds = 10f;

    private bool _ran;

    private async void Start()
    {
        if (_ran) return;
        _ran = true;

        // 1) Wait for Firebase to be initialized (your FirebaseInitializer sets this flag)
        await WaitForFirebaseReady();

        // 2) Get current user (wait briefly if needed)
        var user = await GetUserOrWait();
        if (user == null)
        {
            Debug.LogWarning("[EnsurePlayerProfile] No signed-in user found. Profile not created.");
            return;
        }

        // 3) Ensure profile exists
        await EnsureProfileExistsAsync(user.UserId);
    }

    private async Task WaitForFirebaseReady()
    {
        // If you're using FirebaseInitializer.IsFirebaseReady, wait for it.
        // If you don't have that in the scene, this will just proceed immediately.
        float start = Time.realtimeSinceStartup;

        while (true)
        {
            // If FirebaseInitializer exists in project, use it
            // (This compiles as long as FirebaseInitializer class exists)
            if (FirebaseInitializer.IsFirebaseReady)
                return;

            // If it never becomes ready, don't hang forever; just proceed after a bit.
            if (Time.realtimeSinceStartup - start > 10f)
            {
                Debug.LogWarning("[EnsurePlayerProfile] FirebaseInitializer.IsFirebaseReady still false after 10s. Proceeding anyway.");
                return;
            }

            await Task.Yield();
        }
    }

    private async Task<FirebaseUser> GetUserOrWait()
    {
        var auth = FirebaseAuth.DefaultInstance;
        if (auth == null) return null;

        if (!waitForUser)
            return auth.CurrentUser;

        float start = Time.realtimeSinceStartup;
        while (auth.CurrentUser == null && (Time.realtimeSinceStartup - start) < userWaitTimeoutSeconds)
        {
            await Task.Yield();
        }

        return auth.CurrentUser;
    }

    private async Task EnsureProfileExistsAsync(string uid)
    {
        var db = FirebaseDatabase.DefaultInstance;
        var node = db.RootReference.Child(rootNode).Child(uid);

        try
        {
            var snap = await node.GetValueAsync();

            if (snap != null && snap.Exists)
            {
                Debug.Log($"[EnsurePlayerProfile] Profile exists at {rootNode}/{uid}");
                return;
            }

            Debug.Log($"[EnsurePlayerProfile] No profile found at {rootNode}/{uid}. Creating default profile...");

            var profile = new Dictionary<string, object>
            {
                { "growthStage", defaultGrowthStage },
                { "hunger", defaultHunger },
                { "happiness", defaultHappiness },
                { "createdAtUtc", DateTime.UtcNow.ToString("o") },

                // Optional: useful default tracking fields so nothing is missing later
                { "lastFedAtUtc", "" },
                { "lastFedItem", "" },
                { "lastFedHungerDelta", 0 },
                { "lastFedHappinessDelta", 0 }
            };

            await node.SetValueAsync(profile);

            Debug.Log($"[EnsurePlayerProfile] ✅ Created profile at {rootNode}/{uid}");
        }
        catch (Exception e)
        {
            Debug.LogError("[EnsurePlayerProfile] EnsureProfileExistsAsync error: " + e);
        }
    }
}
