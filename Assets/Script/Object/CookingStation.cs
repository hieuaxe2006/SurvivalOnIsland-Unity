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
            Debug.Log("You are near the campfire. Hold RawMeat and press E to cook!");
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
            // Check if holding RawMeat
            if (EquipManager.Instance != null && EquipManager.Instance.currentEquipped != null)
            {
                ItemData holdingItem = EquipManager.Instance.currentEquipped;
                if (holdingItem.itemName == "RawMeat")
                {
                    CookMeat(holdingItem);
                }
                else
                {
                    Debug.Log("You need to hold RawMeat to cook!");
                }
            }
        }
    }

    private void CookMeat(ItemData rawMeatData)
    {
        ItemData cookedMeatData = InventoryManager.Instance.GetItemDataByName("CookedMeat");

        if (cookedMeatData != null)
        {
            // Remove 1 raw meat
            if (InventoryManager.Instance.RemoveItem(rawMeatData.itemName, 1))
            {
                // Unequip if last raw meat was used
                if (InventoryManager.Instance.GetItemCount(rawMeatData.itemName) <= 0)
                {
                    EquipManager.Instance.Unequip();
                }

                // Add cooked meat to inventory
                InventoryManager.Instance.addItem(cookedMeatData, 1);
                Debug.Log("Cooking done! Received 1 CookedMeat.");
            }
        }
        else
        {
            Debug.LogWarning("No ItemData found for CookedMeat in InventoryManager.allItems!");
        }
    }
}
