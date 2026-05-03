using System.Collections.Generic;
using UnityEngine;

public class ChestInventory : MonoBehaviour
{
    // Menggunakan class InventorySlot yang sama dengan milik pemain
    public List<PlayerInventory.InventorySlot> items = new List<PlayerInventory.InventorySlot>();

    // Fungsi untuk menambah item ke dalam chest ini
    public void AddItem(ItemData newItem, int amountToAdd)
    {
        foreach (var slot in items)
        {
            if (slot.item == newItem)
            {
                slot.amount += amountToAdd;
                return;
            }
        }
        items.Add(new PlayerInventory.InventorySlot { item = newItem, amount = amountToAdd });
    }
}
