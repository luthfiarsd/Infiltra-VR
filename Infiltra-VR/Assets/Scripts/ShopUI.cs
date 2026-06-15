using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [System.Serializable]
    public class CartItem
    {
        public ItemData item;
        public int qty;

        public CartItem(ItemData item, int qty)
        {
            this.item = item;
            this.qty = qty;
        }
    }

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
    
    [Header("Referensi UI Cart")]
    public TextMeshProUGUI cartSummaryText; 
    public TextMeshProUGUI cartTotalText; // Teks khusus untuk menampilkan total harga belanjaan di keranjang
    public Button addToCartButton; 
    public Button clearCartButton; 
    public Button checkoutButton; 

    [Header("Referensi UI Detail Hover")]
    public TextMeshProUGUI hoverInfoText; 

    private ItemData selectedItem;
    private int currentQty = 1;

    // Menyimpan daftar belanjaan di keranjang
    private List<CartItem> cartList = new List<CartItem>();

    private void Awake()
    {
        // Cari otomatis PlayerInventory jika lupa dimasukkan di Inspector
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        // Pasang event klik tombol-tombol secara otomatis via script
        if (addToCartButton != null)
        {
            addToCartButton.onClick.RemoveAllListeners();
            addToCartButton.onClick.AddListener(AddSelectedItemToCart);
        }

        if (clearCartButton != null)
        {
            clearCartButton.onClick.RemoveAllListeners();
            clearCartButton.onClick.AddListener(ClearCart);
        }

        if (checkoutButton != null)
        {
            checkoutButton.onClick.RemoveAllListeners();
            checkoutButton.onClick.AddListener(CheckoutCart);
        }
    }

    private void Start()
    {
        ClearDetailPanel();
        UpdateCartDisplay();
    }

    public void RefreshUI()
    {
        // Bersihkan grid jualan sebelumnya
        foreach (Transform child in itemGrid)
        {
            Destroy(child.gameObject);
        }

        // Munculkan daftar barang jualan
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

            // Setup Event Trigger untuk mendeteksi Hover Laser VR
            SetupHoverEvent(newSlot, itemData);
        }
    }

    private void SetupHoverEvent(GameObject slotObj, ItemData itemData)
    {
        UnityEngine.EventSystems.EventTrigger trigger = slotObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = slotObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        trigger.triggers.Clear();

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

    public void OnLaserHoverEnter(ItemData itemData)
    {
        if (hoverInfoText != null)
        {
            hoverInfoText.text = $"{itemData.itemName}\nDaya Serap: {itemData.waterAbsorption} Liter";
        }
    }

    public void OnLaserHoverExit()
    {
        if (hoverInfoText != null)
        {
            hoverInfoText.text = ""; 
        }
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

        if (judulText != null) judulText.text = selectedItem.itemName;
        if (qtyText != null) qtyText.text = currentQty.ToString();

        int totalHarga = selectedItem.buyPrice * currentQty;
        if (cartTotalText != null && totalText != null) totalText.text = totalHarga.ToString();

        if (addToCartButton != null)
        {
            addToCartButton.interactable = true;
        }
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

    // --- LOGIKA UTAMA SISTEM KERANJANG (CART SYSTEM) ---

    public void AddSelectedItemToCart()
    {
        if (selectedItem == null) return;

        // Cek apakah barang yang sama sudah ada di keranjang
        CartItem existingItem = cartList.Find(x => x.item == selectedItem);
        if (existingItem != null)
        {
            existingItem.qty += currentQty;
        }
        else
        {
            cartList.Add(new CartItem(selectedItem, currentQty));
        }

        Debug.Log($"[ShopUI] Menambahkan {currentQty} {selectedItem.itemName} ke keranjang.");
        
        ClearDetailPanel();
        UpdateCartDisplay();
    }

    public void ClearCart()
    {
        cartList.Clear();
        Debug.Log("[ShopUI] Keranjang belanja dikosongkan.");
        UpdateCartDisplay();
    }

    public void CheckoutCart()
    {
        if (cartList.Count == 0)
        {
            Debug.LogWarning("[ShopUI] Keranjang kosong, tidak bisa checkout.");
            return;
        }

        // Hitung total harga seluruh isi keranjang
        int totalBayar = GetCartTotal();

        // Cek uang pemain
        if (playerInventory != null && playerInventory.uang >= totalBayar)
        {
            playerInventory.uang -= totalBayar;
            if (GameManager.Instance != null) GameManager.Instance.UpdateUI();

            // Masukkan seluruh isi keranjang ke tas pemain
            foreach (var cartItem in cartList)
            {
                playerInventory.AddItem(cartItem.item, cartItem.qty);
                Debug.Log($"[ShopUI] Checkout: Membeli {cartItem.qty} {cartItem.item.itemName}");
            }

            // Refresh UI Inventory
            InventoryUI inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
            if (inventoryUI != null)
            {
                inventoryUI.RefreshUI();
            }

            cartList.Clear();
            ClearDetailPanel();
            
            if (cartSummaryText != null)
            {
                cartSummaryText.text = "<color=green>Pembelian berhasil!</color>";
            }
            if (cartTotalText != null)
            {
                cartTotalText.text = "0";
            }
            else if (totalText != null)
            {
                totalText.text = "0";
            }

            Debug.Log($"[ShopUI] Checkout Berhasil! Total Bayar: {totalBayar} | Sisa Uang: {playerInventory.uang}");
        }
        else
        {
            Debug.LogWarning($"[ShopUI] Uang tidak cukup! Total: {totalBayar} | Uang pemain: {(playerInventory != null ? playerInventory.uang : 0)}");
        }
    }

    private int GetCartTotal()
    {
        int total = 0;
        foreach (var cartItem in cartList)
        {
            total += cartItem.item.buyPrice * cartItem.qty;
        }
        return total;
    }

    private void UpdateCartDisplay()
    {
        if (cartSummaryText == null) return;

        if (cartList.Count == 0)
        {
            cartSummaryText.text = "Keranjang kosong";
            if (cartTotalText != null) cartTotalText.text = "0";
            else if (totalText != null) totalText.text = "0";
            if (checkoutButton != null) checkoutButton.interactable = false;
            if (clearCartButton != null) clearCartButton.interactable = false;
        }
        else
        {
            string summary = "";
            foreach (var cartItem in cartList)
            {
                // Format: [NamaBarang] [Jumlah]x [HargaPerUnit]
                summary += $"{cartItem.item.itemName} {cartItem.qty}x {cartItem.item.buyPrice}\n";
            }
            cartSummaryText.text = summary;

            if (cartTotalText != null)
            {
                // Menampilkan total murni tanpa simbol Rupiah
                cartTotalText.text = GetCartTotal().ToString();
            }
            else if (totalText != null)
            {
                totalText.text = GetCartTotal().ToString();
            }
            
            if (checkoutButton != null) checkoutButton.interactable = true;
            if (clearCartButton != null) clearCartButton.interactable = true;
        }
    }

    public void ClearDetailPanel()
    {
        selectedItem = null;
        currentQty = 1;

        if (judulText != null) judulText.text = "Pilih Pohon";
        if (qtyText != null) qtyText.text = "-";
        if (cartTotalText != null && totalText != null) totalText.text = "0";
        
        if (addToCartButton != null)
        {
            addToCartButton.interactable = false;
        }

        if (hoverInfoText != null) 
        {
            hoverInfoText.text = "";
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }
}