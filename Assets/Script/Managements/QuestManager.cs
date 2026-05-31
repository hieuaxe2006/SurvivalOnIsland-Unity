using System;
using UnityEngine;
using TMPro;

public enum QuestState { NotStarted, InProgress, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Settings")]
    public int totalParts = 5;
    public QuestState currentState = QuestState.NotStarted;

    [Header("Quest UI References")]
    [SerializeField] private GameObject questPanel; // Khung UI chua thong tin quest
    [SerializeField] private TMP_Text questText;     // Text hien thi: "Linh kien: 0/5"

    [Header("Win Trigger Settings")]
    [SerializeField] private GameObject escapeTrigger; // Object chua trigger de thoat game (kich hoat khi quest Completed)

    // Lay so luong linh kien tu Inventory
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
            escapeTrigger.SetActive(false); // An trigger escape luc dau
        }

        UpdateQuestUI();
    }

    private void Update()
    {
        UpdateQuestUI();
    }

    public void StartQuest()
    {
        if (currentState == QuestState.NotStarted)
        {
            currentState = QuestState.InProgress;
            Debug.Log("Quest bat dau! Hay thu thap 5 linh kien vao Inventory.");
        }
    }

    public void CompleteQuest()
    {
        if (currentState == QuestState.InProgress)
        {
            // Xoa 5 AirplanePart khoi inventory khi hoan thanh
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem("AirplanePart", totalParts);
            }

            currentState = QuestState.Completed;
            Debug.Log("Da hoan thanh nhiem vu sua may bay!");
            ActivateEscapeTrigger();
        }
    }

    public void ActivateEscapeTrigger()
    {
        if (escapeTrigger != null)
        {
            escapeTrigger.SetActive(true);
            Debug.Log("Cua thoat hiem (Escape Trigger) da duoc kich hoat! Hay chay vao may bay de tron thoat.");
        }
        else
        {
            Debug.LogWarning("Chua gan escapeTrigger trong QuestManager! Se ket thuc game lap tuc (gia lap).");
        }
    }

    private void UpdateQuestUI()
    {
        bool isQuestActive = currentState == QuestState.InProgress;

        if (questPanel != null)
        {
            // SetActive hoạt động an toàn từ manager luôn hoạt động
            questPanel.SetActive(isQuestActive);
        }

        if (isQuestActive && questText != null)
        {
            questText.text = partsCollected + " / " + totalParts;
        }
    }

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
