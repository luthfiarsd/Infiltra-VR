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

    private void Awake()
    {
        // Mencegah duplikasi komponen InventoryToggle pada GameObject yang sama (menghindari double-toggle bug)
        InventoryToggle[] toggles = GetComponents<InventoryToggle>();
        if (toggles.Length > 1 && toggles[0] != this)
        {
            Debug.LogWarning("[InventoryToggle] Menghapus duplikasi komponen pada GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }

        // Cari otomatis panel inventory jika kosong
        if (inventoryPanel == null)
        {
            RectTransform[] allPanels = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (RectTransform rt in allPanels)
            {
                if (rt.name == "PlayerInventoryPanel")
                {
                    inventoryPanel = rt.gameObject;
                    break;
                }
            }
        }
        
        // Cari otomatis player inventory jika kosong
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

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
