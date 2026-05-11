using System;
using UnityEngine;
using TMPro; // Untuk mengakses UI Text

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    GameWon,
    WaveWon // Menang satu wave, tapi belum tamat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState CurrentState { get; private set; }
    
    // Event yang dipanggil saat state game berubah
    public static event Action<GameState> OnGameStateChanged;

    [Header("Referensi Sistem")]
    [Tooltip("Tarik GameObject yang memiliki script PlayerInventory ke sini")]
    public PlayerInventory playerInventory;

    [Header("Environment State")]
    public int totalWaterAbsorption = 0; // Serapan air dari pohon yang DITANAM

    [Header("UI Visuals")]
    [Tooltip("Tarik Text (TMP) untuk menampilkan total serapan air ke sini")]
    public TextMeshProUGUI absorptionTextUI;

    private void Awake()
    {
        // Singleton pattern
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

    private void Start()
    {
        // Set awal game ke Main Menu
        ChangeState(GameState.MainMenu);
        
        // Update UI saat game pertama kali dimulai
        UpdateUI();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f; // Pause game saat di menu
                break;
            case GameState.Playing:
                Time.timeScale = 1f; // Jalankan game
                break;
            case GameState.Paused:
                Time.timeScale = 0f; // Pause game
                break;
            case GameState.GameOver:
                Time.timeScale = 0f; // Pause game
                break;
            case GameState.GameWon:
                Time.timeScale = 0f; // Pause game
                break;
            case GameState.WaveWon:
                Time.timeScale = 0f; // Pause game saat layar menang wave
                break;
        }

        Debug.Log($"[GameManager] Game State berubah menjadi: {newState}");
        OnGameStateChanged?.Invoke(newState);
    }

    // Dipanggil saat pohon ditanam dari tas ke dunia
    public void AddPlantedTreeAbsorption(int absorptionValue)
    {
        totalWaterAbsorption += absorptionValue;
        Debug.Log($"[GameManager] Pohon ditanam! Serapan bertambah {absorptionValue}. Total Serapan: {totalWaterAbsorption}");
        
        // Perbarui UI secara langsung
        UpdateUI();
    }

    // Fungsi untuk menyegarkan tampilan teks UI
    private void UpdateUI()
    {
        if (absorptionTextUI != null)
        {
            absorptionTextUI.text = "Penyerapan Air: " + totalWaterAbsorption.ToString() + " Liter";
        }
    }

    // --- FITUR TESTING (Bisa diklik kanan di Inspector) ---
    [ContextMenu("Test: Tanam Pohon (+15 Serapan)")]
    public void TestPlantTree()
    {
        AddPlantedTreeAbsorption(15);
    }

    
}
