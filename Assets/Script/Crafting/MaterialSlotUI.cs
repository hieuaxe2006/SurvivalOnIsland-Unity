using UnityEngine;
using TMPro;

public class MaterialSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private CanvasGroup canvasGroup;

    /// <summary>Updates quantity text and color (green if sufficient, red if not).</summary>
    public void UpdateQuantity(int currentAmount, int requiredAmount)
    {
        quantityText.text = currentAmount + "/" + requiredAmount;

        if (currentAmount >= requiredAmount)
        {
            quantityText.color = Color.green;
        }
        else
        {
            quantityText.color = Color.red;
        }
    }

    /// <summary>Sets the opacity of this material slot.</summary>
    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
