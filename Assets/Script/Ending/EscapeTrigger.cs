using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only allow escape if quest is completed
            if (QuestManager.Instance != null && QuestManager.Instance.currentState == QuestState.Completed)
            {
                Debug.Log("Player entered escape trigger! Loading EndScene...");

                // Unlock cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Delete save file on successful escape
                if (SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.DeleteSave();
                }

                // Trigger victory screen
                if (DeathUI.Instance != null)
                {
                    DeathUI.Instance.TriggerVictoryScreen();
                }
                else
                {
                    Debug.LogError("[EscapeTrigger] DeathUI.Instance not found for Victory Panel! Returning to MainMenu.");
                    SceneManager.LoadScene("MainMenu");
                }
            }
            else
            {
                Debug.Log("Quest not completed yet. Cannot escape the island!");
            }
        }
    }
}
