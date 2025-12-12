using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text playerGrowthText;
    public TMP_Text inventoryText;
    public TMP_Text lastFedText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateUI(int hunger, int happiness, string growthStage)
    {

    }

    public void ShowToast(string message)
    {
        Debug.Log("TOAST: " + message);
    }
}
