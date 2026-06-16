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
            bool newState = !isActive;
            inventoryPanel.SetActive(newState);

            // --- TAMBAHAN BARU: Pemosisian UI Melayang di Depan Mata VR ---
            if (newState) // Jika panel baru saja DIBUKA
            {
                PositionPanelInFrontOfPlayer();
            }

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

    // --- FUNGSI BARU: Menaruh Panel Selalu Mengikuti Arah Pandangan Mata VR ---
    private void PositionPanelInFrontOfPlayer()
    {
        if (Camera.main == null) return;

        Transform camTransform = Camera.main.transform;
        
        // 1. Taruh posisi panel sekitar 1.2 meter tepat di depan arah pandang Kamera VR
        Vector3 targetPosition = camTransform.position + (camTransform.forward * 1.2f);
        
        // 2. Setel tingginya sedikit di bawah mata (sejajar dada) agar tidak terlalu mendongak ke atas
        targetPosition.y = camTransform.position.y - 0.1f; 
        inventoryPanel.transform.position = targetPosition;

        // 3. Hadapkan panel lurus ke wajah pemain (Efek Billboard)
        Vector3 lookAtTarget = camTransform.position;
        lookAtTarget.y = inventoryPanel.transform.position.y; // Kunci sumbu Y agar panel tidak mendongak/nunduk kaku
        
        inventoryPanel.transform.LookAt(lookAtTarget);
        inventoryPanel.transform.Rotate(0, 180, 0); // Balik 180 derajat agar teks Canvas UI-mu tidak tercermin terbalik
    }
}