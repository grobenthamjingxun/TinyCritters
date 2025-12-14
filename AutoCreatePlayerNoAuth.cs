using UnityEngine;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AutoCreatePlayerNoAuth : MonoBehaviour
{
    [Header("Database Root Node")]
    public string rootNode = "players"; // or "pangolins"

    [Header("Default Values")]
    public string defaultGrowthStage = "baby";
    public int defaultHunger = 50;
    public int defaultHappiness = 50;

    private bool _ran;

    private async void Start()
    {
        if (_ran) return;
        _ran = true;

        // 1️⃣ Wait for Firebase initialization
        await WaitForFirebaseReady();

        // 2️⃣ Get or create a local player ID
        string playerId = GetOrCreateLocalPlayerId();

        // 3️⃣ Ensure profile exists in RTDB
        await EnsureProfileExistsAsync(playerId);
    }

    private async Task WaitForFirebaseReady()
    {
        float start = Time.realtimeSinceStartup;

        while (!FirebaseInitializer.IsFirebaseReady)
        {
            if (Time.realtimeSinceStartup - start > 10f)
            {
                Debug.LogWarning("[AutoCreatePlayerNoAuth] Firebase not ready after 10s, continuing anyway.");
                break;
            }
            await Task.Yield();
        }
    }

    private string GetOrCreateLocalPlayerId()
    {
        if (PlayerPrefs.HasKey("PLAYER_ID"))
            return PlayerPrefs.GetString("PLAYER_ID");

        // Prefer device ID, fallback to GUID
        string id = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrEmpty(id) || id == "unknown")
            id = Guid.NewGuid().ToString();

        PlayerPrefs.SetString("PLAYER_ID", id);
        PlayerPrefs.Save();

        Debug.Log("[AutoCreatePlayerNoAuth] Created local player ID: " + id);
        return id;
    }

    private async Task EnsureProfileExistsAsync(string playerId)
    {
        var db = FirebaseDatabase.DefaultInstance;
        var node = db.RootReference.Child(rootNode).Child(playerId);

        try
        {
            var snapshot = await node.GetValueAsync();

            if (snapshot != null && snapshot.Exists)
            {
                Debug.Log($"[AutoCreatePlayerNoAuth] Profile exists at {rootNode}/{playerId}");
                return;
            }

            Debug.Log($"[AutoCreatePlayerNoAuth] No profile found. Creating new profile for {playerId}");

            var profile = new Dictionary<string, object>
            {
                { "growthStage", defaultGrowthStage },
                { "hunger", defaultHunger },
                { "happiness", defaultHappiness },
                { "createdAtUtc", DateTime.UtcNow.ToString("o") },

                // Optional defaults for your feeding system
                { "lastFedItem", "" },
                { "lastFedAtUtc", "" },
                { "lastFedHungerDelta", 0 },
                { "lastFedHappinessDelta", 0 }
            };

            await node.SetValueAsync(profile);

            Debug.Log($"[AutoCreatePlayerNoAuth] ✅ Profile created at {rootNode}/{playerId}");
        }
        catch (Exception e)
        {
            Debug.LogError("[AutoCreatePlayerNoAuth] Database error: " + e);
        }
    }
}
