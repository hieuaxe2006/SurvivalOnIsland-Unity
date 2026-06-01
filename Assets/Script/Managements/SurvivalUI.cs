using UnityEngine;
using UnityEngine.UI;

public class SurvivalUI : MonoBehaviour
{
    [Header("UI Images (Filled Type)")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image hungerBarImage;
    [SerializeField] private Image thirstBarImage;

    private void Start()
    {
        if (healthBarImage == null) Debug.LogWarning("[SurvivalUI] Missing healthBarImage reference!", this);
        if (hungerBarImage == null) Debug.LogWarning("[SurvivalUI] Missing hungerBarImage reference!", this);
        if (thirstBarImage == null) Debug.LogWarning("[SurvivalUI] Missing thirstBarImage reference!", this);
    }

    private void Update()
    {
        if (SurvivalStats.Instance == null) return;

        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = SurvivalStats.Instance.HealthPercent;
        }

        if (hungerBarImage != null)
        {
            hungerBarImage.fillAmount = SurvivalStats.Instance.HungerPercent;
        }

        if (thirstBarImage != null)
        {
            thirstBarImage.fillAmount = SurvivalStats.Instance.ThirstPercent;
        }
    }
}
