using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // for RawImage
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Fixed inventory slots in the Canvas")]
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called by PangolinManager whenever inventory changes.
    /// Shows / hides rows and updates text + textures.
    /// </summary>
    public void UpdateInventory(Dictionary<string, int> inventory)
{
    foreach (var slot in slots)
    {
        if (slot == null || string.IsNullOrEmpty(slot.itemName))
            continue;

        // Default: assume 0
        int count = 0;

        bool hasEntry = inventory != null && inventory.TryGetValue(slot.itemName, out count);
        bool hasItem = hasEntry && count > 0;

        // Control the root active state strictly
        if (slot.root != null)
        {
            slot.root.SetActive(hasItem);
        }

        if (!hasItem)
            continue;

        // Update label
        if (slot.label != null)
        {
            slot.label.text = $"{slot.itemName} x{count}";
        }

        // Update icon texture
        if (slot.icon != null && slot.iconTexture != null)
        {
            slot.icon.texture = slot.iconTexture;
        }
    }
}


}

[System.Serializable]
public class InventorySlot
{
    [Tooltip("Must match the itemName used in PangolinManager.AddItem(\"...\")")]
    public string itemName;   // e.g. "banana"

    [Tooltip("Root GameObject for this slot (row/panel)")]
    public GameObject root;

    [Tooltip("RawImage that will display the Texture2D")]
    public RawImage icon;

    [Tooltip("Texture shown for this item (drag your texture here)")]
    public Texture2D iconTexture;

    [Tooltip("Text label showing name + count")]
    public TMP_Text label;
}
