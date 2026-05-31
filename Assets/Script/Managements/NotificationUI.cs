using System.Collections;
using UnityEngine;
using TMPro;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    [Header("UI Text References")]
    [SerializeField] private TMP_Text infoNotificationText;
    [SerializeField] private TMP_Text targetHPText;

    [Header("Fade Settings")]
    [SerializeField] private float notificationDuration = 2.5f;
    [SerializeField] private float hpDuration = 3f;

    private Coroutine hideNotificationCoroutine;
    private Coroutine hideHPCoroutine;

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
        if (infoNotificationText != null)
        {
            infoNotificationText.text = "";
        }

        if (targetHPText != null)
        {
            targetHPText.text = "";
        }
    }

    // --- DISPLAY HUD NOTIFICATIONS ---
    public void ShowNotification(string message)
    {
        if (infoNotificationText == null) return;

        // Ghi đè ngay lập tức nội dung cũ
        infoNotificationText.text = message;

        // Dừng coroutine ẩn cũ nếu đang chạy
        if (hideNotificationCoroutine != null)
        {
            StopCoroutine(hideNotificationCoroutine);
        }

        // Bắt đầu đếm ngược ẩn mới
        hideNotificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
    }

    // Cập nhật text liên tục (như khi giữ phím) mà không tạo coroutine mới mỗi frame
    public void UpdateProgressNotification(string message)
    {
        if (infoNotificationText == null) return;
        infoNotificationText.text = message;
        if (hideNotificationCoroutine != null)
        {
            StopCoroutine(hideNotificationCoroutine);
            hideNotificationCoroutine = null;
        }
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        infoNotificationText.text = "";
        hideNotificationCoroutine = null;
    }

    // --- DISPLAY TARGET HP / HITS ---
    public void ShowTargetHP(string targetName, float current, float max)
    {
        if (targetHPText == null) return;

        // Định dạng text: "Mục tiêu - HP: Hiện tại/Tối đa"
        targetHPText.text = $"{targetName} - HP: {current}/{max}";

        // Dừng coroutine ẩn cũ nếu đang chạy
        if (hideHPCoroutine != null)
        {
            StopCoroutine(hideHPCoroutine);
        }

        // Bắt đầu đếm ngược ẩn mới
        hideHPCoroutine = StartCoroutine(HideHPAfterDelay());
    }

    private IEnumerator HideHPAfterDelay()
    {
        yield return new WaitForSeconds(hpDuration);
        targetHPText.text = "";
        hideHPCoroutine = null;
    }
}
