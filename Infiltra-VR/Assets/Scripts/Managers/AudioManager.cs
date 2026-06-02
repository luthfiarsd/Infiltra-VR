using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource untuk memutar musik background (Main Menu dll)")]
    public AudioSource musicSource;
    [Tooltip("AudioSource untuk memutar suara ambience environment (Angin, Hutan, dll)")]
    public AudioSource ambienceSource;

    [Header("Audio Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip playingAmbience;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Berlangganan event saat GameState berubah
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Berhenti berlangganan saat script dimatikan
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        // Jika belum ada AudioSource, tambahkan otomatis
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (ambienceSource == null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
        }

        musicSource.volume = musicVolume;
        ambienceSource.volume = ambienceVolume;

        // Panggil penanganan state awal
        if (GameManager.Instance != null)
        {
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                PlayMusic(mainMenuMusic);
                StopAmbience();
                break;
            case GameState.Playing:
                StopMusic();
                PlayAmbience(playingAmbience);
                break;
            // Tambahkan kondisi state lain jika perlu (misal pause)
            case GameState.Paused:
                // Opsional: Kecilkan volume ambience atau jeda
                break;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        
        // Jika musik yang sama sedang diputar, jangan restart
        if (musicSource.isPlaying && musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (clip == null || ambienceSource == null) return;

        if (ambienceSource.isPlaying && ambienceSource.clip == clip) return;

        ambienceSource.clip = clip;
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        if (ambienceSource != null && ambienceSource.isPlaying)
        {
            ambienceSource.Stop();
        }
    }
}
