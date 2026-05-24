using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Transform itemGrid; // Tempat slot akan di-spawn
    public GameObject slotPrefab; // Objek cetakan slot item

    [Header("Referensi Lain (Opsional)")]
    public ChestUI chestUI; // Agar tas tau jika layar peti sedang terbuka

    private RectTransform myRect;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        
        // Coba cari otomatis jika belum dimasukkan di Inspector
        if (chestUI == null)
        {
            chestUI = FindAnyObjectByType<ChestUI>(FindObjectsInactive.Include);
        }
    }

    // Fungsi ini dipanggil untuk memperbarui tampilan UI
    public void RefreshUI()
    {
        // 1. Hapus semua slot lama yang ada di UI screen
        foreach (Transform child in itemGrid)
        {
            Destroy(child.gameObject);
        }

        // 2. Buat slot baru berdasarkan data dari PlayerInventory
        foreach (var slotData in playerInventory.items)
        {
            // Spawn prefab ke dalam Grid
            GameObject newSlot = Instantiate(slotPrefab, itemGrid);
            
            // Atur gambar icon
            Transform iconTransform = newSlot.transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                Image icon = iconTransform.GetComponent<Image>();
                icon.sprite = slotData.item.itemIcon;
            }

            // Atur teks jumlah
            Transform textTransform = newSlot.transform.Find("AmountText");
            if (textTransform != null)
            {
                TextMeshProUGUI amountText = textTransform.GetComponent<TextMeshProUGUI>();
                amountText.text = slotData.amount.ToString();
            }

            // --- TAMBAHAN BARU: Membuat tombol bisa diklik untuk memindah barang ---
            Button slotButton = newSlot.GetComponent<Button>();
            if (slotButton != null)
            {
                // Saat tombol diklik, panggil fungsi transfer barang
                slotButton.onClick.AddListener(() => TransferItemToChest(slotData));
            }
        }
    }

    // Fungsi otomatis berjalan saat GameObject (Panel Inventory) diaktifkan
    private void OnEnable()
    {
        if (playerInventory != null && itemGrid != null && slotPrefab != null)
        {
            RefreshUI();
        }
    }

    // Fungsi untuk memindah barang dari Tas ke Peti (jika peti sedang terbuka)
    public void TransferItemToChest(PlayerInventory.InventorySlot slotData)
    {
        // Pastikan UI peti ada, sedang aktif/nyala di layar, dan ada peti yang dipilih
        if (chestUI != null && chestUI.gameObject.activeInHierarchy && chestUI.currentChest != null)
        {
            if (slotData.amount > 0)
            {
                // 1. Tambahkan 1 barang ke dalam Peti
                chestUI.currentChest.AddItem(slotData.item, 1);
                
                // 2. Kurangi 1 barang dari Tas Pemain
                playerInventory.RemoveItem(slotData.item, 1);
                
                // 3. Perbarui kedua layar UI (Tas dan Peti)
                RefreshUI();
                chestUI.RefreshUI();
            }
        }
        else
        {
            Debug.Log("Kamu mengklik item: " + slotData.item.itemName + " di Inventory");
        }
    }
}
