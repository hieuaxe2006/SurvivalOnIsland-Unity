using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.Video;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject menuButtonsPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject backgroundPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button guideButton;
    [SerializeField] private Button quitButton;

    [Header("Guide Controller")]
    [SerializeField] private GuideUI guideUI;

    [Header("Loading UI Elements")]
    [SerializeField] private Image loadingBarFill;
    [SerializeField] private TMP_Text loadingText;

    [Header("Intro Video Settings")]
    [SerializeField] private VideoPlayer introVideoPlayer;
    [SerializeField] private RawImage videoOutputImage;

    private void Start()
    {
        CheckAndFixEventSystem();

        // Check if a valid save exists to toggle the Continue button
        bool hasSave = PlayerPrefs.GetInt("save_exists", 0) == 1;

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(hasSave);
            continueButton.onClick.AddListener(ContinueGame);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(NewGame);
        }

        if (guideButton != null && guideUI != null)
        {
            guideButton.onClick.AddListener(OpenGuide);
            guideUI.OnClose += () => {
                if (menuButtonsPanel != null)
                {
                    menuButtonsPanel.SetActive(true);
                }
            };
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false); // Hide loading panel at start
        }

        if (menuButtonsPanel != null)
        {
            menuButtonsPanel.SetActive(true); // Show menu buttons at start
        }

        if (videoOutputImage != null)
        {
            videoOutputImage.gameObject.SetActive(false); // Hide raw image at start
        }

        if (introVideoPlayer != null)
        {
            introVideoPlayer.playOnAwake = false;
            introVideoPlayer.Stop();
        }

        // Unlock mouse cursor for Main Menu navigation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log("[MainMenuUI] Continuing game from last save...");
        SaveLoadManager.shouldLoadSave = true;
        StartCoroutine(LoadGameplayAsync());
    }

    public void NewGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log("[MainMenuUI] Starting a new game...");
        SaveLoadManager.shouldLoadSave = false;
        
        // Wipe old save data to prevent collision
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Play intro video first if assigned
        if (introVideoPlayer != null && videoOutputImage != null)
        {
            StartCoroutine(PlayIntroThenLoad());
        }
        else
        {
            StartCoroutine(LoadGameplayAsync());
        }
    }

    private void OpenGuide()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (guideUI != null)
        {
            if (menuButtonsPanel != null)
            {
                menuButtonsPanel.SetActive(false);
            }
            guideUI.OpenGuide();
        }
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log("[MainMenuUI] Quitting application...");
        Application.Quit();
    }

    // Handles async scene loading with a loading progress bar
    private IEnumerator LoadGameplayAsync()
    {
        if (menuButtonsPanel != null)
        {
            menuButtonsPanel.SetActive(false);
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GamePlay");
        
        while (!asyncLoad.isDone)
        {
            // Normalize progress range from 0 - 0.9 to 0 - 1.0
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (loadingBarFill != null)
            {
                loadingBarFill.fillAmount = progress;
            }

            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100f)}%";
            }

            yield return null;
        }
    }

    // Plays the PlaneCrash video clip, then starts loading the gameplay scene
    private IEnumerator PlayIntroThenLoad()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        if (menuButtonsPanel != null)
        {
            menuButtonsPanel.SetActive(false);
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // Find and hide background panel automatically if not assigned
        if (backgroundPanel == null)
        {
            Transform bgTransform = transform.parent != null ? transform.parent.Find("Background") ?? transform.parent.Find("BackgroundImage") ?? transform.parent.Find("BackgroundPanel") : null;
            if (bgTransform != null)
            {
                backgroundPanel = bgTransform.gameObject;
            }
            else
            {
                backgroundPanel = GameObject.Find("Background") ?? GameObject.Find("BackgroundImage") ?? GameObject.Find("BackgroundPanel");
            }
        }
        if (backgroundPanel != null)
        {
            backgroundPanel.SetActive(false);
        }

        // Prepare video to prevent black flickering screens
        introVideoPlayer.Prepare();
        while (!introVideoPlayer.isPrepared)
        {
            yield return null;
        }

        videoOutputImage.gameObject.SetActive(true);
        introVideoPlayer.Play();

        // Wait until video has finished playing
        bool videoFinished = false;
        VideoPlayer.EventHandler onVideoFinished = null;
        onVideoFinished = (vp) => {
            videoFinished = true;
            introVideoPlayer.loopPointReached -= onVideoFinished;
        };
        introVideoPlayer.loopPointReached += onVideoFinished;

        while (!videoFinished)
        {
            yield return null;
        }

        videoOutputImage.gameObject.SetActive(false);
        yield return StartCoroutine(LoadGameplayAsync());
    }

    private void CheckAndFixEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            UnityEngine.EventSystems.StandaloneInputModule oldModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                Destroy(oldModule);
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("[MainMenuUI] EventSystem upgraded to the new InputSystem UI module.");
            }
        }
    }
}
