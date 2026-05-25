using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int buyPrice; // Harga beli di Shop
    public int sellPrice; // Harga jual
    public int waterAbsorption; // tambahan nada
    
    [Header("3D Representation")]
    public GameObject itemPrefab; // Prefab untuk dimunculkan di dunia nyata
}
