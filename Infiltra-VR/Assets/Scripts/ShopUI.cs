using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [System.Serializable]
    public class CartEntry
    {
        public ItemData item;
        public int quantity;
    }

    [Header("Referensi Utama")]
    public PlayerInventory playerInventory;
    public Transform itemGrid;
    public GameObject slotPrefab;

    [Header("Barang yang Dijual")]
    public List<ItemData> itemsForSale = new List<ItemData>();

    [Header("Panel Detail")]
    public TextMeshProUGUI judulText;
    public TextMeshProUGUI qtyText;
    public TextMeshProUGUI totalText;
    public Button btnBeli;
    public Button addToCartButton;
    public TextMeshProUGUI hoverInfoText;

    [Header("Keranjang")]
    public TextMeshProUGUI cartSummaryText;
    public Button checkoutButton;
    public Button clearCartButton;

    [Header("Panel Otomatis Tambahan")]
    [SerializeField] bool buildPolishedLayout = true;
    [SerializeField] bool autoWireButtons = false;
    [SerializeField] Color cartTextColor = new Color(1f, 0.96f, 0.78f, 1f);
    [SerializeField, Range(40f, 95f)] float cartQuantityColumnPercent = 68f;

    ItemData selectedItem;
    int currentQty = 1;
    readonly List<CartEntry> cart = new List<CartEntry>();

    void Awake()
    {
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();

        if (buildPolishedLayout)
            EnsureExtraInfoPanels();

        EnsureCartControls();

        if (autoWireButtons)
            WireButtons();
    }

    void Start()
    {
        ClearDetailPanel();
        UpdateCartDisplay();
    }

    void OnEnable()
    {
        RefreshUI();
        UpdateCartDisplay();
    }

    public void RefreshUI()
    {
        if (itemGrid == null || slotPrefab == null)
            return;

        foreach (Transform child in itemGrid)
            Destroy(child.gameObject);

        foreach (var itemData in itemsForSale)
        {
            if (itemData == null)
                continue;

            var newSlot = Instantiate(slotPrefab, itemGrid);
            SetSlotIcon(newSlot, itemData);
            SetSlotPrice(newSlot, itemData);

            var slotButton = newSlot.GetComponent<Button>();
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => SelectItem(itemData));
            }

            SetupHoverEvent(newSlot, itemData);
        }
    }

    public void SelectItem(ItemData itemData)
    {
        selectedItem = itemData;
        currentQty = 1;
        UpdateDetailDisplay();
    }

    public void TambahQty()
    {
        if (selectedItem == null)
            return;

        currentQty++;
        UpdateDetailDisplay();
    }

    public void KurangQty()
    {
        if (selectedItem == null || currentQty <= 1)
            return;

        currentQty--;
        UpdateDetailDisplay();
    }

    public void AddSelectedItemToCart()
    {
        if (selectedItem == null || currentQty <= 0)
            return;

        AddToCart(selectedItem, currentQty);
        Debug.Log($"Masuk keranjang: {currentQty} {selectedItem.itemName}");

        currentQty = 1;
        UpdateDetailDisplay();
        UpdateCartDisplay();
    }

    public void CheckoutCart()
    {
        if (cart.Count == 0)
        {
            Debug.LogWarning("Keranjang masih kosong.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory belum terhubung ke ShopUI.");
            return;
        }

        var total = GetCartTotal();
        if (playerInventory.uang < total)
        {
            Debug.LogWarning($"Uang tidak cukup! Total: Rp {total} | Uang kamu: Rp {playerInventory.uang}");
            return;
        }

        playerInventory.uang -= total;

        foreach (var entry in cart)
        {
            if (entry.item != null && entry.quantity > 0)
                playerInventory.AddItem(entry.item, entry.quantity);
        }

        Debug.Log($"Checkout berhasil. Total: Rp {total} | Sisa uang: Rp {playerInventory.uang}");
        cart.Clear();

        var inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (inventoryUI != null)
            inventoryUI.RefreshUI();

        ClearDetailPanel();
        UpdateCartDisplay();
    }

    public void ClearCart()
    {
        cart.Clear();
        UpdateCartDisplay();
    }

    public void BuyItemFromPanel()
    {
        AddSelectedItemToCart();
    }

    public void OnLaserHoverEnter(ItemData itemData)
    {
        if (hoverInfoText == null || itemData == null)
            return;

        var statsSource = itemData.grownTreeData != null ? itemData.grownTreeData : itemData;
        var absorption = statsSource != null ? statsSource.waterAbsorption : 0;
        var bonus = statsSource != null ? statsSource.waveRewardBonus : 0;

        hoverInfoText.text =
            $"{itemData.itemName}\n" +
            $"Harga: Rp {itemData.buyPrice}\n" +
            $"Daya Serap: {absorption} Liter\n" +
            $"Bonus Wave: Rp {bonus}";
    }

    public void OnLaserHoverExit()
    {
        if (hoverInfoText != null)
            hoverInfoText.text = "";
    }

    void AddToCart(ItemData item, int quantity)
    {
        foreach (var entry in cart)
        {
            if (entry.item == item)
            {
                entry.quantity += quantity;
                return;
            }
        }

        cart.Add(new CartEntry { item = item, quantity = quantity });
    }

    void UpdateDetailDisplay()
    {
        if (selectedItem == null)
            return;

        if (judulText != null)
            judulText.text = selectedItem.itemName;

        if (qtyText != null)
            qtyText.text = currentQty.ToString();

        if (totalText != null)
            totalText.text = $"Total: Rp {GetCartTotal()}";

        var addButton = GetAddToCartButton();
        if (addButton != null)
            addButton.interactable = true;
    }

    void UpdateCartDisplay()
    {
        var total = GetCartTotal();

        if (cartSummaryText != null)
        {
            if (cart.Count == 0)
            {
                cartSummaryText.text = "Keranjang kosong";
            }
            else
            {
                var builder = new StringBuilder();

                foreach (var entry in cart)
                {
                    if (entry.item == null)
                        continue;

                    builder.AppendLine($"{GetCartDisplayName(entry.item)}<pos={cartQuantityColumnPercent}%>x{entry.quantity}");
                }
                cartSummaryText.text = builder.ToString();
            }
        }

        if (checkoutButton != null)
            checkoutButton.interactable = cart.Count > 0 && playerInventory != null && playerInventory.uang >= total;

        if (clearCartButton != null)
            clearCartButton.interactable = cart.Count > 0;

        if (selectedItem != null && totalText != null)
            totalText.text = $"Total: Rp {total}";
    }

    int GetCartTotal()
    {
        var total = 0;

        foreach (var entry in cart)
        {
            if (entry.item != null && entry.quantity > 0)
                total += entry.item.buyPrice * entry.quantity;
        }

        return total;
    }

    string GetCartDisplayName(ItemData item)
    {
        if (item == null || string.IsNullOrEmpty(item.itemName))
            return "-";

        const int maxLength = 16;
        if (item.itemName.Length <= maxLength)
            return item.itemName;

        return item.itemName.Substring(0, maxLength - 3) + "...";
    }

    void ClearDetailPanel()
    {
        selectedItem = null;
        currentQty = 1;

        if (judulText != null)
            judulText.text = "Pilih Barang";

        if (qtyText != null)
            qtyText.text = "-";

        if (totalText != null)
            totalText.text = "Total: Rp " + GetCartTotal();

        var addButton = GetAddToCartButton();
        if (addButton != null)
            addButton.interactable = false;

        if (hoverInfoText != null)
            hoverInfoText.text = "";
    }

    void WireButtons()
    {
        var addButton = GetAddToCartButton();
        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(AddSelectedItemToCart);
            SetButtonLabel(addButton, "Add To Cart");
        }

        if (checkoutButton != null)
        {
            checkoutButton.onClick.RemoveAllListeners();
            checkoutButton.onClick.AddListener(CheckoutCart);
            SetButtonLabel(checkoutButton, "Beli");
        }

        if (clearCartButton != null)
        {
            clearCartButton.onClick.RemoveAllListeners();
            clearCartButton.onClick.AddListener(ClearCart);
            SetButtonLabel(clearCartButton, "Kosongkan");
        }
    }

    void EnsureCartControls()
    {
        var addButton = GetAddToCartButton();
        var fallbackParent = qtyText != null ? qtyText.transform.parent : addButton != null ? addButton.transform.parent : transform;

        if (cartSummaryText == null)
        {
            cartSummaryText = CreateCartSummaryText(fallbackParent);
        }
        else if (buildPolishedLayout)
        {
            ConfigureCartSummaryText(cartSummaryText);
        }

        if (checkoutButton == null && btnBeli != null && btnBeli != addButton)
            checkoutButton = btnBeli;
        else if (checkoutButton == null && addButton != null)
            checkoutButton = CreateButtonFromTemplate(addButton, "CheckoutButton", new Vector2(0f, -42f));

        if (clearCartButton == null && addButton != null)
            clearCartButton = CreateButtonFromTemplate(addButton, "ClearCartButton", new Vector2(0f, -84f));
    }

    Button GetAddToCartButton()
    {
        return addToCartButton != null ? addToCartButton : btnBeli;
    }

    TextMeshProUGUI CreateCartSummaryText(Transform parent)
    {
        var textObject = new GameObject("CartSummaryText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        var text = textObject.GetComponent<TextMeshProUGUI>();
        ConfigureCartSummaryText(text);

        return text;
    }

    Button CreateButtonFromTemplate(Button template, string objectName, Vector2 offset)
    {
        var newButton = Instantiate(template, template.transform.parent);
        newButton.name = objectName;

        var rect = newButton.GetComponent<RectTransform>();
        if (rect != null)
            rect.anchoredPosition += offset;

        return newButton;
    }

    void SetupHoverEvent(GameObject slotObj, ItemData itemData)
    {
        var trigger = slotObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = slotObj.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener(_ => OnLaserHoverEnter(itemData));
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener(_ => OnLaserHoverExit());
        trigger.triggers.Add(entryExit);
    }

    void SetSlotIcon(GameObject slot, ItemData itemData)
    {
        var iconTransform = slot.transform.Find("ItemIcon");
        if (iconTransform == null)
            return;

        var icon = iconTransform.GetComponent<Image>();
        if (icon != null)
            icon.sprite = itemData.itemIcon;
    }

    void SetSlotPrice(GameObject slot, ItemData itemData)
    {
        var textTransform = slot.transform.Find("AmountText");
        if (textTransform == null)
            return;

        var priceText = textTransform.GetComponent<TextMeshProUGUI>();
        if (priceText == null)
            return;

        priceText.text = itemData.buyPrice.ToString();
        priceText.color = Color.yellow;
    }

    void SetButtonLabel(Button button, string label)
    {
        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
            text.text = label;
    }

    void EnsureExtraInfoPanels()
    {
        ConfigureHoverInfoText();
    }

    void ConfigureHoverInfoText()
    {
        if (hoverInfoText == null)
            return;

        hoverInfoText.color = Color.white;
        hoverInfoText.fontSize = Mathf.Clamp(hoverInfoText.fontSize > 0 ? hoverInfoText.fontSize : 24f, 20f, 26f);
        hoverInfoText.alignment = TextAlignmentOptions.TopLeft;
        hoverInfoText.textWrappingMode = TextWrappingModes.Normal;
        hoverInfoText.overflowMode = TextOverflowModes.Overflow;

        var hoverTextRect = hoverInfoText.GetComponent<RectTransform>();
        if (hoverTextRect != null)
        {
            hoverTextRect.SetAsLastSibling();
            hoverTextRect.sizeDelta = new Vector2(300f, 78f);
        }
    }

    void ConfigureCartSummaryText(TextMeshProUGUI text)
    {
        if (text == null)
            return;

        text.color = cartTextColor;
        text.fontSize = 20f;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Overflow;
        text.lineSpacing = -8f;

        if (totalText != null)
            text.font = totalText.font;

        var rect = text.GetComponent<RectTransform>();
        if (rect == null)
            return;

        var qtyRect = qtyText != null ? qtyText.GetComponent<RectTransform>() : null;
        if (qtyRect != null)
        {
            rect.anchorMin = qtyRect.anchorMin;
            rect.anchorMax = qtyRect.anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = qtyRect.anchoredPosition + new Vector2(-95f, -245f);
            rect.sizeDelta = new Vector2(390f, 190f);
            rect.localScale = new Vector3(3.2f, 1.05f, 1f);
            rect.localRotation = Quaternion.identity;
        }
        else
        {
            rect.sizeDelta = new Vector2(320f, 160f);
        }

        rect.SetAsLastSibling();
    }

    void ReparentIfAvailable(TextMeshProUGUI text, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (text == null)
            return;

        text.transform.SetParent(parent, false);
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;

        var rect = text.GetComponent<RectTransform>();
        StretchRect(rect, anchorMin, anchorMax, new Vector2(8f, 4f), new Vector2(-8f, -4f));
    }

    void ReparentButton(Button button, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        button.transform.SetParent(parent, false);

        var rect = button.GetComponent<RectTransform>();
        StretchRect(rect, anchorMin, anchorMax, new Vector2(8f, 4f), new Vector2(-8f, -4f));

        var image = button.GetComponent<Image>();
        if (image != null)
            image.color = new Color(0.29f, 0.52f, 0.28f, 0.95f);
    }

    static void StretchRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}
