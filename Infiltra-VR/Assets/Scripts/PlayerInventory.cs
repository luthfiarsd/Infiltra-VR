using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Class untuk mewakili satu tumpukan barang
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int amount;
    }

    // Daftar barang yang dimiliki pemain
    public List<InventorySlot> items = new List<InventorySlot>();

    // --- TAMBAHAN BARU: Sistem Uang ---
    public int uang = 100; // Uang modal awal pemain

    // Fungsi untuk menambah item
    public void AddItem(ItemData newItem, int amountToAdd)
    {
        // Cek apakah item sudah ada di inventory
        foreach (InventorySlot slot in items)
        {
            if (slot.item == newItem)
            {
                slot.amount += amountToAdd; // Tambah jumlahnya
                return;
            }
        }
        // Jika belum ada, buat slot baru
        items.Add(new InventorySlot { item = newItem, amount = amountToAdd });
    }
}
