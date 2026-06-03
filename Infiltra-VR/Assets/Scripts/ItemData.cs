using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int buyPrice; // Harga beli di Shop
    public int sellPrice; // Harga jual
    public int waterAbsorption; // tambahan nada
    
    [Header("Wave Reward")]
    [Tooltip("Bonus uang saat menang wave dengan tanaman ini sudah dewasa")]
    public int waveRewardBonus;
    
    [Header("3D Representation")]
    public GameObject itemPrefab; // Prefab untuk dimunculkan di dunia nyata
    [Tooltip("Prefab visual saat ditanam di tanah (misal model sapling, jika berbeda dengan itemPrefab)")]
    public GameObject plantedVisualPrefab;

    [Header("Growth Link (For Seeds/Bibit)")]
    [Tooltip("Pohon dewasa yang dihasilkan dari bibit ini (opsional jika statistik langsung ditaruh di bibit)")]
    public ItemData grownTreeData;
}
