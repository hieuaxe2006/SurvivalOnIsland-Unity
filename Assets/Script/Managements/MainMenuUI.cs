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

        // Kiem tra truc tiep trong PlayerPrefs xem co ban save hay khong
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
            menuButtonsPanel.SetActive(true); // Show menu panel at start
        }

        if (videoOutputImage != null)
        {
            videoOutputImage.gameObject.SetActive(false); // Hide video image at start
        }

        // Đảm bảo video intro không tự chạy đè lên menu
        if (introVideoPlayer != null)
        {
            introVideoPlayer.playOnAwake = false;
            introVideoPlayer.Stop();
        }

        // Đảm bảo chuột hiển thị và tự do ở Main Menu
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
        
        // Xoa ban save cu de tranh xung dot
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Neu co video thi phat video truoc, sau do load game
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

    private IEnumerator LoadGameplayAsync()
    {
        // Hide button panel
        if (menuButtonsPanel != null)
        {
            menuButtonsPanel.SetActive(false);
        }

        // Show loading panel
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Start loading scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GamePlay");
        
        // Do not let scene switch until loading is at 90% (or let it switch automatically)
        // Here we just let it load naturally and track progress
        while (!asyncLoad.isDone)
        {
            // progress is from 0 to 0.9. Normalize it to 0 to 1
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

    private IEnumerator PlayIntroThenLoad()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Ẩn menu buttons
        if (menuButtonsPanel != null)
        {
            menuButtonsPanel.SetActive(false);
        }

        // Ẩn loading panel để tránh việc nó che mất video khi phát
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // Ẩn background panel để tránh che mất video
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

        // Chuẩn bị video player trước để tránh màn hình trống hoặc nháy hình
        introVideoPlayer.Prepare();
        while (!introVideoPlayer.isPrepared)
        {
            yield return null;
        }

        // Hiện video output và phát video
        videoOutputImage.gameObject.SetActive(true);
        introVideoPlayer.Play();

        // Chờ cho đến khi video chạy xong
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

        // Ẩn video, hiện loading và bắt đầu load scene
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
                Debug.Log("[MainMenuUI] Replaced StandaloneInputModule with InputSystemUIInputModule on EventSystem.");
            }
        }
    }
}

