using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public static EquipManager Instance;

    [SerializeField] private Transform handSlot; // Bone tay phai cua player model
    
    public ItemData currentEquipped { get; private set; }
    private GameObject currentModel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Equip(ItemData item)
    {
        // Neu dang cam cung 1 item thi Unequip
        if (currentEquipped == item)
        {
            Unequip();
            return;
        }

        // Destroy model cu
        Unequip();
        //debug
        if (item.prefab3D == null)
        {
            Debug.LogWarning("Item " + item.itemName + " does not have a prefab3D for equipping.");
            return;
        }
        if(item.itemName == "AirplanePart")
        {
            Debug.Log("Item" + item.itemName + " cant be equiped");
            return;
        }

        // Instantiate model moi vao tay
        currentModel = Instantiate(item.prefab3D, handSlot);
        //get value from each item
        currentModel.transform.localPosition = item.equipPos;
        currentModel.transform.localRotation = Quaternion.Euler(item.equipRot);
        
        // --- FIX BUG NHẶT ĐỒ TỪ TRÊN TAY ---
        // Vô hiệu hóa script InteractableObject và Collider để tia Raycast (tâm ngắm) 
        // không tự bắn trúng đồ vật đang cầm trên tay.
        InteractableObject interactObj = currentModel.GetComponent<InteractableObject>();
        if (interactObj != null) Destroy(interactObj);

        Collider[] colliders = currentModel.GetComponentsInChildren<Collider>();
        foreach(Collider col in colliders)
        {
            col.enabled = false;
        }
        // ------------------------------------

        currentEquipped = item;
        Debug.Log("Equipped: " + item.itemName);
        
        // Co the kich hoat animation o day
    }

    public void Unequip()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);//xoa item dang equip
        }
        currentEquipped = null;
        currentModel = null;
    }

    public int GetCurrentWeaponDamage()
    {
        if (currentEquipped != null)
        {
            return currentEquipped.damage;
        }
        return 0; 
    }
}
