using UnityEngine;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    public static bool shouldLoadSave = false;

    private void Awake()
    {
        // Singleton pattern to keep this manager across scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Check if a saved game exists
    public bool HasSave()
    {
        return PlayerPrefs.GetInt("save_exists", 0) == 1;
    }

    // Save game progress to PlayerPrefs
    public void SaveGame()
    {
        Debug.Log("[SaveLoadManager] Saving game progress...");

        // 1. Mark save as existing
        PlayerPrefs.SetInt("save_exists", 1);

        // 2. Save Player Position
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            PlayerPrefs.SetFloat("player_pos_x", pos.x);
            PlayerPrefs.SetFloat("player_pos_y", pos.y);
            PlayerPrefs.SetFloat("player_pos_z", pos.z);
        }

        // 3. Save Player Stats
        if (SurvivalStats.Instance != null)
        {
            PlayerPrefs.SetFloat("player_health", SurvivalStats.Instance.CurrentHealth);
            PlayerPrefs.SetFloat("player_hunger", SurvivalStats.Instance.CurrentHunger);
            PlayerPrefs.SetFloat("player_thirst", SurvivalStats.Instance.CurrentThirst);
        }

        // 4. Save Quest State
        if (QuestManager.Instance != null)
        {
            PlayerPrefs.SetInt("quest_state", (int)QuestManager.Instance.currentState);
        }

        // 5. Save Inventory Items (Item name and stack amount)
        if (InventoryManager.Instance != null)
        {
            List<GameObject> slots = GetSlotListDirectly();
            int savedIndex = 0;

            foreach (GameObject slot in slots)
            {
                if (slot != null && slot.transform.childCount > 0)
                {
                    InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                    if (item != null && item.itemData != null)
                    {
                        PlayerPrefs.SetString($"inventory_item_{savedIndex}_name", item.itemData.itemName);
                        PlayerPrefs.SetInt($"inventory_item_{savedIndex}_amount", item.amount);
                        savedIndex++;
                    }
                }
            }
            PlayerPrefs.SetInt("inventory_item_count", savedIndex);
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveLoadManager] Game saved successfully.");
    }

    // Load game progress from PlayerPrefs
    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[SaveLoadManager] No save exists to load!");
            return;
        }

        Debug.Log("[SaveLoadManager] Loading game progress...");

        // 1. Load Position
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            float x = PlayerPrefs.GetFloat("player_pos_x");
            float y = PlayerPrefs.GetFloat("player_pos_y");
            float z = PlayerPrefs.GetFloat("player_pos_z");
            Vector3 targetPos = new Vector3(x, y, z);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = targetPos;
            if (cc != null) cc.enabled = true;
        }

        // 2. Load Stats
        if (SurvivalStats.Instance != null)
        {
            float health = PlayerPrefs.GetFloat("player_health");
            float hunger = PlayerPrefs.GetFloat("player_hunger");
            float thirst = PlayerPrefs.GetFloat("player_thirst");
            SurvivalStats.Instance.RestoreStats(health, hunger, thirst);
        }

        // 3. Load Quest State
        if (QuestManager.Instance != null)
        {
            int questInt = PlayerPrefs.GetInt("quest_state", 0);
            QuestManager.Instance.RestoreQuestState((QuestState)questInt);
        }

        // 4. Load Inventory
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InputSlotList();
            InventoryManager.Instance.ClearInventory();

            int itemCount = PlayerPrefs.GetInt("inventory_item_count", 0);
            for (int i = 0; i < itemCount; i++)
            {
                string itemName = PlayerPrefs.GetString($"inventory_item_{i}_name");
                int amount = PlayerPrefs.GetInt($"inventory_item_{i}_amount");

                ItemData itemData = InventoryManager.Instance.GetItemDataByName(itemName);
                if (itemData != null)
                {
                    InventoryManager.Instance.addItem(itemData, amount);
                }
            }
        }

        // 5. Scan and destroy already collected/harvested scene items
        InteractableObject[] allSceneObjects = FindObjectsOfType<InteractableObject>(true);
        int destroyedCount = 0;
        foreach (InteractableObject obj in allSceneObjects)
        {
            if (obj != null && (obj.interactType == InteractType.Collectable || obj.interactType == InteractType.Harvestable))
            {
                string itemID = $"collected_{obj.gameObject.name}_{obj.transform.position.x:F2}_{obj.transform.position.y:F2}_{obj.transform.position.z:F2}";
                if (PlayerPrefs.GetInt(itemID, 0) == 1)
                {
                    Destroy(obj.gameObject);
                    destroyedCount++;
                }
            }
        }
        Debug.Log($"[SaveLoadManager] Destroyed {destroyedCount} already collected/harvested scene items on load.");
        Debug.Log("[SaveLoadManager] Game loaded successfully.");
    }

    // Delete all saved player data (for death or victory)
    public void DeleteSave()
    {
        Debug.Log("[SaveLoadManager] Deleting save game...");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private List<GameObject> GetSlotListDirectly()
    {
        if (InventoryManager.Instance != null)
        {
            return InventoryManager.Instance.GetSlotList();
        }
        return new List<GameObject>();
    }
}
