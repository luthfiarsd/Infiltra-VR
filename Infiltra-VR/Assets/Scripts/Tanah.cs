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

    [Header("Pengaturan Ukuran & Posisi Kustom")]
    [Tooltip("Multiplier untuk mengatur ukuran bibit & pohon secara manual")]
    public float scaleMultiplier = 1.0f;
    [Tooltip("Offset posisi kustom untuk bibit & pohon dewasa di plot ini")]
    public Vector3 customPositionOffset = Vector3.zero;
    [Tooltip("Rotasi kustom (Euler) untuk bibit & pohon dewasa di plot ini (jika Vector3.zero, menggunakan rotasi prefab)")]
    public Vector3 customRotationOffset = Vector3.zero;

    [Header("Data Tanaman yang Ditanam")]
    public ItemData plantedItemData; // Diisi otomatis saat bibit dimasukkan

    [Header("Dynamic Model Instances")]
    public GameObject activeBibitInstance;
    public GameObject activePohonInstance;

    private Vector3 originalBibitScale = Vector3.one;
    private Vector3 originalPohonScale = Vector3.one;
    [HideInInspector]
    public Vector3 targetPohonScale = Vector3.one;

    [Header("Visual Cue Cangkul/Gembur")]
    [Tooltip("Material untuk tanah gembur (opsional, jika ingin mengganti material)")]
    public Material materialTanahGembur;
    [Tooltip("Warna tanah saat gembur (jika materialTanahGembur tidak diisi)")]
    public Color warnaTanahGembur = new Color(0.6f, 0.45f, 0.3f);

    private Renderer plotRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private bool hasOriginalColor = false;

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

        // Cari renderer utama pada plot tanah ini (atau pada anaknya)
        plotRenderer = GetComponent<Renderer>();
        if (plotRenderer == null)
        {
            plotRenderer = GetComponentInChildren<Renderer>();
        }

        if (plotRenderer != null)
        {
            originalMaterial = plotRenderer.material;
            if (originalMaterial.HasProperty("_Color"))
            {
                originalColor = originalMaterial.color;
                hasOriginalColor = true;
            }
            else if (originalMaterial.HasProperty("_BaseColor"))
            {
                originalColor = originalMaterial.GetColor("_BaseColor");
                hasOriginalColor = true;
            }
        }
    }

    void Start()
    {
        // Pastikan visual awal sesuai dengan statusTanah saat game mulai
        if (statusTanah == 0)
        {
            if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if (modelBibit != null) modelBibit.SetActive(false);
            if (modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            if (modelTunas != null) modelTunas.SetActive(false);
            if (modelPohonDewasa != null) modelPohonDewasa.SetActive(false);
            SetTilledVisual(false);
        }
        else if (statusTanah == 1)
        {
            if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(true);
            if (modelBibit != null) modelBibit.SetActive(false);
            if (modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            if (modelTunas != null) modelTunas.SetActive(false);
            if (modelPohonDewasa != null) modelPohonDewasa.SetActive(false);
            SetTilledVisual(true);
        }
        else if (statusTanah == 2)
        {
            if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(true);
            if (modelBibit != null) modelBibit.SetActive(true);
            if (modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            if (modelTunas != null) modelTunas.SetActive(false);
            if (modelPohonDewasa != null) modelPohonDewasa.SetActive(false);
            SetTilledVisual(true);
        }
        else if (statusTanah == 3)
        {
            if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if (modelBibit != null) modelBibit.SetActive(true);
            if (modelTanahTertutup != null) modelTanahTertutup.SetActive(true);
            if (modelTunas != null) modelTunas.SetActive(false);
            if (modelPohonDewasa != null) modelPohonDewasa.SetActive(false);
            SetTilledVisual(false);
        }
        else if (statusTanah == 4)
        {
            if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if (modelBibit != null) modelBibit.SetActive(true);
            if (modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            if (modelTunas != null) modelTunas.SetActive(false);
            if (modelPohonDewasa != null) modelPohonDewasa.SetActive(false);
            SetTilledVisual(false);
        }
        else if (statusTanah == 5)
        {
            if (modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if (modelBibit != null) modelBibit.SetActive(false);
            if (modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            if (modelTunas != null) modelTunas.SetActive(false);
            if (modelPohonDewasa != null) modelPohonDewasa.SetActive(true);
            SetTilledVisual(false);
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

            SetTilledVisual(true);

            // Progress planting guide to Step 2 (Ambil bibit...)
            if (GameManager.Instance != null && GameManager.Instance.plantingGuideTrigger != null)
            {
                GameManager.Instance.plantingGuideTrigger.ProgressPlantingGuide(1);
            }
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

            // Tentukan posisi & rotasi spawn di tingkat dunia (world space) menggunakan plot tanah langsung + offset
            Vector3 spawnPos = transform.position + customPositionOffset;

            // Cari prefab bibit yang akan ditampilkan di tanah (utamakan plantedVisualPrefab)
            GameObject bibitVisualPrefab = null;
            if (plantedItemData != null)
            {
                bibitVisualPrefab = (plantedItemData.plantedVisualPrefab != null) ? plantedItemData.plantedVisualPrefab : plantedItemData.itemPrefab;
            }

            // Gunakan rotasi kustom jika diisi, jika tidak gunakan rotasi asli prefab
            Quaternion spawnRot = Quaternion.identity;
            if (customRotationOffset != Vector3.zero)
            {
                spawnRot = Quaternion.Euler(customRotationOffset);
            }
            else
            {
                spawnRot = (bibitVisualPrefab != null) ? bibitVisualPrefab.transform.rotation : Quaternion.identity;
            }

            // Spawn model bibit spesifik dari ScriptableObject di tingkat dunia (tanpa parent)
            if (bibitVisualPrefab != null)
            {
                activeBibitInstance = Instantiate(bibitVisualPrefab, spawnPos, spawnRot);
                
                // Gunakan skala asli dari prefab dikali scaleMultiplier
                activeBibitInstance.transform.localScale = bibitVisualPrefab.transform.localScale * scaleMultiplier;

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

            // Progress planting guide to Step 4 (Tutup/padatkan tanah...)
            if (GameManager.Instance != null && GameManager.Instance.plantingGuideTrigger != null)
            {
                GameManager.Instance.plantingGuideTrigger.ProgressPlantingGuide(3);
            }
        }

        // STEP 3: TUTUP TANAH
        else if (statusTanah == 2 && bendaYangNyentuh.CompareTag("cangkul"))
        {
            statusTanah = 3;
            Debug.Log("Tanah ditutup rapat!");
            if(modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if(activeBibitInstance != null) activeBibitInstance.SetActive(true);
            if(modelTanahTertutup != null) modelTanahTertutup.SetActive(true);

            SetTilledVisual(false);

            // Progress planting guide to Step 5 (Siram tanaman...)
            if (GameManager.Instance != null && GameManager.Instance.plantingGuideTrigger != null)
            {
                GameManager.Instance.plantingGuideTrigger.ProgressPlantingGuide(4);
            }
        }

        // STEP 5: DIBERI PUPUK (Tumbuh Dewasa & Masuk ke GameManager)
        else if (statusTanah >= 2 && statusTanah <= 4 && bendaYangNyentuh.CompareTag("pupuk"))
        {
            TumbuhkanKeDewasa(startAtZeroScale: false);
            
            // Hancurkan item pupuk setelah dipakai
            Destroy(bendaYangNyentuh.gameObject);

            // Progress planting guide to Step 7 (Selamat! Tanamanmu...)
            if (GameManager.Instance != null && GameManager.Instance.plantingGuideTrigger != null)
            {
                GameManager.Instance.plantingGuideTrigger.ProgressPlantingGuide(6);
            }
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

            // Progress planting guide to Step 6 (Berikan pupuk...)
            if (GameManager.Instance != null && GameManager.Instance.plantingGuideTrigger != null)
            {
                GameManager.Instance.plantingGuideTrigger.ProgressPlantingGuide(5);
            }
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

        // Tentukan posisi & rotasi spawn di tingkat dunia (world space) menggunakan plot tanah langsung + offset
        Vector3 spawnPos = transform.position + new Vector3(0, 1f, 0) + customPositionOffset;
        
        // Gunakan rotasi kustom jika diisi, jika tidak gunakan rotasi asli prefab
        Quaternion spawnRot = Quaternion.identity;
        if (customRotationOffset != Vector3.zero)
        {
            spawnRot = Quaternion.Euler(customRotationOffset);
        }
        else
        {
            spawnRot = (adultPrefab != null) ? adultPrefab.transform.rotation : Quaternion.identity;
        }

        // Spawn model pohon dewasa di tingkat dunia (tanpa parent agar tidak terpengaruh scale parent)
        if (adultPrefab != null)
        {
            activePohonInstance = Instantiate(adultPrefab, spawnPos, spawnRot);
            
            // Simpan skala asli langsung dari prefab dikali scaleMultiplier
            targetPohonScale = adultPrefab.transform.localScale * scaleMultiplier;
            
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
            targetPohonScale = originalPohonScale * scaleMultiplier;
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

        SetTilledVisual(false);

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

    private void SetMaterialColor(Material mat, Color col)
    {
        if (mat.HasProperty("_Color"))
        {
            mat.color = col;
        }
        else if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", col);
        }
    }

    private void SetTilledVisual(bool isTilled)
    {
        if (plotRenderer == null) return;

        if (isTilled)
        {
            if (materialTanahGembur != null)
            {
                plotRenderer.material = materialTanahGembur;
            }
            else if (hasOriginalColor)
            {
                SetMaterialColor(plotRenderer.material, warnaTanahGembur);
            }
        }
        else
        {
            if (materialTanahGembur != null)
            {
                if (originalMaterial != null)
                {
                    plotRenderer.material = originalMaterial;
                }
            }
            else if (hasOriginalColor)
            {
                SetMaterialColor(plotRenderer.material, originalColor);
            }
        }
    }
}