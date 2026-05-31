using UnityEngine;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    public static bool shouldLoadSave = false;

    private void Awake()
    {
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

    public bool HasSave()
    {
        return PlayerPrefs.GetInt("save_exists", 0) == 1;
    }

    public void SaveGame()
    {
        Debug.Log("[SaveLoadManager] Saving game progress...");

        // 1. Mark save as existing
        PlayerPrefs.SetInt("save_exists", 1);

        // 2. Save Player Position (No rotation, as requested)
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            PlayerPrefs.SetFloat("player_pos_x", pos.x);
            PlayerPrefs.SetFloat("player_pos_y", pos.y);
            PlayerPrefs.SetFloat("player_pos_z", pos.z);
            Debug.Log($"[SaveLoadManager] Saved player position: {pos}");
        }

        // 3. Save Player Stats
        if (SurvivalStats.Instance != null)
        {
            PlayerPrefs.SetFloat("player_health", SurvivalStats.Instance.CurrentHealth);
            PlayerPrefs.SetFloat("player_hunger", SurvivalStats.Instance.CurrentHunger);
            PlayerPrefs.SetFloat("player_thirst", SurvivalStats.Instance.CurrentThirst);
            Debug.Log($"[SaveLoadManager] Saved stats - Health: {SurvivalStats.Instance.CurrentHealth}, Hunger: {SurvivalStats.Instance.CurrentHunger}, Thirst: {SurvivalStats.Instance.CurrentThirst}");
        }

        // 4. Save Quest State
        if (QuestManager.Instance != null)
        {
            PlayerPrefs.SetInt("quest_state", (int)QuestManager.Instance.currentState);
            Debug.Log($"[SaveLoadManager] Saved quest state: {QuestManager.Instance.currentState}");
        }

        // 5. Save Inventory (Simple Item + Amount format, as requested)
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
            Debug.Log($"[SaveLoadManager] Saved {savedIndex} inventory stacks.");
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveLoadManager] Game saved successfully.");
    }

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

            Debug.Log($"[SaveLoadManager] Loaded player position: {targetPos}");
        }

        // 2. Load Stats
        if (SurvivalStats.Instance != null)
        {
            float health = PlayerPrefs.GetFloat("player_health");
            float hunger = PlayerPrefs.GetFloat("player_hunger");
            float thirst = PlayerPrefs.GetFloat("player_thirst");
            SurvivalStats.Instance.RestoreStats(health, hunger, thirst);
            Debug.Log($"[SaveLoadManager] Loaded stats - Health: {health}, Hunger: {hunger}, Thirst: {thirst}");
        }

        // 3. Load Quest State
        if (QuestManager.Instance != null)
        {
            int questInt = PlayerPrefs.GetInt("quest_state", 0);
            QuestManager.Instance.RestoreQuestState((QuestState)questInt);
            Debug.Log($"[SaveLoadManager] Loaded quest state: {(QuestState)questInt}");
        }

        // 4. Load Inventory
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InputSlotList();
            InventoryManager.Instance.ClearInventory();

            int itemCount = PlayerPrefs.GetInt("inventory_item_count", 0);
            Debug.Log($"[SaveLoadManager] Loading {itemCount} inventory items...");
            for (int i = 0; i < itemCount; i++)
            {
                string itemName = PlayerPrefs.GetString($"inventory_item_{i}_name");
                int amount = PlayerPrefs.GetInt($"inventory_item_{i}_amount");

                ItemData itemData = InventoryManager.Instance.GetItemDataByName(itemName);
                if (itemData != null)
                {
                    InventoryManager.Instance.addItem(itemData, amount);
                    Debug.Log($"[SaveLoadManager] Restored item: {itemName} x {amount}");
                }
                else
                {
                    Debug.LogWarning($"[SaveLoadManager] Could not find ItemData for name: {itemName}");
                }
            }
        }

        Debug.Log("[SaveLoadManager] Game loaded successfully.");
    }

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
