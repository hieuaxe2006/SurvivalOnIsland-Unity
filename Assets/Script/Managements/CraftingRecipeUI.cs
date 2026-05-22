using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CraftingRecipeUI : MonoBehaviour
{
    [Header("Recipe Data")]//header = heading
    [SerializeField] private BluePrint bluePrint; // cong thuc crafting

    [Header("Material Slots")]
    [SerializeField] private List<MaterialSlotUI> materialSlots; // cac o material

    [Header("Product")]
    [SerializeField] private CanvasGroup productCanvasGroup; // set alpha product slot
    [SerializeField] private Button productButton; // nut bam craft product

    private void Start()
    {
        // Gan su kien click product
        if (productButton != null)
        {
            productButton.onClick.AddListener(OnProductClicked);
        }

        // Lang nghe thay doi inventory de cap nhat UI
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += UpdateUI;
        }

        // Cap nhat UI lan dau
        UpdateUI();
    }

    private void OnDestroy()
    {
        // Huy event khi bi destroy
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateUI;
        }
    }

    /// Cap nhat toan bo UI cua recipe row nay
    public void UpdateUI()
    {
        if (bluePrint == null || InventoryManager.Instance == null) return;

        bool canCraft = true;

        // Cap nhat tung material slot
        for (int i = 0; i < bluePrint.materials.Count; i++)
        {
            //check tung slot material
            if (i >= materialSlots.Count) break;
            //get material amount da co
            MaterialRequirement requirement = bluePrint.materials[i];
            int currentCount = InventoryManager.Instance.GetItemCount(requirement.item.itemName);

            // Cap nhat text so luong va mau
            materialSlots[i].UpdateQuantity(currentCount, requirement.quantity);

            // Kiem tra du so luong chua
            if (currentCount < requirement.quantity)
            {
                canCraft = false;
            }
        }

        // Cap nhat alpha cho tat ca material slots trong row
        float alpha = canCraft ? 1f : 0.5f;
        foreach (MaterialSlotUI slot in materialSlots)
        {
            slot.SetAlpha(alpha);
        }

        // Cap nhat product
        if (productCanvasGroup != null)
        {
            productCanvasGroup.alpha = alpha;
        }
        if (productButton != null)
        {
            productButton.interactable = canCraft;
        }
    }

    /// Khi nguoi choi click vao product (collect)
    private void OnProductClicked()
    {
        if (bluePrint == null || InventoryManager.Instance == null) return;

        //check amount
        foreach (MaterialRequirement requirement in bluePrint.materials)
        {
            int currentCount = InventoryManager.Instance.GetItemCount(requirement.item.itemName);
            if (currentCount < requirement.quantity)
            {
                Debug.LogWarning("Not enough materials to craft: " + bluePrint.blueprintName);
                return;
            }
        }

        //amount enough -> check fullslot
        if (InventoryManager.Instance.CheckFullSlot())
        {
            Debug.LogWarning("Inventory is full! Cannot craft: " + bluePrint.blueprintName);
            return;
        }

        // has slot -> call removed func
        foreach (MaterialRequirement requirement in bluePrint.materials)
        {
            InventoryManager.Instance.RemoveItem(requirement.item.itemName, requirement.quantity);
        }

        // add product
        InventoryManager.Instance.addItem(bluePrint.resultItem);

        Debug.Log("Crafted: " + bluePrint.resultItem.itemName);
    }
}
