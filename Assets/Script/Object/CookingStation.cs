using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CookingStation : MonoBehaviour
{
    private bool isPlayerNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("Ban dang o gan lua trai. Cam thit song (RawMeat) va an E de nuong!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    private void Update()
    {
        bool isEPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        if (isPlayerNear && isEPressed)
        {
            // Kiem tra xem co dang cam RawMeat khong
            if (EquipManager.Instance != null && EquipManager.Instance.currentEquipped != null)
            {
                ItemData holdingItem = EquipManager.Instance.currentEquipped;
                if (holdingItem.itemName == "RawMeat")
                {
                    // Convert thit song thanh thit chin
                    CookMeat(holdingItem);
                }
                else
                {
                    Debug.Log("Ban can cam RawMeat tren tay de nuong!");
                }
            }
        }
    }

    private void CookMeat(ItemData rawMeatData)
    {
        // Kiem tra inventory xem co item "CookedMeat" khong
        ItemData cookedMeatData = InventoryManager.Instance.GetItemDataByName("CookedMeat");
        
        if (cookedMeatData != null)
        {
            // Xoa 1 thit song (neu remove thanh cong)
            if (InventoryManager.Instance.RemoveItem(rawMeatData.itemName, 1))
            {
                // Neu vuot qua so luong sau khi xoa (Item cuoi cung) thi unequip
                if (InventoryManager.Instance.GetItemCount(rawMeatData.itemName) <= 0)
                {
                    EquipManager.Instance.Unequip();
                }

                // Them thit chin vao tui
                InventoryManager.Instance.addItem(cookedMeatData, 1);
                Debug.Log("Da nuong xong! Nhan duoc 1 CookedMeat.");
                
                // Play sound/particle if you have
            }
        }
        else
        {
            Debug.LogWarning("Chua co ItemData cho CookedMeat trong InventoryManager.allItems!");
        }
    }
}
