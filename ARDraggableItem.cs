using UnityEngine;

public class ARDraggableItem : MonoBehaviour
{
    [Header("What is this item called in Firebase logs?")]
    [SerializeField] private string itemName = "draggable_item";

    [Header("Collision Target")]
    [SerializeField] private string targetTag = "Pangolin";

    [Header("Stat Effects")]
    [SerializeField] private int hungerDelta = 0;
    [SerializeField] private int happinessDelta = 0;

    [Header("Counts as feeding? (updates lastFed)")]
    [SerializeField] private bool setLastFed = false;

    [Header("One-time use")]
    [SerializeField] private bool destroyAfterUse = true;
    private bool used;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag(targetTag)) return;

        used = true;

        PangolinManager.Instance?.ApplyDelta(itemName, hungerDelta, happinessDelta, setLastFed);

        if (destroyAfterUse)
            Destroy(gameObject);
    }
}
