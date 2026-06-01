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

    /// <summary>Shows a notification message with auto-hide.</summary>
    public void ShowNotification(string message)
    {
        if (infoNotificationText == null) return;

        // Overwrite current message immediately
        infoNotificationText.text = message;

        // Stop previous hide coroutine if running
        if (hideNotificationCoroutine != null)
        {
            StopCoroutine(hideNotificationCoroutine);
        }

        // Start new hide countdown
        hideNotificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
    }

    /// <summary>Updates text continuously (e.g. while holding a key) without spawning new coroutines each frame.</summary>
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

    /// <summary>Displays target HP info with auto-hide.</summary>
    public void ShowTargetHP(string targetName, float current, float max)
    {
        if (targetHPText == null) return;

        targetHPText.text = $"{targetName} - HP: {current}/{max}";

        // Stop previous hide coroutine if running
        if (hideHPCoroutine != null)
        {
            StopCoroutine(hideHPCoroutine);
        }

        // Start new hide countdown
        hideHPCoroutine = StartCoroutine(HideHPAfterDelay());
    }

    private IEnumerator HideHPAfterDelay()
    {
        yield return new WaitForSeconds(hpDuration);
        targetHPText.text = "";
        hideHPCoroutine = null;
    }
}
