using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public static EquipManager Instance;

    [SerializeField] private Transform handSlot; // Player right-hand bone

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

    /// <summary>Equips an item, or unequips if already holding the same item.</summary>
    public void Equip(ItemData item)
    {
        // If already holding the same item, unequip it
        if (currentEquipped == item)
        {
            Unequip();
            return;
        }

        // Destroy previous model
        Unequip();

        if (item.prefab3D == null)
        {
            Debug.LogWarning("Item " + item.itemName + " does not have a prefab3D for equipping.");
            return;
        }
        if (item.itemName == "AirplanePart")
        {
            Debug.Log("Item " + item.itemName + " can't be equipped.");
            return;
        }

        // Instantiate new model in hand
        currentModel = Instantiate(item.prefab3D, handSlot);
        currentModel.transform.localPosition = item.equipPos;
        currentModel.transform.localRotation = Quaternion.Euler(item.equipRot);

        // Disable interaction on held item so raycast doesn't hit it
        InteractableObject interactObj = currentModel.GetComponent<InteractableObject>();
        if (interactObj != null) Destroy(interactObj);

        Collider[] colliders = currentModel.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        currentEquipped = item;
        Debug.Log("Equipped: " + item.itemName);
    }

    /// <summary>Unequips the currently held item.</summary>
    public void Unequip()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
        currentEquipped = null;
        currentModel = null;
    }

    /// <summary>Returns the damage value of the currently equipped weapon.</summary>
    public int GetCurrentWeaponDamage()
    {
        if (currentEquipped != null)
        {
            return currentEquipped.damage;
        }
        return 0;
    }
}
