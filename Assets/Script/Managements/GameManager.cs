using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel; // Reference to the Pause Panel GameObject
    
    private bool isPaused = false;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;

    private void Awake()
    {
        // Keep GameManager alive across all scene transitions
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        FindSceneReferences();
        CheckAndFixEventSystem();
    }

    private void Update()
    {
        // Toggle pause menu with ESC (only in gameplay scene)
        bool isEscapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (isEscapePressed && SceneManager.GetActiveScene().name == "GamePlay")
        {
            // Do not pause if player is in dialogue or is dead
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

    // Runs automatically whenever a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        Time.timeScale = 1f; // Reset timescale on scene load
        FindSceneReferences();
        CheckAndFixEventSystem();

        // Manage mouse cursor state based on active scene
        if (scene.name == "MainMenu" || scene.name == "EndScene")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (scene.name == "GamePlay")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Defer loading to ensure all game objects are fully initialized first
            if (SaveLoadManager.shouldLoadSave)
            {
                StartCoroutine(LoadSaveDeferred());
            }
        }
    }

    // Wait exactly 1 frame for Awake/Start methods to finish before loading save
    private System.Collections.IEnumerator LoadSaveDeferred()
    {
        yield return null;

        SaveLoadManager slm = SaveLoadManager.Instance;
        if (slm == null)
        {
            slm = FindObjectOfType<SaveLoadManager>();
        }

        if (slm != null)
        {
            slm.LoadGame();
        }
        else
        {
            Debug.LogError("[GameManager] SaveLoadManager not found in scene!");
        }

        SaveLoadManager.shouldLoadSave = false; // Reset state
    }

    // Automatically locate references in the active scene
    private void FindSceneReferences()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerLook = playerMovement.GetComponent<PlayerLook>();
        }

        // Find the Pause Panel in the scene dynamically
        if (pauseMenuPanel == null)
        {
            pauseMenuPanel = FindInactiveGameObjectInScene("PausePanel");
        }

        // Setup button click events in the Pause Menu dynamically
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);

            UnityEngine.UI.Button[] buttons = pauseMenuPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (UnityEngine.UI.Button button in buttons)
            {
                if (button != null)
                {
                    if (button.gameObject.name == "Resume")
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(ResumeGame);
                    }
                    else if (button.gameObject.name == "MainMenu")
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(GoToMainMenu);
                    }
                    else if (button.gameObject.name == "Quit")
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(QuitGame);
                    }
                }
            }
        }
    }

    // Helper to find GameObjects that are inactive in the scene hierarchy
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

    // Pause the game (stops time and enables cursor)
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        SetPlayerControl(false);
    }

    // Resume the game (resumes time and locks cursor)
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        SetPlayerControl(true);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Save progress and return to the Main Menu
    public void GoToMainMenu()
    {
        if (SceneManager.GetActiveScene().name == "GamePlay")
        {
            SaveLoadManager slm = SaveLoadManager.Instance;
            if (slm == null)
            {
                slm = FindObjectOfType<SaveLoadManager>();
            }

            if (slm != null)
            {
                slm.SaveGame();
            }
            else
            {
                Debug.LogError("[GameManager] Could not find SaveLoadManager to save game progress!");
            }
        }

        ResumeGame();
        LoadScene("MainMenu");
    }

    public void QuitGame()
    {
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

    // Replaces the old EventSystem input module with the new InputSystem UI module to avoid UI click errors
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
                Debug.Log("[GameManager] Upgraded EventSystem to use the new InputSystem module.");
            }
        }
    }
}
