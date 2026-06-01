using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingRecipeUI : MonoBehaviour
{
    [Header("Recipe Data")]
    [SerializeField] private BluePrint bluePrint;

    [Header("Material Slots")]
    [SerializeField] private List<MaterialSlotUI> materialSlots;

    [Header("Product")]
    [SerializeField] private CanvasGroup productCanvasGroup;
    [SerializeField] private Button productButton;

    private void Start()
    {
        // Bind product click event
        if (productButton != null)
        {
            productButton.onClick.AddListener(OnProductClicked);
        }

        // Listen for inventory changes to update UI
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateUI;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe event on destroy
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateUI;
        }
    }

    /// <summary>Updates the entire recipe row UI.</summary>
    public void UpdateUI()
    {
        if (bluePrint == null || InventoryManager.Instance == null) return;

        bool canCraft = true;

        // Update each material slot
        for (int i = 0; i < bluePrint.materials.Count; i++)
        {
            if (i >= materialSlots.Count) break;

            MaterialRequirement requirement = bluePrint.materials[i];
            int currentCount = InventoryManager.Instance.GetItemCount(requirement.item.itemName);

            // Update quantity text and color
            materialSlots[i].UpdateQuantity(currentCount, requirement.quantity);

            if (currentCount < requirement.quantity)
            {
                canCraft = false;
            }
        }

        // Update alpha for all material slots in this row
        float alpha = canCraft ? 1f : 0.5f;
        foreach (MaterialSlotUI slot in materialSlots)
        {
            slot.SetAlpha(alpha);
        }

        // Update product slot
        if (productCanvasGroup != null)
        {
            productCanvasGroup.alpha = alpha;
        }
        if (productButton != null)
        {
            productButton.interactable = canCraft;
        }
    }

    private void OnProductClicked()
    {
        if (bluePrint == null || InventoryManager.Instance == null) return;

        // Check material amounts
        foreach (MaterialRequirement requirement in bluePrint.materials)
        {
            int currentCount = InventoryManager.Instance.GetItemCount(requirement.item.itemName);
            if (currentCount < requirement.quantity)
            {
                Debug.LogWarning("Not enough materials to craft: " + bluePrint.blueprintName);
                return;
            }
        }

        // Check if inventory is full
        if (InventoryManager.Instance.CheckFullSlot())
        {
            Debug.LogWarning("Inventory is full! Cannot craft: " + bluePrint.blueprintName);
            return;
        }

        // Remove required materials
        foreach (MaterialRequirement requirement in bluePrint.materials)
        {
            InventoryManager.Instance.RemoveItem(requirement.item.itemName, requirement.quantity);
        }

        // Add crafted product
        InventoryManager.Instance.addItem(bluePrint.resultItem);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollect();
        }

        Debug.Log("Crafted: " + bluePrint.resultItem.itemName);
    }
}
