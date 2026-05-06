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
                priceText.color = Color.yellow; // Ubah warna jadi kuning agar seolah-olah itu harga koin
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