using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryToggle : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Masukkan GameObject Panel Inventory kamu di sini")]
    public GameObject inventoryPanel; 
    
    // --- TAMBAHAN BARU: Referensi ke PlayerInventory untuk auto-buka Peti ---
    public PlayerInventory playerInventory;

    [Header("Input VR")]
    [Tooltip("Pilih tombol dari VR Controller, misalnya: XRI LeftHand/Primary Button")]
    public InputActionReference toggleButton;

    private void OnEnable()
    {
        // Mengaktifkan action dan mendaftarkan event saat tombol mulai ditekan
        if (toggleButton != null)
        {
            toggleButton.action.Enable();
            toggleButton.action.started += ToggleUI;
        }
    }

    private void OnDisable()
    {
        // Melepas event saat script mati (mencegah error memori)
        if (toggleButton != null)
        {
            toggleButton.action.started -= ToggleUI;
        }
    }

    private void ToggleUI(InputAction.CallbackContext context)
    {
        if (inventoryPanel != null)
        {
            // Membalikkan status aktif/nonaktif dari panel inventory
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);

            // --- TAMBAHAN BARU: Otomatisasi buka/tutup Peti ---
            if (playerInventory != null && playerInventory.nearbyChest != null)
            {
                if (!isActive) // Jika tas baru DIBUKA (karena sebelumnya tidak aktif)
                {
                    playerInventory.nearbyChest.OpenChest();
                }
                else // Jika tas DITUTUP
                {
                    playerInventory.nearbyChest.chestUIPanel.SetActive(false);
                }
            }
        }
    }
}
