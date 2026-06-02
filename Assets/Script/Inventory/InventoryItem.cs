using UnityEngine;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    public ItemData itemData;
    public int amount;
    public TMP_Text amountText;

    // Initializes the inventory item with the given item data and amount, then updates the UI
    public void Initialize(ItemData data, int initialAmount)
    {
        itemData = data;
        amount = initialAmount;
        RefreshUI();
    }
    //add amount to the inventory item and refresh the UI to reflect the new quantity
    public void AddAmount(int value)
    {
        amount += value;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (amountText != null)
        {
            if (amount > 1)
            {
                amountText.text = amount.ToString();
                amountText.gameObject.SetActive(true);//show amount if >2 
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }
}
