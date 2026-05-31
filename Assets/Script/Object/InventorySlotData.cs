using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public ItemData itemData;
    public int amount;

    public InventorySlotData(ItemData itemData, int amount)
    {
        this.itemData = itemData;
        this.amount = amount;
    }
}
