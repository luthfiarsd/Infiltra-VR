using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    [Header("Referensi")]
    public ChestInventory currentChest; // Peti mana yang sedang terbuka
    public PlayerInventory playerInventory; // Referensi ke tas pemain untuk mentransfer barang
    public GameObject playerInventoryPanel; // Panel tas pemain agar otomatis terbuka
    public Transform itemGrid; // Tempat spawn ikon item
    public GameObject slotPrefab; // Objek cetakan item

    private RectTransform myRect;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    // Fungsi untuk memuat ulang tampilan item di dalam chest
    public void RefreshUI()
    {
        if (currentChest == null) return;

        // Bersihkan grid dari item sebelumnya
        foreach (Transform child in itemGrid)
        {
            Destroy(child.gameObject);
        }

        // Munculkan ulang sesuai isi dada petinya
        foreach (var slotData in currentChest.items)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemGrid);
            
            Transform iconTransform = newSlot.transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                iconTransform.GetComponent<Image>().sprite = slotData.item.itemIcon;
            }

            Transform textTransform = newSlot.transform.Find("AmountText");
            if (textTransform != null)
            {
                textTransform.GetComponent<TextMeshProUGUI>().text = slotData.amount.ToString();
            }

            // --- TAMBAHAN BARU: Membuat tombol bisa diklik untuk mengambil barang ---
            Button slotButton = newSlot.GetComponent<Button>();
            if (slotButton != null)
            {
                // Saat tombol diklik, panggil fungsi ambil barang (1 per 1)
                slotButton.onClick.AddListener(() => TakeItem(slotData));
            }
        }
    }

    // Fungsi untuk menyedot barang dari dada peti ke tas pemain
    public void TakeItem(PlayerInventory.InventorySlot chestSlot)
    {
        if (chestSlot.amount > 0)
        {
            // 1. Masukkan 1 barang ke tas pemain
            playerInventory.AddItem(chestSlot.item, 1);

            // Progress planting guide if player takes Cangkul
            if (chestSlot.item != null && chestSlot.item.itemName.ToLower().Contains("cangkul"))
            {
                if (GameManager.Instance != null && GameManager.Instance.plantingGuideTrigger != null)
                {
                    GameManager.Instance.plantingGuideTrigger.ProgressPlantingGuide(0);
                }
            }
            
            // 2. Kurangi 1 barang dari peti
            chestSlot.amount -= 1;

            // 3. Jika barang di peti sudah habis, buang slot tersebut dari peti
            if (chestSlot.amount <= 0)
            {
                currentChest.items.Remove(chestSlot);
            }

            // 4. Perbarui layar peti agar angkanya berkurang secara visual
            RefreshUI();

            // 5. Perbarui layar tas agar barang barunya langsung muncul
            if (playerInventoryPanel != null)
            {
                InventoryUI invUI = playerInventoryPanel.GetComponent<InventoryUI>();
                if (invUI != null)
                {
                    invUI.RefreshUI();
                }
            }
        }
    }

    // Terpanggil otomatis saat panel ini dinyalakan (muncul)
    private void OnEnable()
    {
        RefreshUI();
        
        // Otomatis membuka layar tas pemain (seperti Minecraft)
        if (playerInventoryPanel != null)
        {
            playerInventoryPanel.SetActive(true);
        }
    }

    // Terpanggil saat panel ini dimatikan (ditutup)
    private void OnDisable()
    {
        // Sengaja dibiarkan kosong
        // Kini layar peti mati TIDAK akan memaksa layar tas tertutup
    }
}