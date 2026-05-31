using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteract : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "Pilot";
    
    [Header("Quest Item Settings")]
    [SerializeField] private ItemData airplanePartItem; // Keo tha ItemData cua AirplanePart vao day de NPC tang 1 cai free

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI; // GameObject hien "Nhan E de tro chuyen" (tuy chon)

    private bool isInRange = false;

    private void Update()
    {
        // Neu player dang trong tam va nhan phim E, dong thoi khong dang trong hoi thoai
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
            Debug.LogError("QuestManager chua duoc khoi tao!");
            return;
        }

        // An prompt UI khi bat dau tro chuyen
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
                
                // NPC tang 1 linh kien free truc tiep vao inventory
                if (airplanePartItem != null && InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.addItem(airplanePartItem);
                    Debug.Log("NPC Pilot give you first part" + airplanePartItem.itemName);
                }
                else
                {
                    Debug.LogWarning("Chua gan airplanePartItem hoac thieu InventoryManager!");
                }
                
                // Hien lai prompt UI neu player van o trong tam
                if (isInRange && promptUI != null) promptUI.SetActive(true);
            });
        }
        else if (QuestManager.Instance.currentState == QuestState.InProgress)
        {
            // Kiem tra xem co du 5 linh kien trong inventory chua
            if (QuestManager.Instance.partsCollected >= QuestManager.Instance.totalParts)
            {
                string[] finishSentences = new string[]
                {
                    "Excellent! You’ve found all 5 plane parts!",
                    "Let me repair the engine. Get inside the plane cabin — we can leave this island right now!"
    
                };

                DialogueUI.Instance.StartDialogue(npcName, finishSentences, () =>
                {
                    QuestManager.Instance.CompleteQuest(); // Tang completed, xoa 5 linh kien, bat escape trigger
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
                "Engine has been worked! Go inside plane"
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
