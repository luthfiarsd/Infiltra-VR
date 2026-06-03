/*
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
*/

using System;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    GameWon,
    WaveWon 
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState CurrentState { get; private set; }
    
    public static event Action<GameState> OnGameStateChanged;

    [Header("Referensi Sistem")]
    [Tooltip("Tarik GameObject yang memiliki script PlayerInventory ke sini")]
    public PlayerInventory playerInventory;

    [Header("Environment State")]
    public int totalWaterAbsorption = 0; 
    public int totalPlantRewardBonus = 0; // Total bonus uang dari semua tanaman dewasa
    public int currentWave = 1; 
    public int baseThreshold = 50; 

    [Header("Wave Reward Settings")]
    [Tooltip("Reward dasar saat menang wave pertama")]
    public int baseWaveReward = 1000;
    [Tooltip("Tambahan reward per level wave")]
    public int rewardPerWaveLevel = 500;

    [Header("UI Visuals Baru")]
    [Tooltip("Tarik objek Wave_Text (TMP) ke sini")]
    public TextMeshProUGUI waveTextUI;
    
    [Tooltip("Tarik objek Progress_Text (TMP) ke sini")]
    public TextMeshProUGUI progressTextUI;

    [Tooltip("Tarik objek Slider Progres ke sini")]
    public Slider progressSlider;

    [Header("Efek Visual Target Tercapai")]
    [Tooltip("Tarik objek 'Fill' (yang ada di dalam Slider) ke sini")]
    public Image sliderFillImage; // Menambahkan akses ke komponen gambar untuk mengubah warna
    public Color warnaNormal = Color.green; // Warna saat belum tercapai
    public Color warnaTargetTercapai = new Color(0.2f, 0.8f, 1f); // Warna biru muda saat target tercapai

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

    private void Start()
    {
        ChangeState(GameState.MainMenu);
        UpdateUI();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f; 
                break;
            case GameState.Playing:
                Time.timeScale = 1f; 
                break;
            case GameState.Paused:
                Time.timeScale = 0f; 
                break;
            case GameState.GameOver:
                Time.timeScale = 0f; 
                break;
            case GameState.GameWon:
                Time.timeScale = 0f; 
                break;
            case GameState.WaveWon:
                Time.timeScale = 0f; 
                break;
        }

        Debug.Log($"[GameManager] Game State berubah menjadi: {newState}");
        OnGameStateChanged?.Invoke(newState);
    }

    public void ResetGame()
    {
        totalWaterAbsorption = 0;
        totalPlantRewardBonus = 0;
        currentWave = 1;
        
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.currentWave = 1;
            WaveManager.Instance.waveWaterThreshold = 50;
        }

        // Reset efek cuaca dan banjir di TimelapseManager
        TimelapseManager timelapse = FindAnyObjectByType<TimelapseManager>();
        if (timelapse != null)
        {
            timelapse.ResetEnvironment();
        }

        // Reset semua plot tanah ke kosong
        ResetAllPlots();
        
        UpdateUI();
        Debug.Log("[GameManager] Game di-reset ke awal.");
    }

    private void ResetAllPlots()
    {
        TanahBerkebun[] allPlots = FindObjectsByType<TanahBerkebun>(FindObjectsSortMode.None);
        foreach (var plot in allPlots)
        {
            if (plot != null)
            {
                plot.ResetPlot();
            }
        }
    }

    public void AddPlantedTreeAbsorption(int absorptionValue)
    {
        totalWaterAbsorption += absorptionValue;
        Debug.Log($"[GameManager] Pohon ditanam! Serapan bertambah {absorptionValue}. Total Serapan: {totalWaterAbsorption}");
        
        UpdateUI();
        CheckWaveCondition();
    }

    /// <summary>
    /// Dipanggil oleh TanahBerkebun saat pohon dewasa (dipupuk).
    /// </summary>
    public void AddPlantedTreeReward(int rewardValue)
    {
        totalPlantRewardBonus += rewardValue;
        Debug.Log($"[GameManager] Bonus tanaman +{rewardValue}. Total Bonus: {totalPlantRewardBonus}");
    }

    /// <summary>
    /// Hitung total reward menang wave.
    /// Formula: baseWaveReward + (currentWave - 1) * rewardPerWaveLevel + totalPlantRewardBonus
    /// </summary>
    public int CalculateWaveReward()
    {
        int waveBonus = baseWaveReward + (currentWave - 1) * rewardPerWaveLevel;
        int total = waveBonus + totalPlantRewardBonus;
        Debug.Log($"[GameManager] Reward = {waveBonus} (wave) + {totalPlantRewardBonus} (tanaman) = {total}");
        return total;
    }

    private void CheckWaveCondition()
    {
        int currentThreshold = currentWave * baseThreshold;
        
        if (totalWaterAbsorption >= currentThreshold)
        {
            // KITA MATIKAN PERGANTIAN STATE OTOMATISNYA
            // ChangeState(GameState.WaveWon); 

            Debug.Log($"[GameManager] Target Wave {currentWave} tercapai! Menunggu simulasi dimulai...");
        }
    }

    public void UpdateUI()
    {
        // 1. Cari referensi otomatis jika belum disetup di Inspector
        if (waveTextUI == null || progressTextUI == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in allTexts)
            {
                if (waveTextUI == null && t.name == "Wave_Text") waveTextUI = t;
                if (progressTextUI == null && t.name == "Progress_Text") progressTextUI = t;
            }
        }

        if (progressSlider == null || sliderFillImage == null)
        {
            RectTransform[] allTransforms = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var rt in allTransforms)
            {
                if (rt.name == "GameplayHUD_Panel")
                {
                    progressSlider = rt.GetComponentInChildren<Slider>(true);
                    if (progressSlider != null)
                    {
                        Transform fillTransform = progressSlider.transform.Find("Fill Area/Fill");
                        if (fillTransform != null) sliderFillImage = fillTransform.GetComponent<Image>();
                    }
                    break;
                }
            }
        }

        // 2. Logika Update Text & Slider
        int currentThreshold = currentWave * baseThreshold;
        
        // Mengecek apakah target sudah tercapai atau belum
        bool isTargetTercapai = totalWaterAbsorption >= currentThreshold;

        if (waveTextUI != null)
        {
            waveTextUI.text = "Wave: " + currentWave;
        }

        if (progressTextUI != null)
        {
            // Logika perubahan teks
            if (isTargetTercapai)
            {
                progressTextUI.text = "Target Aman: " + totalWaterAbsorption + " / " + currentThreshold + " L";
            }
            else
            {
                progressTextUI.text = "Penyerapan: " + totalWaterAbsorption + " / " + currentThreshold + " L";
            }
        }

        if (progressSlider != null)
        {
            progressSlider.maxValue = currentThreshold;
            progressSlider.value = totalWaterAbsorption;
        }

        // Logika perubahan warna bar
        if (sliderFillImage != null)
        {
            sliderFillImage.color = isTargetTercapai ? warnaTargetTercapai : warnaNormal;
        }
    }

    [ContextMenu("Test: Tanam Pohon (+15 Serapan)")]
    public void TestPlantTree()
    {
        AddPlantedTreeAbsorption(15);
    }
}