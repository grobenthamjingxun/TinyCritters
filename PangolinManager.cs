// PangolinManager.cs
using UnityEngine;
using Firebase.Database;
using System;
using System.Collections.Generic;

public class PangolinManager : MonoBehaviour
{

    public static PangolinManager Instance;

    [SerializeField] private int happiness = 50;
    [SerializeField] private int hunger = 50;
    [SerializeField] private string growthStage = "egg";

    // Public read-only access
    public int Happiness => happiness;
    public int Hunger => hunger;
    public string GrowthStage => growthStage;

    private DatabaseReference pangolinRef;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FirebaseManager.Instance.OnLoginCompleted += LoadPangolinData;
    }

    private void LoadPangolinData()
    {
        string playerId = FirebaseManager.Instance.User.UserId;
        pangolinRef = FirebaseManager.Instance.DbRef.Child("pangolins").Child(playerId);

        pangolinRef.GetValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError("Failed to load pangolin: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            int loadedHunger = snapshot.HasChild("hunger") ? Convert.ToInt32(snapshot.Child("hunger").Value) : 50;
            int loadedHappiness = snapshot.HasChild("happiness") ? Convert.ToInt32(snapshot.Child("happiness").Value) : 50;
            string loadedStage = snapshot.HasChild("growthStage") ? snapshot.Child("growthStage").Value.ToString() : "egg";

            // Apply loaded values
            hunger = loadedHunger;
            happiness = loadedHappiness;
            growthStage = loadedStage;

            // Update UI
            UIManager.Instance.UpdateUI(Hunger, Happiness, GrowthStage);
        });
    }

    public void ApplyReward(int hungerDelta, int happinessDelta)
    {
        hunger = Mathf.Clamp(hunger + hungerDelta, 0, 100);
        happiness = Mathf.Clamp(happiness + happinessDelta, 0, 100);
        UpdateGrowthStage();

        var updates = new Dictionary<string, object>
        {
            { "hunger", hunger },
            { "happiness", happiness },
            { "growthStage", growthStage }
        };

        pangolinRef?.UpdateChildrenAsync(updates);
        UIManager.Instance.UpdateUI(Hunger, Happiness, GrowthStage);
    }


    // Public method to feed — other scripts should call this!
    public void Feed(string food)
    {
        if (pangolinRef == null)
        {
            Debug.LogWarning("Pangolin not loaded yet!");
            return;
        }

        int hungerGain = 0;
        int happinessGain = 0;

        switch (food.ToLower())
        {
            case "Ant":
                hungerGain = 50;
                happinessGain = 25;
                break;
            case "Banana": // ← since you're using BananaFeed.cs
                hungerGain = 10;
                happinessGain = -100;
                break;
            case "Ball":
                hungerGain = -25;
                happinessGain = 50;
                break;
            default:
                Debug.LogWarning("Unknown food: " + food);
                return;
        }

        // Update local values (clamped)
        hunger = Mathf.Clamp(hunger + hungerGain, 0, 100);
        happiness = Mathf.Clamp(happiness + happinessGain, 0, 100);

        // Update growth stage
        UpdateGrowthStage();

        // Save to Firebase
        var updates = new Dictionary<string, object>
        {
            { "hunger", hunger },
            { "happiness", happiness },
            { "growthStage", growthStage },
            { "lastFed", DateTime.UtcNow.ToString("o") }
        };

        pangolinRef.UpdateChildrenAsync(updates);

        // Update UI
        UIManager.Instance.UpdateUI(Hunger, Happiness, GrowthStage);
    }

    private void UpdateGrowthStage()
    {
        int total = happiness + hunger;
        growthStage = total >= 160 ? "adult" :
                      total >= 120 ? "teen" :
                      total >= 70  ? "baby" :
                      "egg";
    }
}
