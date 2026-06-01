using UnityEngine;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    public ItemData itemData;
    public int amount;
    public TMP_Text amountText;

    /// <summary>Initializes the item with data and amount.</summary>
    public void Initialize(ItemData data, int initialAmount)
    {
        itemData = data;
        amount = initialAmount;
        RefreshUI();
    }

    /// <summary>Adds to the current stack amount.</summary>
    public void AddAmount(int value)
    {
        amount += value;
        RefreshUI();
    }

    /// <summary>Updates the amount text display.</summary>
    public void RefreshUI()
    {
        if (amountText != null)
        {
            if (amount > 1)
            {
                amountText.text = amount.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }
}
