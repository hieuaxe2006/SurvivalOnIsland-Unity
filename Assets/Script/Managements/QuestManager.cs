using System;
using UnityEngine;
using TMPro;

public enum QuestState { NotStarted, InProgress, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Settings")]
    public int totalParts = 5; // Target count of AirplaneParts to collect
    public QuestState currentState = QuestState.NotStarted;

    [Header("Quest UI References")]
    [SerializeField] private GameObject questPanel; // Quest info container panel
    [SerializeField] private TMP_Text questText; // Displays progress, e.g. "0 / 5"

    [Header("Win Trigger Settings")]
    [SerializeField] private GameObject escapeTrigger; // Trigger inside plane cabin activated on quest completion

    // Get the current number of AirplaneParts in the inventory
    public int partsCollected
    {
        get
        {
            if (InventoryManager.Instance != null)
            {
                return InventoryManager.Instance.GetItemCount("AirplanePart");
            }
            return 0;
        }
    }

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
        if (escapeTrigger != null)
        {
            escapeTrigger.SetActive(false); // Hide escape trigger initially
        }

        UpdateQuestUI();
    }

    private void Update()
    {
        UpdateQuestUI();
    }

    // Start the quest
    public void StartQuest()
    {
        if (currentState == QuestState.NotStarted)
        {
            currentState = QuestState.InProgress;
            Debug.Log("Quest started! Find all 5 plane parts scattered on the island.");
        }
    }

    // Complete the quest (removes the AirplaneParts and activates the escape trigger)
    public void CompleteQuest()
    {
        if (currentState == QuestState.InProgress)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem("AirplanePart", totalParts);
            }

            currentState = QuestState.Completed;
            Debug.Log("Quest completed! Cabin door is now open.");
            ActivateEscapeTrigger();
        }
    }

    public void ActivateEscapeTrigger()
    {
        if (escapeTrigger != null)
        {
            escapeTrigger.SetActive(true);
            Debug.Log("Escape cabin trigger activated! Go inside to leave the island.");
        }
        else
        {
            Debug.LogWarning("escapeTrigger not assigned in QuestManager!");
        }
    }

    private void UpdateQuestUI()
    {
        bool isQuestActive = currentState == QuestState.InProgress;

        if (questPanel != null)
        {
            questPanel.SetActive(isQuestActive);
        }

        if (isQuestActive && questText != null)
        {
            questText.text = partsCollected + " / " + totalParts;
        }
    }

    // Used when loading a saved game to restore quest states
    public void RestoreQuestState(QuestState state)
    {
        currentState = state;
        if (currentState == QuestState.Completed)
        {
            ActivateEscapeTrigger();
        }
        else
        {
            if (escapeTrigger != null)
            {
                escapeTrigger.SetActive(false);
            }
        }
        UpdateQuestUI();
    }
}
