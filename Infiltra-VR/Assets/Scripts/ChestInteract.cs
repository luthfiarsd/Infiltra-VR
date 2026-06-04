using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ChestInventory))]
public class ChestInteract : MonoBehaviour
{
    [Header("Referensi UI")]
    public ChestUI chestUIManager;   // Script pengatur UI Peti
    public GameObject chestUIPanel;  // Panel layarnya

    private bool isPlayerNear = false; // --- TAMBAHAN BARU: Jarak pemain ---

    public static bool HasOpenedChest { get; private set; } = false;

    private void Awake()
    {
        HasOpenedChest = false;
    }

    // Fungsi ini akan dipanggil oleh sistem interaksi VR atau lewat tombol tes
    [ContextMenu("Tes Buka Peti (Klik Kanan)")]
    public void OpenChest()
    {
        HasOpenedChest = true; // Set status bahwa peti telah dibuka
        
        // 1. Beritahu UI bahwa "Peti ini" yang sedang dibuka
        chestUIManager.currentChest = GetComponent<ChestInventory>();
        
        // 2. Munculkan Layar Panel Peti
        chestUIPanel.SetActive(true);
        
        // 3. Perbarui gambar-gambar item di dalamnya
        chestUIManager.RefreshUI();
    }

    // Fungsi sementara untuk mempermudah pengetesan tanpa kacamata VR
    private void Update()
    {
        // Jika pemain dekat dan menekan tombol 'F' di keyboard
        if (isPlayerNear && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (chestUIPanel != null && chestUIPanel.activeSelf)
            {
                // Jika sedang terbuka, matikan (tutup) peti
                chestUIPanel.SetActive(false);
            }
            else
            {
                // Jika sedang tertutup, buka peti
                OpenChest();
            }
        }
    }

    // --- TAMBAHAN BARU: Deteksi pemain di dekat Peti ---
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan pemain memiliki Tag "Player" dan memiliki komponen PlayerInventory
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            PlayerInventory playerInv = other.GetComponentInChildren<PlayerInventory>();
            if (playerInv != null)
            {
                playerInv.nearbyChest = this;
                
                // Cek apakah tas sedang terbuka saat pemain mendekat
                InventoryToggle invToggle = other.GetComponentInChildren<InventoryToggle>();
                if (invToggle != null && invToggle.inventoryPanel != null && invToggle.inventoryPanel.activeSelf)
                {
                    // Jika tas sudah terbuka, otomatis buka peti juga
                    OpenChest();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            PlayerInventory playerInv = other.GetComponentInChildren<PlayerInventory>();
            if (playerInv != null && playerInv.nearbyChest == this)
            {
                playerInv.nearbyChest = null;
                
                // Opsional: Tutup otomatis layar peti jika pemain menjauh
                if (chestUIPanel != null && chestUIPanel.activeSelf)
                {
                    chestUIPanel.SetActive(false);
                }
            }
        }
    }
}
