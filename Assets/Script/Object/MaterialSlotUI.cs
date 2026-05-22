using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText; // text hien thi amount item need
    [SerializeField] private CanvasGroup canvasGroup; // de chinh alpha

    /// Cap nhat text so luong va mau sac
    public void UpdateQuantity(int currentAmount, int requiredAmount)
    {
        // Hien thi "current/required"
        quantityText.text = currentAmount + "/" + requiredAmount;

        // Doi mau: xanh neu du, do neu thieu
        if (currentAmount >= requiredAmount)
        {
            quantityText.color = Color.green;
        }
        else
        {
            quantityText.color = Color.red;
        }
    }
    /// Chinh do mo cua o material
    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
