using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel; // Panel chua Menu Pause (ESC)
    
    private bool isPaused = false;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giu GameManager qua cac scene
        }
    }

    private void Start()
    {
        FindSceneReferences();
        CheckAndFixEventSystem();
    }

    private void Update()
    {
        // Khi bam ESC thi bat/tat Pause Menu (chi cho phep khi dang trong scene GamePlay)
        bool isEscapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (isEscapePressed && SceneManager.GetActiveScene().name == "GamePlay")
        {
            // Tranh bat pause khi dang trong hoi thoai hoac player da chet
            bool isDialogueActive = DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueActive();
            bool isDead = SurvivalStats.Instance != null && SurvivalStats.Instance.IsDead;

            if (!isDialogueActive && !isDead)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        Time.timeScale = 1f; // Reset timescale khi chuyen scene
        FindSceneReferences();
        CheckAndFixEventSystem();

        // Kiem tra ten scene de setup con tro chuot phu hop
        if (scene.name == "MainMenu" || scene.name == "EndScene")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (scene.name == "GamePlay")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Load game neu continue duoc bam
            if (SaveLoadManager.shouldLoadSave && SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.LoadGame();
                SaveLoadManager.shouldLoadSave = false; // Reset
            }
        }
    }

    private void FindSceneReferences()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerLook = playerMovement.GetComponent<PlayerLook>();
        }

        // Re-find the PausePanel dynamically since GameManager is DontDestroyOnLoad
        if (pauseMenuPanel == null)
        {
            pauseMenuPanel = FindInactiveGameObjectInScene("PausePanel");
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false); // An pause menu luc dau

            // Gan listener cho cac button trong PausePanel de tranh loi hoat dong do Singleton GameManager bi huy
            UnityEngine.UI.Button[] buttons = pauseMenuPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (UnityEngine.UI.Button button in buttons)
            {
                if (button != null)
                {
                    if (button.gameObject.name == "Resume")
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(ResumeGame);
                        Debug.Log("[GameManager] Bound Resume button dynamically.");
                    }
                    else if (button.gameObject.name == "MainMenu")
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(GoToMainMenu);
                        Debug.Log("[GameManager] Bound MainMenu button dynamically.");
                    }
                    else if (button.gameObject.name == "Quit")
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(QuitGame);
                        Debug.Log("[GameManager] Bound Quit button dynamically.");
                    }
                }
            }
        }
    }

    private GameObject FindInactiveGameObjectInScene(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.isLoaded) return null;

        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        foreach (GameObject root in rootObjects)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == objectName)
                {
                    return child.gameObject;
                }
            }
        }
        return null;
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Dung thoi gian trong game

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Vo hieu hoa dieu khien cua player
        SetPlayerControl(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Chay lai thoi gian

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Bat lai dieu khien cua player
        SetPlayerControl(true);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // Reset thoi gian truoc khi load scene
        SceneManager.LoadScene(sceneName);
    }

    public void GoToMainMenu()
    {
        // Tu dong save khi quay ve Menu tu gameplay
        if (SaveLoadManager.Instance != null && SceneManager.GetActiveScene().name == "GamePlay")
        {
            SaveLoadManager.Instance.SaveGame();
        }

        ResumeGame(); // Reset pause state, timescale, v.v.
        LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
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
                Debug.Log("[GameManager] Replaced StandaloneInputModule with InputSystemUIInputModule on EventSystem.");
            }
        }
    }
}
