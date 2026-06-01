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
            deathPanel.SetActive(false); // Hide panel at start
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(false); // Hide overlay at start
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
        }

        if (victoryVideoOutput != null)
        {
            victoryVideoOutput.gameObject.SetActive(false); // Hide video image at start
        }

        // Hide buttons and title at start
        SetUIElementsActive(false);

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

    // Triggers game over sequence
    public void TriggerDeathScreen()
    {
        if (hasTriggeredDeath) return;
        hasTriggeredDeath = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbient();
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxDeath);
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmGameOver, false);
        }

        // Disable player HUD panels if present
        var hudCanvas = GameObject.Find("HUDCanvas") ?? GameObject.Find("HUD");
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(false);
        }

        SetUIElementsActive(false);

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        gameObject.SetActive(true);
        StartCoroutine(TriggerDeathScreenSequence());
    }

    private IEnumerator TriggerDeathScreenSequence()
    {
        // Save current death coordinates before respawn calculations
        if (playerMovement != null)
        {
            deathPosition = playerMovement.transform.position;
        }

        // Lock player controls
        SetPlayerControl(false);

        // Slow down camera look and player actions
        if (playerLook != null)
        {
            playerLook.xRotation = 0f;
            playerLook.yRotation = 0f;
        }

        // Wait 2s for falling down death animation to play out
        yield return new WaitForSeconds(2.0f);

        // Smoothly fade to a black overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            float elapsed = 0f;
            float duration = 1.5f;
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

        // Show buttons and texts after fully faded black
        if (endTitleText != null)
        {
            endTitleText.gameObject.SetActive(true);
            endTitleText.text = "GAME OVER";
        }

        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(true); // Replay allowed on GameOver
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

    // Triggers victory screen sequence
    public void TriggerVictoryScreen()
    {
        if (hasTriggeredDeath) return;
        hasTriggeredDeath = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbient();
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmVictory, true);
        }

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
        SetPlayerControl(false);

        // Play the plane take-off video if assigned
        if (victoryVideoPlayer != null && victoryVideoOutput != null)
        {
            victoryVideoOutput.gameObject.SetActive(true);
            victoryVideoPlayer.Play();

            // Wait until video has finished playing
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
            yield return new WaitForSeconds(2f);
        }

        // Fade in black overlay
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

        if (endTitleText != null)
        {
            endTitleText.gameObject.SetActive(true);
            endTitleText.text = "VICTORY";
        }

        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false); // No replay button on Victory
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

        // Victory deletes the save file so players start fresh on next load
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.DeleteSave();
        }
    }

    // Handles the Respawn logic
    public void RespawnPlayer()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        Time.timeScale = 1f;

        // Wipe old save file because player died
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

        // Reload the current gameplay scene
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(activeSceneName);
    }

    public void QuitToMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMainMenu();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    private void FindPlayerReferences()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerLook = playerMovement.GetComponent<PlayerLook>();
            characterController = playerMovement.GetComponent<CharacterController>();
        }
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
        if (endTitleText != null) endTitleText.gameObject.SetActive(state);
        if (replayButton != null) replayButton.gameObject.SetActive(state);
        if (menuButton != null) menuButton.gameObject.SetActive(state);
        if (quitButton != null) quitButton.gameObject.SetActive(state);
    }
}
