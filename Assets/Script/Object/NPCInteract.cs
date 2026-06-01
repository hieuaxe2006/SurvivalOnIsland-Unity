using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteract : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Pilot";
    
    [Header("Quest Item Settings")]
    [SerializeField] private ItemData airplanePartItem; // Drop here the AirplanePart ItemData asset so the NPC can give 1 free part on quest start

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI; // UI GameObject showing "Press E to talk"

    private bool isInRange = false;

    private void Update()
    {
        // Check if the player is in range, presses E, and dialogue is not already running
        bool isEPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        if (isInRange && isEPressed)
        {
            if (DialogueUI.Instance != null && !DialogueUI.Instance.IsDialogueActive())
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager not found!");
            return;
        }

        // Hide talk prompt UI when conversation starts
        if (promptUI != null) promptUI.SetActive(false);

        if (QuestManager.Instance.currentState == QuestState.NotStarted)
        {
            string[] introSentences = new string[]
            {
                "You’re awake! Our plane has crashed...",
                "I need to collect all 5 parts to repair the plane engine.",
                "Here, I found one nearby. Take it.",
                "The other 4 parts are scattered across the island. Be careful... wild beasts are guarding them!"
            };

            DialogueUI.Instance.StartDialogue(npcName, introSentences, () =>
            {
                QuestManager.Instance.StartQuest();
                
                // NPC awards player 1 free part instantly into the inventory
                if (airplanePartItem != null && InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.addItem(airplanePartItem);
                    Debug.Log("NPC Pilot gave player the first AirplanePart.");
                }
                else
                {
                    Debug.LogWarning("Missing airplanePartItem or InventoryManager reference!");
                }
                
                // Show talk prompt UI again if player is still within range
                if (isInRange && promptUI != null) promptUI.SetActive(true);
            });
        }
        else if (QuestManager.Instance.currentState == QuestState.InProgress)
        {
            // Check if player has collected all 5 parts inside their inventory
            if (QuestManager.Instance.partsCollected >= QuestManager.Instance.totalParts)
            {
                string[] finishSentences = new string[]
                {
                    "Excellent! You’ve found all 5 plane parts!",
                    "Let me repair the engine. Get inside the plane cabin — we can leave this island right now!"
                };

                DialogueUI.Instance.StartDialogue(npcName, finishSentences, () =>
                {
                    QuestManager.Instance.CompleteQuest(); // Mark complete, consume items, unlock cabin trigger
                    if (isInRange && promptUI != null) promptUI.SetActive(true);
                });
            }
            else
            {
                string[] progressSentences = new string[]
                {
                    "Try to find all 5 parts to repair the plane.",
                    "Right now, you only have " + QuestManager.Instance.partsCollected + "/" +
                        QuestManager.Instance.totalParts + " parts in your inventory.",
                    "Be careful of the wild beasts on the island!"
                };

                DialogueUI.Instance.StartDialogue(npcName, progressSentences, () =>
                {
                    if (isInRange && promptUI != null) promptUI.SetActive(true);
                });
            }
        }
        else if (QuestManager.Instance.currentState == QuestState.Completed)
        {
            string[] afterFinishSentences = new string[]
            {
                "Engine is working! Go inside the plane cabin!"
            };

            DialogueUI.Instance.StartDialogue(npcName, afterFinishSentences, () =>
            {
                if (isInRange && promptUI != null) promptUI.SetActive(true);
            });
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            if (promptUI != null && DialogueUI.Instance != null && !DialogueUI.Instance.IsDialogueActive())
            {
                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
    }
}
