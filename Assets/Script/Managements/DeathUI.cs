using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathUI : MonoBehaviour
{
    public static DeathUI Instance { get; private set; }

    [Header("UI Panel Reference")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private TMP_Text endTitleText;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    [Header("Buttons")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button quitButton;

    [Header("Victory Video Settings")]
    [SerializeField] private UnityEngine.Video.VideoPlayer victoryVideoPlayer;
    [SerializeField] private UnityEngine.UI.RawImage victoryVideoOutput;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private CharacterController characterController;
    private bool hasTriggeredDeath = false;
    private Vector3 deathPosition;

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
        FindPlayerReferences();

        if (deathPanel != null)
        {
            deathPanel.SetActive(false); // hide ban dau
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(false); // hide overlay at start
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
        }

        if (victoryVideoOutput != null)
        {
            victoryVideoOutput.gameObject.SetActive(false); // Hide video image at start
        }

        // Đảm bảo các nút bấm và tiêu đề bị ẩn lúc đầu
        SetUIElementsActive(false);

        // Đảm bảo video chiến thắng không tự chạy trên awake che mất gameplay
        if (victoryVideoPlayer != null)
        {
            victoryVideoPlayer.playOnAwake = false;
            victoryVideoPlayer.Stop();
        }

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(RespawnPlayer);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(QuitToMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    public void TriggerDeathScreen()
    {
        if (hasTriggeredDeath) return;
        hasTriggeredDeath = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbient();
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmGameOver, false);
        }

        // Ẩn tất cả nút bấm và chữ ngay lập tức
        SetUIElementsActive(false);
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        gameObject.SetActive(true);
        StartCoroutine(TriggerDeathScreenSequence());
    }

    //func get player info 
    private void FindPlayerReferences()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerLook = playerMovement.GetComponent<PlayerLook>();
            characterController = playerMovement.GetComponent<CharacterController>();
        }
    }

    private IEnumerator TriggerDeathScreenSequence()
    {
        if (playerMovement != null)
        {
            deathPosition = playerMovement.transform.position;
        }
        else
        {
            deathPosition = Vector3.zero;
        }

        // Khóa điều khiển người chơi và mở khóa con trỏ chuột
        SetPlayerControl(false);

        // Chờ 2 giây để hoạt ảnh chết chạy xong
        yield return new WaitForSeconds(2f);

        // Hiển thị hiệu ứng mờ đen dần (Fade in)
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            float elapsed = 0f;
            float duration = 1.0f;
            Color c = fadeOverlay.color;
            while (elapsed < duration)
            {
                c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
                fadeOverlay.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            c.a = 1f;
            fadeOverlay.color = c;
        }

        // Chỉ hiển thị tiêu đề và nút sau khi đã fade đen xong 100%
        if (endTitleText != null)
        {
            endTitleText.gameObject.SetActive(true);
            endTitleText.text = "GAME OVER";
        }

        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(true); // Hiện nút Replay khi thua
        }

        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(true);
        }

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }

    public void TriggerVictoryScreen()
    {
        if (hasTriggeredDeath) return;
        hasTriggeredDeath = true;

        // Phát nhạc chiến thắng ngay lập tức để chạy song song cùng video
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbient();
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmVictory, true);
        }

        // Ẩn tất cả nút bấm và chữ ngay lập tức
        SetUIElementsActive(false);
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        gameObject.SetActive(true);
        StartCoroutine(TriggerVictoryScreenSequence());
    }

    private IEnumerator TriggerVictoryScreenSequence()
    {
        // Khóa điều khiển người chơi và mở khóa con trỏ chuột
        SetPlayerControl(false);

        // Phát video chiến thắng máy bay cất cánh bay lên trời
        if (victoryVideoPlayer != null && victoryVideoOutput != null)
        {
            victoryVideoOutput.gameObject.SetActive(true);
            victoryVideoPlayer.Play();

            // Chờ cho đến khi video chạy xong
            bool videoFinished = false;
            UnityEngine.Video.VideoPlayer.EventHandler onVideoFinished = null;
            onVideoFinished = (vp) => {
                videoFinished = true;
                victoryVideoPlayer.loopPointReached -= onVideoFinished;
            };
            victoryVideoPlayer.loopPointReached += onVideoFinished;

            while (!videoFinished)
            {
                yield return null;
            }

            victoryVideoOutput.gameObject.SetActive(false);
        }
        else
        {
            // Chờ 2 giây nếu không có video
            yield return new WaitForSeconds(2f);
        }

        // Hiển thị hiệu ứng mờ đen dần (Fade in)
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            float elapsed = 0f;
            float duration = 1.0f;
            Color c = fadeOverlay.color;
            while (elapsed < duration)
            {
                c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
                fadeOverlay.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            c.a = 1f;
            fadeOverlay.color = c;
        }

        // Chỉ hiển thị tiêu đề và nút sau khi đã fade đen xong 100%
        if (endTitleText != null)
        {
            endTitleText.gameObject.SetActive(true);
            endTitleText.text = "VICTORY";
        }

        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false); // Không có Replay khi thắng
        }

        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(true);
        }

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        // Xóa file save khi đã thoát đảo thành công
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave();
        }
    }

    public void RespawnPlayer()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        // Reset timescale de tranh game bi ngung khi load scene moi
        Time.timeScale = 1f;

        // Xóa file save cũ vì người chơi đã chết, chơi lại từ đầu như New Game
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave();
        }
        else
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        SaveLoadManager.shouldLoadSave = false;

        // Load lai scene Gameplay hien tai
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(activeSceneName);
    }

    public void QuitToMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();


        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log("Quitting application...");
        Application.Quit();
    }

    private void SetPlayerControl(bool state)
    {
        if (playerMovement != null) playerMovement.enabled = state;
        if (playerLook != null) playerLook.enabled = state;

        if (!state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SetUIElementsActive(bool state)
    {
        if (replayButton != null) replayButton.gameObject.SetActive(state);
        if (menuButton != null) menuButton.gameObject.SetActive(state);
        if (quitButton != null) quitButton.gameObject.SetActive(state);
        if (endTitleText != null) endTitleText.gameObject.SetActive(state);
    }
}
