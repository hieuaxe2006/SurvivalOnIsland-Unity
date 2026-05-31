using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeTrigger : MonoBehaviour
{
    //func detect player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if quest is completed -> load end scene
            if (QuestManager.Instance != null && QuestManager.Instance.currentState == QuestState.Completed)
            {
                Debug.Log("Player entered escape trigger! Loading EndScene...");
                
                // Mo khoa chuot lai
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Xoa file save khi da thoat dao thanh cong
                if (SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.DeleteSave();
                }

                // Trigger Victory Screen on DeathUI
                if (DeathUI.Instance != null)
                {
                    DeathUI.Instance.TriggerVictoryScreen();
                }
                else
                {
                    Debug.LogError("[EscapeTrigger] Khong tim thay DeathUI.Instance de hien thi Victory Panel! Chuyen ve MainMenu.");
                    SceneManager.LoadScene("MainMenu");
                }
            }
            else
            {
                Debug.Log("Nhiem vu chua hoan thanh. Chua the thoat khoi dao!");
            }
        }
    }
}
