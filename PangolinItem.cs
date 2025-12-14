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
        if (used || !other.CompareTag(pangolinTag)) return;
        used = true;

        int hungerDelta = 0;
        int happinessDelta = 0;

        switch (itemType)
        {
            case ItemType.Ant:
                hungerDelta = 25;
                happinessDelta = 20;
                PangolinManager.Instance.AddScale(feedScaleAdd);
                break;

            case ItemType.Banana:
                hungerDelta = 15;
                happinessDelta = -10;
                PangolinManager.Instance.AddScale(feedScaleAdd);
                break;

            case ItemType.Ball:
                happinessDelta = 25;
                PangolinManager.Instance.PulseGlow(glowColor, glowIntensity, glowDuration);
                break;
        }

        // 🔥 ALWAYS LOG ITEM
        PangolinManager.Instance.ApplyItem(
            itemType.ToString().ToLower(),
            hungerDelta,
            happinessDelta
        );

        Destroy(gameObject);
    }
}
