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
            if (StoryTrigger.Instance != null) StoryTrigger.Instance.TriggerPhase2Step(1);
        }

        // STEP 2: MASUKIN BIBIT
        else if (statusTanah == 1 && bendaYangNyentuh.CompareTag("bibit"))
        {
            statusTanah = 2;
            Debug.Log("Bibit masuk ke lubang!");
            if(modelBibit != null) {
                modelBibit.SetActive(true);
                BekukanFisika(modelBibit);
            }
            Destroy(bendaYangNyentuh.gameObject); 
            if (StoryTrigger.Instance != null) StoryTrigger.Instance.TriggerPhase2Step(2);
        }

        // STEP 3: TUTUP TANAH
        else if (statusTanah == 2 && bendaYangNyentuh.CompareTag("cangkul"))
        {
            statusTanah = 3;
            Debug.Log("Tanah ditutup rapat!");
            if(modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if(modelBibit != null) modelBibit.SetActive(true);
            if(modelTanahTertutup != null) modelTanahTertutup.SetActive(true);
            if (StoryTrigger.Instance != null) StoryTrigger.Instance.TriggerPhase2Step(3);
        }

        // STEP 5: DIBERI PUPUK (Tumbuh Dewasa & Masuk ke GameManager)
        else if (statusTanah == 4 && bendaYangNyentuh.CompareTag("pupuk"))
        {
            statusTanah = 5;
            Debug.Log("Diberi Pupuk! Pohon tumbuh dewasa dan menyerap air!");
            
            if(modelTunas != null) modelTunas.SetActive(false);
            if(modelPohonDewasa != null) 
            {
                modelPohonDewasa.SetActive(true);
                BekukanFisika(modelPohonDewasa);
            }

            // Menyambungkan ke GameManager dan WaveManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPlantedTreeAbsorption(waterAbsorption);
            }
            
            // Opsional: Hancurkan item pupuk setelah dipakai
            Destroy(bendaYangNyentuh.gameObject);

            if (StoryTrigger.Instance != null) StoryTrigger.Instance.TriggerPhase2Step(5);
        }
    }

    // --- STEP 4: FUNGSI BARU YANG DIPANGGIL RADAR TEKO ---
    public void DisiramOlehTeko()
    {
        // Kalau statusnya benar-benar lagi siap disiram (3)
        if (statusTanah == 3)
        {
            statusTanah = 4;
            Debug.Log("Disiram pakai RADAR PRO! Tunas muncul dan TERKUNCI!");
            
            if(modelTanahTertutup != null) modelTanahTertutup.SetActive(false);
            
            if(modelTunas != null) {
                modelTunas.SetActive(true);
                BekukanFisika(modelTunas);
            }

            if (StoryTrigger.Instance != null) StoryTrigger.Instance.TriggerPhase2Step(4);
        }
    }

    // Pembuat Patung
    void BekukanFisika(GameObject objek)
    {
        Rigidbody rb = objek.GetComponent<Rigidbody>();
        if(rb != null) rb.isKinematic = true; 

        Collider col = objek.GetComponent<Collider>();
        if(col != null)
        {
            if (col is MeshCollider meshCol) meshCol.convex = true;
            col.isTrigger = true;
        }
    }
}