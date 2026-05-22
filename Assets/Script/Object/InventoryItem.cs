using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    public ItemData itemData;
    public int amount;
    public TMP_Text amountText;
    //init data
    public void Initialize(ItemData data, int initialAmount)
    {
        itemData = data;
        amount = initialAmount;
        RefreshUI();
    }

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
                amountText.text = amount.ToString();//show amount if > 1
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.gameObject.SetActive(false);//hide amount if = 1
            }
        }
    }
}
