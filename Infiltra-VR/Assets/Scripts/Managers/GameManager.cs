using UnityEngine;
using TMPro; // Untuk mengakses UI Text

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
        // Update UI saat game pertama kali dimulai
        UpdateUI();
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
