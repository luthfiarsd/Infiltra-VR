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

    // Fungsi untuk mengurangi item (untuk dipindah ke peti / dipakai)
    public void RemoveItem(ItemData itemToRemove, int amountToRemove)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == itemToRemove)
            {
                items[i].amount -= amountToRemove;
                
                // Jika barang habis, hapus dari list
                if (items[i].amount <= 0)
                {
                    items.RemoveAt(i);
                }
                return;
            }
        }
    }
}
