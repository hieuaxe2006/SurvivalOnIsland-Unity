using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private GameObject inventoryBG; // Inventory UI panel
    private bool isOpenInventory;

    [SerializeField] private List<GameObject> slotList = new List<GameObject>(); // List of slot GameObjects
    [SerializeField] private List<ItemData> allItems; // Database of all ItemData ScriptableObjects

    [SerializeField] private GameObject itemToAdd;
    [SerializeField] private GameObject slotWillContainItem;

    private InputAction inventoryAction;
    private PlayerInput playerInput;

    public System.Action OnInventoryChanged; // Event triggered whenever the inventory changes

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Set up and enable the new input system action for inventory toggle
        playerInput = GetComponent<PlayerInput>();
        inventoryAction = playerInput.actions["inventoryToggle"];
        inventoryAction.Enable();

        // Close inventory initially
        isOpenInventory = false;
        inventoryBG.SetActive(isOpenInventory);

        InputSlotList();
    }

    // Gathers all slots inside inventoryBG, filtering out crafting-related slots
    public void InputSlotList()
    {
        slotList.Clear();
        SlotGetItem[] slots = inventoryBG.GetComponentsInChildren<SlotGetItem>(true); // Get slots including inactive ones

        foreach (SlotGetItem slot in slots)
        {
            // Skip slots in crafting
            if (IsCraftingSlot(slot.transform))
            {
                continue;
            }
            slotList.Add(slot.gameObject);
        }
    }

    // Helper to check if a UI slot belongs to any crafting panel in the hierarchy
    private bool IsCraftingSlot(Transform t)
    {
        Transform current = t;
        while (current != null && current != inventoryBG.transform)
        {
            string nameLower = current.name.ToLower();
            if (nameLower.Contains("craft") || nameLower.Contains("recipe") || nameLower.Contains("material") || nameLower.Contains("product"))
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    private void Update()
    {
        // Toggle inventory
        if (inventoryAction.triggered)
        {
            isOpenInventory = !isOpenInventory;
            inventoryBG.SetActive(isOpenInventory);

            // Handle cursor 
            if (isOpenInventory)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // Add an item to the inventory (handles stacking and new slots)
    public void addItem(ItemData itemData, int amount = 1)
    {
        // 1. Try to find an existing stack of the same item that is not full
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                InventoryItem itemInSlot = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                //if slot same item and not full
                if (itemInSlot != null && itemInSlot.itemData == itemData && itemInSlot.amount < itemData.maxStack)
                {
                    int spaceLeft = itemData.maxStack - itemInSlot.amount;
                    int toAdd = Mathf.Min(amount, spaceLeft);
                    
                    itemInSlot.AddAmount(toAdd);
                    amount -= toAdd;

                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return;
                    }
                }
            }
        }

        // 2. Create new item stacks if slot space is available
        while (amount > 0)
        {
            slotWillContainItem = FindEmptySlot();
            if (slotWillContainItem == null)
            {
                Debug.LogWarning("No empty slot available for remaining items: " + itemData.itemName);
                break;
            }

            // Load UI slot prefab dynamically from Resources folder
            GameObject prefab = Resources.Load<GameObject>(itemData.itemName);
            if (prefab == null)
            {
                Debug.LogError("Failed to load item UI prefab from Resources: " + itemData.itemName);
                break;
            }

            itemToAdd = Instantiate(prefab, slotWillContainItem.transform);
            itemToAdd.name = itemData.itemName; // Keep clean name

            InventoryItem invItem = itemToAdd.GetComponent<InventoryItem>();
            if (invItem == null) invItem = itemToAdd.AddComponent<InventoryItem>();

            int toAdd = Mathf.Min(amount, itemData.maxStack);
            invItem.Initialize(itemData, toAdd);
            amount -= toAdd;

            // Reset anchored position to center within the slot
            RectTransform rt = itemToAdd.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            itemToAdd.transform.localScale = Vector3.one;
        }

        OnInventoryChanged?.Invoke();
    }

    // Find the first empty slot
    public GameObject FindEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }

    // Check if the inventory slots are fully packed
    public bool CheckFullSlot()
    {
        if (slotList.Count == 0)
        {
            // Auto nạp lại slot để phòng tránh lỗi nạp dữ liệu rỗng
            InputSlotList();
            if (slotList.Count == 0)
            {
                Debug.LogWarning("[InventoryManager] slotList is still empty! Allowing collection to prevent gameplay blocking.");
                return false;
            }
        }

        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount == 0)
            {
                return false;
            }
        }
        return true;
    }

    // Fetch ItemData scriptable object by name (with safe Resources fallbacks)
    public ItemData GetItemDataByName(string itemName)
    {
        // 1. Search inside the Inspector serialized list
        if (allItems != null)
        {
            foreach (ItemData item in allItems)
            {
                if (item != null && item.itemName == itemName)
                    return item;
            }
        }

        // 2. Resources fallbacks if not found in list
        ItemData loadedItem = Resources.Load<ItemData>(itemName);
        if (loadedItem != null) return loadedItem;

        loadedItem = Resources.Load<ItemData>("Data/" + itemName);
        if (loadedItem != null) return loadedItem;

        loadedItem = Resources.Load<ItemData>("Items/" + itemName);
        if (loadedItem != null) return loadedItem;

        return null;
    }

    // Get the total quantity of a specific item inside the inventory
    public int GetItemCount(string itemName)
    {
        int count = 0;
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                if (item != null && item.itemData.itemName == itemName)
                {
                    count += item.amount;
                }
            }
        }
        return count;
    }

    // Remove a quantity of an item from the inventory (searches slots backwards)
    public bool RemoveItem(string itemName, int count)
    {
        int toRemove = count;
        for (int i = slotList.Count - 1; i >= 0; i--)
        {
            GameObject slot = slotList[i];
            if (slot.transform.childCount > 0)
            {
                InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                if (item != null && item.itemData.itemName == itemName)
                {
                    if (item.amount > toRemove)
                    {
                        item.amount -= toRemove;
                        item.RefreshUI();
                        toRemove = 0;
                        break;
                    }
                    else
                    {
                        toRemove -= item.amount;
                        item.amount = 0;
                        Destroy(slot.transform.GetChild(0).gameObject);
                    }
                }
            }
            if (toRemove <= 0) break;
        }

        if (toRemove < count)
            OnInventoryChanged?.Invoke();

        return toRemove <= 0;
    }

    public List<GameObject> GetSlotList()
    {
        return slotList;
    }

    // Clear all inventory slots
    public void ClearInventory()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount > 0)
            {
                for (int i = slot.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(slot.transform.GetChild(i).gameObject);
                }
            }
        }
        OnInventoryChanged?.Invoke();
    }

#if UNITY_EDITOR
    // Automatically find and populate all ItemData assets
    private void OnValidate()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
        if (guids != null && guids.Length > 0)
        {
            allItems = new List<ItemData>();
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null && !allItems.Contains(item))
                {
                    allItems.Add(item);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
