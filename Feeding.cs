using UnityEngine;

public class Feeding : MonoBehaviour
{
    public PangolinManager pangolinHunger;
    
    public float growthAmount = 0.5f; // How much the animal grows per feed
    public string targetTag = "Pangolin"; // Tag of your animal

    private void OnTriggerEnter(Collider collision)
    {
        // Check if the collided object is the animal
        if (collision.gameObject.CompareTag(targetTag))
        {
            // Scale the animal locally
            collision.transform.localScale += new Vector3(growthAmount, growthAmount, growthAmount);

            // Call Firebase to update player growth
            if (PangolinManager.Instance != null)
            {
                PangolinManager.Instance.Feed("ant"); // Or the item name
            }
            else
            {
                Debug.LogWarning("PangolinManager not found. Growth will not sync to Firebase.");
            }

            PangolinManager.Instance.Feed("ant");

            // Destroy the ant after feeding
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Wrong object! Cannot feed this.");
        }
    }
}