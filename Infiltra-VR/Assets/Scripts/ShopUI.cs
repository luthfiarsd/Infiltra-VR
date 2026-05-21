/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Referensi")]
    public PlayerInventory playerInventory; // Akses ke tas dan uang pemain
    public Transform itemGrid; // Tempat spawn area toko
    public GameObject slotPrefab; // Boleh pakai prefab yang sama (InventorySlot)

    [Header("Barang yang Dijual")]
    public List<ItemData> itemsForSale = new List<ItemData>(); // Daftar jualan

    public void RefreshUI()
    {
        // 1. Bersihkan jualan sebelumnya (jika ada)
        foreach (Transform child in itemGrid)
        {
            Destroy(child.gameObject);
        }

        // 2. Munculkan daftar barang jualan
        foreach (var itemData in itemsForSale)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemGrid);
            
            // Set gambar
            Transform iconTransform = newSlot.transform.Find("ItemIcon");
            if (iconTransform != null)
                iconTransform.GetComponent<Image>().sprite = itemData.itemIcon;

            // Berhubung ini toko, teks "Amount" kita gunakan untuk menampilkan Harga
            Transform textTransform = newSlot.transform.Find("AmountText");
            if (textTransform != null)
            {
                TextMeshProUGUI priceText = textTransform.GetComponent<TextMeshProUGUI>();
                priceText.text = "$" + itemData.buyPrice.ToString();
                priceText.color = Color.blue; // Ubah warna jadi kuning agar seolah-olah itu harga koin
            }

            // --- Logika Beli saat diklik ---
            Button slotButton = newSlot.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(() => BuyItem(itemData));
            }
        }
    }

    public void BuyItem(ItemData itemToBuy)
    {
        // Cek apakah uangnya cukup
        if (playerInventory.uang >= itemToBuy.buyPrice)
        {
            playerInventory.uang -= itemToBuy.buyPrice; // Potong uangnya
            playerInventory.AddItem(itemToBuy, 1);      // Masukkan barang 1 buah ke tas pemain

            // Munculkan pesan sukses di Console Unity (Bawah)
            Debug.Log("Berhasil membeli: " + itemToBuy.itemName + " | Sisa Uang: $" + playerInventory.uang);
            
            // Beritahu tas pemain untuk refresh jika sedang terbuka di belakang layar
            InventoryUI inventoryUI = playerInventory.GetComponentInChildren<InventoryUI>();
            if (inventoryUI != null) inventoryUI.RefreshUI();
        }
        else
        {
            // Jika uang tidak cukup
            Debug.LogWarning("Uang tidak cukup! Harga: $" + itemToBuy.buyPrice + " | Uang kamu: $" + playerInventory.uang);
        }
    }

    // Dipanggil setiap kali layar toko dibuka
    private void OnEnable()
    {
        RefreshUI();
    }
}

*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Referensi Utama")]
    public PlayerInventory playerInventory; 
    public Transform itemGrid; 
    public GameObject slotPrefab; 

    [Header("Barang yang Dijual (Statis)")]
    public List<ItemData> itemsForSale = new List<ItemData>(); 

    [Header("Referensi UI Panel Detail (Kanan)")]
    public TextMeshProUGUI judulText; 
    public TextMeshProUGUI qtyText;   
    public TextMeshProUGUI totalText; 
    public Button btnBeli;            
    
    // TAMBAHAN: Referensi teks untuk menampilkan info saat hover
    public TextMeshProUGUI hoverInfoText; 

    private ItemData selectedItem;
    private int currentQty = 1;

    private void Start()
    {
        ClearDetailPanel();
    }

    public void RefreshUI()
    {
        foreach (Transform child in itemGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var itemData in itemsForSale)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemGrid);
            
            // Set gambar ikon
            Transform iconTransform = newSlot.transform.Find("ItemIcon");
            if (iconTransform != null)
                iconTransform.GetComponent<Image>().sprite = itemData.itemIcon;

            // Menampilkan Harga
            Transform textTransform = newSlot.transform.Find("AmountText");
            if (textTransform != null)
            {
                TextMeshProUGUI priceText = textTransform.GetComponent<TextMeshProUGUI>();
                priceText.text = itemData.buyPrice.ToString();
                priceText.color = Color.yellow; 
            }

            // Klik = Memilih barang untuk dibeli
            Button slotButton = newSlot.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(() => SelectItem(itemData));
            }

            // TAMBAHAN: Setup Event Trigger untuk mendeteksi Hover Laser VR
            SetupHoverEvent(newSlot, itemData);
        }
    }

    // Fungsi baru untuk menyambungkan hover laser ke kode C#
    private void SetupHoverEvent(GameObject slotObj, ItemData itemData)
    {
        UnityEngine.EventSystems.EventTrigger trigger = slotObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = slotObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        // 1. Saat Laser Menunjuk (PointerEnter)
        UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnLaserHoverEnter(itemData); });
        trigger.triggers.Add(entryEnter);

        // 2. Saat Laser Keluar (PointerExit)
        UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnLaserHoverExit(); });
        trigger.triggers.Add(entryExit);
    }

    // Dipanggil otomatis saat laser VR menunjuk ke kotak pohon
    public void OnLaserHoverEnter(ItemData itemData)
    {
        // Asumsi di ItemData kamu ada variabel 'itemName' dan 'dayaSerap' (misal tipe float/int/string)
        // Jika nama variabel daya serapmu berbeda, silakan sesuaikan namanya di bawah ini (misal: itemData.waterAbsorption)
        hoverInfoText.text = $"{itemData.itemName}\nDaya Serap: {itemData.waterAbsorption} Liter";
    }

    // Dipanggil otomatis saat laser VR tidak lagi menunjuk kotak tersebut
    public void OnLaserHoverExit()
    {
        hoverInfoText.text = ""; // Kosongkan kembali teks saat laser pergi
    }

    public void SelectItem(ItemData itemData)
    {
        selectedItem = itemData;
        currentQty = 1; 
        UpdateDetailDisplay();
    }

    private void UpdateDetailDisplay()
    {
        if (selectedItem == null) return;

        judulText.text = selectedItem.itemName;
        qtyText.text = currentQty.ToString();

        int totalHarga = selectedItem.buyPrice * currentQty;
        totalText.text = "Rp " + totalHarga.ToString();

        btnBeli.interactable = true;
    }

    public void TambahQty()
    {
        if (selectedItem == null) return;
        currentQty++;
        UpdateDetailDisplay();
    }

    public void KurangQty()
    {
        if (selectedItem == null || currentQty <= 1) return; 
        currentQty--;
        UpdateDetailDisplay();
    }

    public void BuyItemFromPanel()
    {
        if (selectedItem == null) return;

        int totalHarga = selectedItem.buyPrice * currentQty;

        if (playerInventory.uang >= totalHarga)
        {
            playerInventory.uang -= totalHarga; 
            playerInventory.AddItem(selectedItem, currentQty); 

            Debug.Log($"Berhasil membeli: {currentQty} {selectedItem.itemName} | Sisa Uang: Rp {playerInventory.uang}");
            
            InventoryUI inventoryUI = playerInventory.GetComponentInChildren<InventoryUI>();
            if (inventoryUI != null) inventoryUI.RefreshUI();

            ClearDetailPanel();
        }
        else
        {
            Debug.LogWarning($"Uang tidak cukup! Total: Rp {totalHarga} | Uang kamu: Rp {playerInventory.uang}");
        }
    }

    private void ClearDetailPanel()
    {
        selectedItem = null;
        judulText.text = "Pilih Pohon";
        qtyText.text = "-";
        totalText.text = "Rp 0";
        btnBeli.interactable = false;
        if (hoverInfoText != null) hoverInfoText.text = "";
    }

    private void OnEnable()
    {
        RefreshUI();
    }
}