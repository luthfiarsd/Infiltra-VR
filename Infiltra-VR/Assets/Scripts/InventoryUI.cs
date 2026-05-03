using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public Transform itemGrid; // Tempat slot akan di-spawn
    public GameObject slotPrefab; // Objek cetakan slot item

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
}
