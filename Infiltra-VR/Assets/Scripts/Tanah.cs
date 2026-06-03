using UnityEngine;

public class TanahBerkebun : MonoBehaviour
{
    [Header("Status Tanah Saat Ini")]
    public int statusTanah = 0; 
    
    [Header("Slot Objek Visual (Dummy/Patung)")]
    public GameObject modelTanahBerlubang; 
    public GameObject modelBibit;          
    public GameObject modelTanahTertutup;  
    public GameObject modelTunas;          
    public GameObject modelPohonDewasa; // Model pohon saat sudah disiram pupuk

    [Header("Pengaturan Gameplay")]
    public int waterAbsorption = 15; // Jumlah air yang diserap pohon ini saat dewasa

    [Header("Pengaturan Ukuran")]
    [Tooltip("Multiplier untuk mengatur ukuran bibit & pohon secara manual")]
    public float scaleMultiplier = 1.0f;

    [Header("Data Tanaman yang Ditanam")]
    public ItemData plantedItemData; // Diisi otomatis saat bibit dimasukkan

    [Header("Dynamic Model Instances")]
    public GameObject activeBibitInstance;
    public GameObject activePohonInstance;

    private Vector3 originalBibitScale = Vector3.one;
    private Vector3 originalPohonScale = Vector3.one;
    [HideInInspector]
    public Vector3 targetPohonScale = Vector3.one;

    void Awake()
    {
        if (modelBibit != null)
        {
            originalBibitScale = modelBibit.transform.localScale;
        }
        if (modelPohonDewasa != null)
        {
            originalPohonScale = modelPohonDewasa.transform.localScale;
            targetPohonScale = originalPohonScale;
        }
    }

    void OnTriggerEnter(Collider bendaYangNyentuh)
    {
        // Kunci Negara - Sekarang dikunci setelah step 5 (dewasa)
        if (statusTanah == 5) return;

        // STEP 1: MENCANGKUL TANAH
        if (statusTanah == 0 && bendaYangNyentuh.CompareTag("cangkul")) 
        {
            statusTanah = 1;
            Debug.Log("Tanah berlubang!");
            if(modelTanahBerlubang != null) modelTanahBerlubang.SetActive(true);
        }

        // STEP 2: MASUKIN BIBIT
        else if (statusTanah == 1 && bendaYangNyentuh.CompareTag("bibit"))
        {
            statusTanah = 2;
            Debug.Log("Bibit masuk ke lubang!");

            // Ambil data ItemData dari BibitDataCarrier (dipasang otomatis oleh InventoryUI)
            BibitDataCarrier carrier = bendaYangNyentuh.GetComponent<BibitDataCarrier>();
            if (carrier != null && carrier.itemData != null)
            {
                plantedItemData = carrier.itemData;
                Debug.Log("Tanaman terdeteksi: " + plantedItemData.itemName);
            }

            // Nonaktifkan model default bibit (jika ada)
            if (modelBibit != null) modelBibit.SetActive(false);

            // Tentukan posisi & rotasi spawn di tingkat dunia (world space) menggunakan plot tanah langsung
            Vector3 spawnPos = transform.position;

            // Cari prefab bibit yang akan ditampilkan di tanah (utamakan plantedVisualPrefab)
            GameObject bibitVisualPrefab = null;
            if (plantedItemData != null)
            {
                bibitVisualPrefab = (plantedItemData.plantedVisualPrefab != null) ? plantedItemData.plantedVisualPrefab : plantedItemData.itemPrefab;
            }

            // Gunakan rotasi asli dari prefab secara langsung
            Quaternion spawnRot = (bibitVisualPrefab != null) ? bibitVisualPrefab.transform.rotation : Quaternion.identity;

            // Spawn model bibit spesifik dari ScriptableObject di tingkat dunia (tanpa parent)
            if (bibitVisualPrefab != null)
            {
                activeBibitInstance = Instantiate(bibitVisualPrefab, spawnPos, spawnRot);
                
                // Gunakan skala asli dari prefab secara langsung
                activeBibitInstance.transform.localScale = bibitVisualPrefab.transform.localScale;

                BekukanFisika(activeBibitInstance);
                activeBibitInstance.SetActive(true);
            }
            else if (modelBibit != null)
            {
                // Fallback ke model bibit default plot
                modelBibit.SetActive(true);
                activeBibitInstance = modelBibit;
            }

            Destroy(bendaYangNyentuh.gameObject); 
        }

        // STEP 3: TUTUP TANAH
        else if (statusTanah == 2 && bendaYangNyentuh.CompareTag("cangkul"))
        {
            statusTanah = 3;
            Debug.Log("Tanah ditutup rapat!");
            if(modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if(activeBibitInstance != null) activeBibitInstance.SetActive(true);
            if(modelTanahTertutup != null) modelTanahTertutup.SetActive(true);
        }

        // STEP 5: DIBERI PUPUK (Tumbuh Dewasa & Masuk ke GameManager)
        else if (statusTanah >= 2 && statusTanah <= 4 && bendaYangNyentuh.CompareTag("pupuk"))
        {
            TumbuhkanKeDewasa(startAtZeroScale: false);
            
            // Hancurkan item pupuk setelah dipakai
            Destroy(bendaYangNyentuh.gameObject);
        }
    }

    // --- STEP 4: FUNGSI BARU YANG DIPANGGIL RADAR TEKO ---
    public void DisiramOlehTeko()
    {
        // Kalau statusnya benar-benar lagi siap disiram (3)
        if (statusTanah == 3)
        {
            statusTanah = 4;
            Debug.Log("Disiram! Bibit tetap terlihat, menunggu pupuk untuk tumbuh dewasa.");
            
            if(modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            
            // Bibit tetap terlihat (modelBibit sudah aktif dari step 2)
            // Tidak ada perubahan visual — menunggu pupuk untuk tumbuh
        }
    }

    public void TumbuhkanKeDewasa(bool startAtZeroScale = false)
    {
        statusTanah = 5;
        Debug.Log("Pohon tumbuh dewasa!");

        // Sembunyikan/hapus model bibit
        if (activeBibitInstance != null)
        {
            if (activeBibitInstance == modelBibit)
                modelBibit.SetActive(false);
            else
                Destroy(activeBibitInstance);
        }
        else if (modelBibit != null)
        {
            modelBibit.SetActive(false);
        }

        // Nonaktifkan model default pohon dewasa (jika ada)
        if (modelPohonDewasa != null) modelPohonDewasa.SetActive(false);

        // Cari prefab pohon dewasa
        GameObject adultPrefab = null;
        ItemData statsSource = plantedItemData;
        if (plantedItemData != null)
        {
            if (plantedItemData.grownTreeData != null)
            {
                statsSource = plantedItemData.grownTreeData;
            }
            
            if (statsSource != null)
            {
                adultPrefab = statsSource.itemPrefab;
            }
        }

        // Tentukan posisi & rotasi spawn di tingkat dunia (world space) menggunakan plot tanah langsung
        Vector3 spawnPos = transform.position;
        // Gunakan rotasi asli dari prefab secara langsung
        Quaternion spawnRot = (adultPrefab != null) ? adultPrefab.transform.rotation : Quaternion.identity;

        // Spawn model pohon dewasa di tingkat dunia (tanpa parent agar tidak terpengaruh scale parent)
        if (adultPrefab != null)
        {
            activePohonInstance = Instantiate(adultPrefab, spawnPos, spawnRot);
            
            // Simpan skala asli langsung dari prefab
            targetPohonScale = adultPrefab.transform.localScale;
            
            // Salin skala asli prefab atau 0
            activePohonInstance.transform.localScale = startAtZeroScale ? Vector3.zero : targetPohonScale;

            BekukanFisika(activePohonInstance);
            activePohonInstance.SetActive(true);
        }
        else if (modelPohonDewasa != null)
        {
            // Fallback ke model default plot
            modelPohonDewasa.SetActive(true);
            activePohonInstance = modelPohonDewasa;
            targetPohonScale = originalPohonScale;
            activePohonInstance.transform.localScale = startAtZeroScale ? Vector3.zero : targetPohonScale;
        }

        // Menyambungkan ke GameManager dan WaveManager
        if (GameManager.Instance != null)
        {
            int absorption = (statsSource != null) ? statsSource.waterAbsorption : waterAbsorption;
            int bonus = (statsSource != null) ? statsSource.waveRewardBonus : 0;

            GameManager.Instance.AddPlantedTreeAbsorption(absorption);
            GameManager.Instance.AddPlantedTreeReward(bonus);
        }
    }

    public void ResetPlot()
    {
        statusTanah = 0;
        plantedItemData = null;

        // Hancurkan instance dinamis
        if (activeBibitInstance != null)
        {
            if (activeBibitInstance != modelBibit)
                Destroy(activeBibitInstance);
        }
        if (activePohonInstance != null)
        {
            if (activePohonInstance != modelPohonDewasa)
                Destroy(activePohonInstance);
        }

        activeBibitInstance = null;
        activePohonInstance = null;

        // Reset model visual default
        if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
        if (modelBibit != null) modelBibit.SetActive(false);
        if (modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
        if (modelTunas != null) modelTunas.SetActive(false);
        if (modelPohonDewasa != null)
        {
            modelPohonDewasa.SetActive(false);
            modelPohonDewasa.transform.localScale = originalPohonScale;
        }
    }

    // Pembuat Patung (Mengubah Collider menjadi Trigger & Hapus Rigidbody jika ada)
    void BekukanFisika(GameObject objek)
    {
        // Hancurkan Rigidbody jika tidak sengaja terpasang di prefab agar tidak ada error fisika
        Rigidbody[] rbs = objek.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            Destroy(rb);
        }

        // Jadikan semua Collider sebagai trigger agar tidak menghalangi pergerakan player/VR
        Collider[] cols = objek.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
        {
            col.isTrigger = true;
        }
    }
}