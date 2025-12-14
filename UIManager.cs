using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Stat Sliders")]
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider happinessSlider;

    [Header("Growth Stage (RawImage)")]
    [SerializeField] private RawImage growthStageRawImage;
    [SerializeField] private Texture2D eggTexture;
    [SerializeField] private Texture2D babyTexture;
    [SerializeField] private Texture2D juvenileTexture;
    [SerializeField] private Texture2D adultTexture;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateUI(int hunger, int happiness, string growthStage)
    {
        if (hungerSlider != null)
            hungerSlider.value = hunger;

        if (happinessSlider != null)
            happinessSlider.value = happiness;

        UpdateGrowthStageTexture(growthStage);
    }

    private void UpdateGrowthStageTexture(string stage)
    {
        if (growthStageRawImage == null) return;

        switch (stage.ToLower())
        {
            case "egg":
                growthStageRawImage.texture = eggTexture;
                break;
            case "baby":
                growthStageRawImage.texture = babyTexture;
                break;
            case "teen":
                growthStageRawImage.texture = juvenileTexture;
                break;
            case "adult":
                growthStageRawImage.texture = adultTexture;
                break;
        }
    }
}
