using UnityEngine;

public class PangolinItem : MonoBehaviour
{
    public enum ItemType { Ant, Banana, Ball }

    public ItemType itemType;
    public string pangolinTag = "Pangolin";
    public float feedScaleAdd = 0.03f;

    public Color glowColor = Color.cyan;
    public float glowIntensity = 3f;
    public float glowDuration = 0.4f;

    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (other == null || !other.CompareTag(pangolinTag)) return;

        if (PangolinManager.Instance == null)
        {
            Debug.LogWarning("[PangolinItem] No PangolinManager.Instance found.");
            return;
        }

        used = true;

        int hungerDelta = 0;
        int happinessDelta = 0;

        // ✅ NEW: countsAsFed determines whether we set lastFed / countsAsFed in logs
        bool countsAsFed = false;

        switch (itemType)
        {
            case ItemType.Ant:
                hungerDelta = 45;
                happinessDelta = 20;
                countsAsFed = true;
                PangolinManager.Instance.AddScale(feedScaleAdd);
                break;

            case ItemType.Banana:
                hungerDelta = 15;
                happinessDelta = -10;
                countsAsFed = true;
                PangolinManager.Instance.AddScale(feedScaleAdd);
                break;

            case ItemType.Ball:
                happinessDelta = 25;
                countsAsFed = false;
                PangolinManager.Instance.PulseGlow(glowColor, glowIntensity, glowDuration);
                break;
        }

        PangolinManager.Instance.ApplyItem(
            itemType.ToString().ToLower(),
            hungerDelta,
            happinessDelta,
            countsAsFed
        );

        Destroy(gameObject);
    }
}
