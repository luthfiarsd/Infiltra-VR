using UnityEngine;
using TMPro; // Untuk mengakses UI Text

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave State")]
    public int currentWave = 1;
    public int waveWaterThreshold = 50;  // Batas ancaman air dari bencana untuk wave ini

    [Header("UI Visuals")]
    [Tooltip("Tarik Text (TMP) untuk menampilkan angka Wave ke sini")]
    public TextMeshProUGUI waveNumberTextUI;
    
    [Tooltip("Tarik Text (TMP) untuk menampilkan batas ancaman air ke sini")]
    public TextMeshProUGUI waterThresholdTextUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Update UI saat pertama kali mulai
        UpdateUI();
    }

    // Fungsi untuk lanjut ke wave berikutnya
    [ContextMenu("Test: Mulai Wave Berikutnya")]
    public void StartNextWave()
    {
        currentWave++;
        waveWaterThreshold += 50; // Contoh penambahan ancaman di tiap wave, bisa disesuaikan
        Debug.Log($"[WaveManager] Wave {currentWave} dimulai! Ancaman air naik menjadi {waveWaterThreshold}.");
        
        UpdateUI();
    }

    // Fungsi untuk mengecek apakah serapan pemain cukup untuk menahan wave
    [ContextMenu("Test: Check Survival")]
    public void CheckWaveSurvival()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager tidak ditemukan!");
            return;
        }

        int playerAbsorption = GameManager.Instance.totalWaterAbsorption;

        if (playerAbsorption >= waveWaterThreshold)
        {
            Debug.Log($"[WaveManager] BERHASIL! Serapan air ({playerAbsorption}) menahan ancaman wave ({waveWaterThreshold}).");
            
            // Beri hadiah uang ke PlayerInventory jika berhasil
            if (GameManager.Instance.playerInventory != null)
            {
                GameManager.Instance.playerInventory.uang += 100;
                Debug.Log($"Dapat hadiah 100 koin! Total uang: {GameManager.Instance.playerInventory.uang}");
            }
        }
        else
        {
            Debug.Log($"[WaveManager] GAGAL! Serapan air ({playerAbsorption}) kurang dari ancaman wave ({waveWaterThreshold}). Banjir terjadi!");
            // Logika kekalahan di sini
        }
    }

    // Menyegarkan teks UI di layar VR
    private void UpdateUI()
    {
        if (waveNumberTextUI != null)
        {
            waveNumberTextUI.text = "Wave: " + currentWave.ToString();
        }
        
        if (waterThresholdTextUI != null)
        {
            waterThresholdTextUI.text = "Batas Air ancaman bencana: " + waveWaterThreshold.ToString() + " L";
        }
    }
}
