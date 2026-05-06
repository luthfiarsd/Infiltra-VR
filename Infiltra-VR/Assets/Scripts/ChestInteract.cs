using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ChestInventory))]
public class ChestInteract : MonoBehaviour
{
    [Header("Referensi UI")]
    public ChestUI chestUIManager;   // Script pengatur UI Peti
    public GameObject chestUIPanel;  // Panel layarnya

    // Fungsi ini akan dipanggil oleh sistem interaksi VR atau lewat tombol tes
    [ContextMenu("Tes Buka Peti (Klik Kanan)")]
    public void OpenChest()
    {
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
        // Jika kita menekan tombol 'F' di keyboard menggunakan Input System baru
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            OpenChest();
        }
    }
}
