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
    [Min(0f)] public float offsetVisualPermukaan = 0.02f;

    [Header("Visual Lubang 3D")]
    [SerializeField] private bool gunakanVisualLubang3D = true;
    [SerializeField, Min(0.1f)] private float radiusLubang = 0.32f;
    [SerializeField, Min(0.01f)] private float tinggiBibirLubang = 0.06f;
    [SerializeField, Min(0.01f)] private float kedalamanVisualLubang = 0.08f;
    [SerializeField] private Color warnaDalamLubang = new Color(0.055f, 0.025f, 0.01f, 1f);
    [SerializeField] private Color warnaBibirLubang = new Color(0.28f, 0.13f, 0.045f, 1f);

    [Header("Data Tanaman yang Ditanam")]
    public ItemData plantedItemData; // Diisi otomatis saat bibit dimasukkan

    [Header("Dynamic Model Instances")]
    public GameObject activeBibitInstance;
    public GameObject activePohonInstance;

    private Vector3 originalBibitScale = Vector3.one;
    private Vector3 originalPohonScale = Vector3.one;
    private Vector3 posisiLubangAwal;
    private Vector3 posisiTanahTertutupAwal;
    private float waktuCangkulTerakhir = -1f;
    private GameObject visualLubang3D;
    private static Material materialDalamLubang;
    private static Material materialBibirLubang;

    void Awake()
    {
        if (modelBibit != null)
        {
            originalBibitScale = modelBibit.transform.localScale;
        }
        if (modelPohonDewasa != null)
        {
            originalPohonScale = modelPohonDewasa.transform.localScale;
        }

        if (modelTanahBerlubang != null)
        {
            posisiLubangAwal = modelTanahBerlubang.transform.localPosition;
            modelTanahBerlubang.transform.localPosition = posisiLubangAwal + Vector3.up * offsetVisualPermukaan;
        }

        if (modelTanahTertutup != null)
        {
            posisiTanahTertutupAwal = modelTanahTertutup.transform.localPosition;
            modelTanahTertutup.transform.localPosition = posisiTanahTertutupAwal + Vector3.up * offsetVisualPermukaan;
        }

        TerapkanVisualTanah();
    }

    void OnTriggerEnter(Collider bendaYangNyentuh)
    {
        // Kunci Negara - Sekarang dikunci setelah step 5 (dewasa)
        if (statusTanah == 5) return;

        GameObject objekItem = DapatkanObjekItem(bendaYangNyentuh);

        if (MemilikiTag(bendaYangNyentuh, objekItem, "cangkul"))
        {
            Dicangkul();
            return;
        }

        // STEP 2: MASUKIN BIBIT
        if (statusTanah == 1 && MemilikiTag(bendaYangNyentuh, objekItem, "bibit"))
        {
            statusTanah = 2;
            Debug.Log("Bibit masuk ke lubang!");

            // Ambil data ItemData dari BibitDataCarrier (dipasang otomatis oleh InventoryUI)
            BibitDataCarrier carrier = objekItem.GetComponent<BibitDataCarrier>();
            if (carrier == null)
                carrier = bendaYangNyentuh.GetComponentInParent<BibitDataCarrier>();
            if (carrier != null && carrier.itemData != null)
            {
                plantedItemData = carrier.itemData;
                Debug.Log("Tanaman terdeteksi: " + plantedItemData.itemName);
            }

            // Nonaktifkan model default bibit (jika ada)
            if (modelBibit != null) modelBibit.SetActive(false);

            // Tentukan posisi & rotasi spawn di tingkat dunia (world space) menggunakan plot tanah langsung
            Vector3 spawnPos = transform.position;
            Quaternion spawnRot = transform.rotation;

            // Cari prefab bibit yang akan ditampilkan di tanah (utamakan plantedVisualPrefab)
            GameObject bibitVisualPrefab = null;
            if (plantedItemData != null)
            {
                bibitVisualPrefab = (plantedItemData.plantedVisualPrefab != null) ? plantedItemData.plantedVisualPrefab : plantedItemData.itemPrefab;
            }

            // Spawn model bibit spesifik dari ScriptableObject di tingkat dunia (tanpa parent)
            if (bibitVisualPrefab != null)
            {
                activeBibitInstance = Instantiate(bibitVisualPrefab, spawnPos, spawnRot);
                
                // Salin skala lokal asli agar tidak terpengaruh scale parent
                activeBibitInstance.transform.localScale = originalBibitScale;

                BekukanFisika(activeBibitInstance);
                activeBibitInstance.SetActive(true);
            }
            else if (modelBibit != null)
            {
                // Fallback ke model bibit default plot
                modelBibit.SetActive(true);
                activeBibitInstance = modelBibit;
            }

            TerapkanVisualTanah();
            Destroy(objekItem);
        }

        // STEP 5: DIBERI PUPUK (Tumbuh Dewasa & Masuk ke GameManager)
        else if (statusTanah == 4 && MemilikiTag(bendaYangNyentuh, objekItem, "pupuk"))
        {
            statusTanah = 5;
            Debug.Log("Diberi Pupuk! Pohon tumbuh dewasa dan menyerap air!");
            
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
            Quaternion spawnRot = transform.rotation;

            // Spawn model pohon dewasa di tingkat dunia (tanpa parent agar tidak terpengaruh scale parent)
            if (adultPrefab != null)
            {
                activePohonInstance = Instantiate(adultPrefab, spawnPos, spawnRot);
                
                // Salin skala lokal asli agar tidak terpengaruh scale parent
                activePohonInstance.transform.localScale = originalPohonScale;

                BekukanFisika(activePohonInstance);
                activePohonInstance.SetActive(true);
            }
            else if (modelPohonDewasa != null)
            {
                // Fallback ke model default plot
                modelPohonDewasa.SetActive(true);
                activePohonInstance = modelPohonDewasa;
                activePohonInstance.transform.localScale = originalPohonScale; // Set ke skala asli
            }

            // Menyambungkan ke GameManager dan WaveManager
            if (GameManager.Instance != null)
            {
                int absorption = (statsSource != null) ? statsSource.waterAbsorption : waterAbsorption;
                int bonus = (statsSource != null) ? statsSource.waveRewardBonus : 0;

                GameManager.Instance.AddPlantedTreeAbsorption(absorption);
                GameManager.Instance.AddPlantedTreeReward(bonus);
            }
            
            // Opsional: Hancurkan item pupuk setelah dipakai
            Destroy(objekItem);
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

    public bool Dicangkul()
    {
        if (Time.time - waktuCangkulTerakhir < 0.15f)
            return false;

        waktuCangkulTerakhir = Time.time;

        if (statusTanah == 0)
        {
            statusTanah = 1;
            TerapkanVisualTanah();
            Debug.Log($"{name}: tanah sekarang berlubang.");
            return true;
        }

        if (statusTanah == 2)
        {
            TutupTanahSetelahDitanam();
            return true;
        }

        return false;
    }

    private void TutupTanahSetelahDitanam()
    {
        statusTanah = 3;
        TerapkanVisualTanah();
        Debug.Log("Tanah ditutup rapat. Lubang sudah tertutup dan bibit berhasil ditanam!");
    }

    private void TerapkanVisualTanah()
    {
        bool tanahBerlubang = statusTanah == 1 || statusTanah == 2;
        bool tanahTertutup = statusTanah == 3;

        if (tanahBerlubang && gunakanVisualLubang3D && visualLubang3D == null)
            SiapkanVisualLubang3D();

        if (modelTanahBerlubang != null)
            modelTanahBerlubang.SetActive(tanahBerlubang);

        if (visualLubang3D != null)
            visualLubang3D.SetActive(tanahBerlubang);

        if (modelTanahTertutup != null)
            modelTanahTertutup.SetActive(tanahTertutup);

        if (activeBibitInstance != null)
            activeBibitInstance.SetActive(statusTanah >= 2);
    }

    private void SiapkanVisualLubang3D()
    {
        visualLubang3D = new GameObject("Visual Lubang 3D");
        visualLubang3D.transform.SetParent(transform, false);

        Vector3 posisiPermukaan = modelTanahBerlubang != null
            ? modelTanahBerlubang.transform.localPosition
            : Vector3.zero;
        visualLubang3D.transform.localPosition = posisiPermukaan + Vector3.up * 0.01f;

        GameObject bagianDalam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bagianDalam.name = "Bagian Dalam Lubang";
        bagianDalam.transform.SetParent(visualLubang3D.transform, false);
        bagianDalam.transform.localPosition = Vector3.up * (-kedalamanVisualLubang * 0.5f + 0.004f);
        bagianDalam.transform.localScale = new Vector3(
            radiusLubang * 1.45f,
            kedalamanVisualLubang * 0.5f,
            radiusLubang * 1.45f);

        Collider colliderDalam = bagianDalam.GetComponent<Collider>();
        if (colliderDalam != null)
        {
            colliderDalam.enabled = false;
            Destroy(colliderDalam);
        }

        MeshRenderer rendererDalam = bagianDalam.GetComponent<MeshRenderer>();
        rendererDalam.sharedMaterial = DapatkanMaterialLubang(ref materialDalamLubang, warnaDalamLubang);
        rendererDalam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GameObject bibirLubang = new GameObject("Bibir Lubang");
        bibirLubang.transform.SetParent(visualLubang3D.transform, false);

        MeshFilter meshFilter = bibirLubang.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = BuatMeshBibirLubang();

        MeshRenderer rendererBibir = bibirLubang.AddComponent<MeshRenderer>();
        rendererBibir.sharedMaterial = DapatkanMaterialLubang(ref materialBibirLubang, warnaBibirLubang);
    }

    private Mesh BuatMeshBibirLubang()
    {
        const int jumlahSegmen = 32;
        const int titikPerSegmen = 3;
        Vector3[] vertices = new Vector3[jumlahSegmen * titikPerSegmen];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[jumlahSegmen * 12];

        float radiusLuar = radiusLubang * 1.32f;
        float radiusPuncak = radiusLubang * 1.02f;
        float radiusDalam = radiusLubang * 0.73f;

        for (int i = 0; i < jumlahSegmen; i++)
        {
            float sudut = i * Mathf.PI * 2f / jumlahSegmen;
            Vector3 arah = new Vector3(Mathf.Cos(sudut), 0f, Mathf.Sin(sudut));
            int vertexIndex = i * titikPerSegmen;

            vertices[vertexIndex] = arah * radiusLuar;
            vertices[vertexIndex + 1] = arah * radiusPuncak + Vector3.up * tinggiBibirLubang;
            vertices[vertexIndex + 2] = arah * radiusDalam + Vector3.up * 0.006f;

            uv[vertexIndex] = new Vector2(0f, i / (float)jumlahSegmen);
            uv[vertexIndex + 1] = new Vector2(0.5f, i / (float)jumlahSegmen);
            uv[vertexIndex + 2] = new Vector2(1f, i / (float)jumlahSegmen);
        }

        int triangleIndex = 0;
        for (int i = 0; i < jumlahSegmen; i++)
        {
            int berikutnya = (i + 1) % jumlahSegmen;
            int saatIni = i * titikPerSegmen;
            int sesudah = berikutnya * titikPerSegmen;

            triangles[triangleIndex++] = saatIni;
            triangles[triangleIndex++] = saatIni + 1;
            triangles[triangleIndex++] = sesudah;
            triangles[triangleIndex++] = saatIni + 1;
            triangles[triangleIndex++] = sesudah + 1;
            triangles[triangleIndex++] = sesudah;

            triangles[triangleIndex++] = saatIni + 1;
            triangles[triangleIndex++] = saatIni + 2;
            triangles[triangleIndex++] = sesudah + 1;
            triangles[triangleIndex++] = saatIni + 2;
            triangles[triangleIndex++] = sesudah + 2;
            triangles[triangleIndex++] = sesudah + 1;
        }

        Mesh mesh = new Mesh { name = "Mesh Bibir Lubang" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Material DapatkanMaterialLubang(ref Material material, Color warna)
    {
        if (material != null)
            return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        material = new Material(shader)
        {
            name = "Material Lubang Runtime",
            color = warna,
            enableInstancing = true,
            hideFlags = HideFlags.DontSave
        };

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", warna);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.08f);

        return material;
    }

    private static GameObject DapatkanObjekItem(Collider colliderItem)
    {
        Rigidbody attachedRigidbody = colliderItem.attachedRigidbody;
        return attachedRigidbody != null ? attachedRigidbody.gameObject : colliderItem.transform.root.gameObject;
    }

    private static bool MemilikiTag(Collider colliderItem, GameObject objekItem, string tag)
    {
        return colliderItem.CompareTag(tag) || objekItem.CompareTag(tag);
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
