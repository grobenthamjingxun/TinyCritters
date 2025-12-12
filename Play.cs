using UnityEngine;

public class Play : MonoBehaviour
{
    public PangolinManager pangolinHappiness;
    public PangolinManager pangolinHunger;

    public string targetTag = "Pangolin"; // Tag of your animal

    public Color glowColor = Color.cyan;   // You can change this in Inspector

    public float glowIntensity = 2f;        // Adjust glow strength

    private void OnTriggerEnter(Collider collision)
    {
        // Check if the collided object is the animal
        if (collision.gameObject.CompareTag(targetTag))
        {
            Debug.Log("collided");
            Renderer petRenderer = collision.gameObject.GetComponent<Renderer>();

            if (petRenderer != null)
            {
                // Enable emission keyword
                petRenderer.material.EnableKeyword("_EMISSION");

                // Set glow color * intensity
                petRenderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);
            }
            PangolinManager.Instance.ApplyReward(10, 5);

        }

            // Call Firebase to update player growth
            if (PangolinManager.Instance != null)
            {
                PangolinManager.Instance.Feed("Ball"); // Or the item name
            }
            else
            {
                Debug.LogWarning("PangolinManager not found. Growth will not sync to Firebase.");
            }

            // Destroy the ball after paly
            Destroy(gameObject);
        
        
    }
}