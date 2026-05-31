using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private GameObject inventoryBG;
    private bool isOpenInventory;
    //create list to save slots
    [SerializeField] private List<GameObject> slotList = new List<GameObject>();
    [SerializeField] private List<ItemData> allItems;

    [SerializeField] private GameObject itemToAdd;
    [SerializeField] private GameObject slotWillContainItem;

    private InputAction inventoryAction;
    private PlayerInput playerInput;

    // Event thong bao khi inventory thay doi (cho crafting UI cap nhat)
    public System.Action OnInventoryChanged;
    //singleton
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //get playerinput
        playerInput = GetComponent<PlayerInput>();
        inventoryAction = playerInput.actions["inventoryToggle"];
        inventoryAction.Enable();

        //tat inventory luc dau
        isOpenInventory = false;
        inventoryBG.SetActive(isOpenInventory);

        InputSlotList();//get slot begin
    }
    public void InputSlotList()
    {
        slotList.Clear();//clear old list
        //get new list
        SlotGetItem[] slots = inventoryBG.GetComponentsInChildren<SlotGetItem>();

        foreach (SlotGetItem slot in slots)//loop all slot
        {
            slotList.Add(slot.gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (inventoryAction.triggered)//triggered = getkeydown(is set = tab in unity)
        {
            isOpenInventory = !isOpenInventory;//hoan doi active
            inventoryBG.SetActive(isOpenInventory);//T/F follow isOpenInventory
            //set mouse
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
    //func add item
    public void addItem(ItemData itemData, int amount = 1)
    {
        // Try to find an existing stack of the same item that is not full
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)//if slot has item
            {
                //choose this slot
                InventoryItem itemInSlot = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                //if same item and not full
                if (itemInSlot != null && itemInSlot.itemData == itemData && itemInSlot.amount < itemData.maxStack)
                {
                    //check space left
                    int spaceLeft = itemData.maxStack - itemInSlot.amount;
                    // tinh so luong cho phep add vao slot nay(max space left or amount)
                    int toAdd = Mathf.Min(amount, spaceLeft);
                    //add vao stack
                    itemInSlot.AddAmount(toAdd);
                    //reduce amount
                    amount -= toAdd;
                    //if amount = 0 , exit loop
                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return;
                    }
                }
            }
        }

        // if still have amount, new stack
        while (amount > 0)
        {
            slotWillContainItem = FindEmptySlot();//call func find empty slot
            if (slotWillContainItem == null)//if no empty slot
            {
                Debug.LogWarning("No empty slot available for remaining items: " + itemData.itemName);
                break;//exit loop
            }
            //load item prefab
            GameObject prefab = Resources.Load<GameObject>(itemData.itemName);
            if (prefab == null)
            {
                Debug.LogError("Failed to load item from Resources: " + itemData.itemName);
                break;//exit loop
            }
            //add item to slot
            itemToAdd = Instantiate(prefab, slotWillContainItem.transform);
            itemToAdd.name = itemData.itemName; // Keep clean name(not clone number)
            //add inventory item component
            InventoryItem invItem = itemToAdd.GetComponent<InventoryItem>();
            if (invItem == null) invItem = itemToAdd.AddComponent<InventoryItem>();
            //tinh so luong cho phep add
            int toAdd = Mathf.Min(amount, itemData.maxStack);
            //init item
            invItem.Initialize(itemData, toAdd);
            //giam so luong
            amount -= toAdd;
            //set size, pos for item
            RectTransform rt = itemToAdd.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            itemToAdd.transform.localScale = Vector3.one;
        }

        OnInventoryChanged?.Invoke();
    }

    public GameObject FindEmptySlot()
    {
        //find slot have 0 child(empty)
        foreach(GameObject slot in slotList)
        {
            if(slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }

    public bool CheckFullSlot()
    {
        foreach (GameObject slot in slotList)
        {
            //if find empty slot -> return false (not full)
            if(slot.transform.childCount == 0)
            {
                return false;
            } 
        }
        return true;
    }

    public ItemData GetItemDataByName(string itemName)
    {
        foreach (ItemData item in allItems)
        {
            if (item.itemName == itemName)
                return item;
        }
        return null;
    }

    public int GetItemCount(string itemName)
    {
        int count = 0;
        foreach(GameObject slot in slotList)
        {
            //if slot have item
            if(slot.transform.childCount > 0)
            {
                //get item count
                InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                if(item != null && item.itemData.itemName == itemName)
                {
                    count += item.amount;
                }
            }
        }
        return count;
    }

    public bool RemoveItem(string itemName, int count)
    {
        int toRemove = count;
        // Search from last slot to first (usually better for usage)
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
                        item.amount = 0; // Fix loi dem sai so luong
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
}

