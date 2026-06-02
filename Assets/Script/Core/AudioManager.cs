using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Audio Clips")]
    public AudioClip bgmMenu;
    public AudioClip bgmGameplay;
    public AudioClip ambientWind;
    public AudioClip bgmVictory;
    public AudioClip bgmGameOver;
    public AudioClip sfxClick;
    public AudioClip sfxCollect;
    public AudioClip sfxAttack;
    public AudioClip sfxHit;
    public AudioClip sfxHurt;
    public AudioClip sfxDeath;
    public AudioClip sfxFootstep;

    private void Awake()
    {
        // Singleton pattern to keep AudioManager alive across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Add AudioSources automatically if not pre-assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;

            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Play scene BGM on start
        PlaySceneMusic(SceneManager.GetActiveScene().name);
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
        PlaySceneMusic(scene.name);
    }

    // Handles playing ambient and music tracks depending on active scene name
    private void PlaySceneMusic(string sceneName)
    {
        if (sceneName == "MainMenu")
        {
            PlayMusic(bgmMenu);
            StopAmbient();
        }
        else if (sceneName == "GamePlay")
        {
            PlayMusic(bgmGameplay);
            PlayAmbient(ambientWind);
        }
        else
        {
            StopMusic();
            StopAmbient();
        }
    }

    // Plays a 2D sound effect (e.g. UI clicks, item collection)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        musicSource.PlayOneShot(clip, volume);
    }

    // Plays a directional 3D sound effect (e.g. tree chop, hitting monsters)
    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        GameObject tempGO = new GameObject("TempSFX_" + clip.name);
        tempGO.transform.position = position;
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.spatialBlend = 1f; // 3D spatial sound
        aSource.minDistance = 2f;
        aSource.maxDistance = 20f;
        aSource.rolloffMode = AudioRolloffMode.Linear;
        aSource.Play();
        Destroy(tempGO, clip.length);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip) return; // Skip if already playing

        musicSource.clip = clip;
        musicSource.loop = loop;
        if (clip != null)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        if (ambientSource.clip == clip) return;

        ambientSource.clip = clip;
        ambientSource.loop = loop;
        if (clip != null)
        {
            ambientSource.Play();
        }
        else
        {
            ambientSource.Stop();
        }
    }

    public void StopAmbient()
    {
        ambientSource.Stop();
    }

    // Convenient shortcut helpers for quick SFX invocation
    public void PlayClick() => PlaySFX(sfxClick, 0.8f);
    public void PlayCollect() => PlaySFX(sfxCollect, 0.9f);
    public void PlayAttack() => PlaySFX(sfxAttack, 0.7f);
    public void PlayHit3D(Vector3 position) => PlaySFX3D(sfxHit, position, 0.8f);
    public void PlayHurt() => PlaySFX(sfxHurt, 0.9f);
    public void PlayDeath3D(Vector3 position) => PlaySFX3D(sfxDeath, position, 1f);
    public void PlayFootstep() => PlaySFX(sfxFootstep, 0.3f);
}
