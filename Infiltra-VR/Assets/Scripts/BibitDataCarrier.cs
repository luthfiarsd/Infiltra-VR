using UnityEngine;

/// <summary>
/// Komponen pembawa data ItemData pada objek 3D yang di-spawn dari inventory.
/// Di-attach otomatis oleh InventoryUI saat item dimunculkan ke dunia.
/// TanahBerkebun membaca komponen ini untuk mengidentifikasi jenis bibit yang ditanam.
/// </summary>
public class BibitDataCarrier : MonoBehaviour
{
    [HideInInspector]
    public ItemData itemData;
}
