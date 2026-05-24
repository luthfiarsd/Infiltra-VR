using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class InventoryItemStorer : MonoBehaviour
{
    [Header("Item Data Referensi")]
    [Tooltip("Data barang (ScriptableObject) yang akan dimasukkan ke dalam inventory.")]
    public ItemData itemData;

    [Header("Visual & Audio Effects")]
    public AudioClip storeSound;
    public GameObject storeEffectPrefab;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private PlayerInventory playerInventory;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        // Cari PlayerInventory yang ada di scene (biasanya menempel di objek Player/GameManager)
        playerInventory = FindAnyObjectByType<PlayerInventory>();
    }

    private void OnEnable()
    {
        // Dengarkan event saat trigger ditekan (activated) pada barang ini
        grabInteractable.activated.AddListener(StoreItemInInventory);
    }

    private void OnDisable()
    {
        grabInteractable.activated.RemoveListener(StoreItemInInventory);
    }

    private void StoreItemInInventory(ActivateEventArgs args)
    {
        if (itemData != null && playerInventory != null)
        {
            // 1. Masukkan barang ke tas
            playerInventory.AddItem(itemData, 1);

            // 2. Jika UI Inventory sedang terbuka, refresh agar datanya langsung muncul
            InventoryUI ui = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
            if (ui != null && ui.gameObject.activeInHierarchy)
            {
                ui.RefreshUI();
            }

            // 3. Efek Suara
            if (storeSound != null)
            {
                // Putar suara di lokasi ini sebelum objek hancur menggunakan sistem suara di dunia
                AudioSource.PlayClipAtPoint(storeSound, transform.position);
            }

            // 4. Efek Visual
            if (storeEffectPrefab != null)
            {
                Instantiate(storeEffectPrefab, transform.position, Quaternion.identity);
            }

            // 5. Hancurkan barang dari dunia nyata
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("ItemData atau PlayerInventory belum terpasang dengan benar pada: " + gameObject.name);
        }
    }
}
