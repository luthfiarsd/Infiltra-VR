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

    void OnTriggerEnter(Collider bendaYangNyentuh)
    {
        // Kunci Negara
        if (statusTanah == 4) return;

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
            if(modelBibit != null) {
                modelBibit.SetActive(true);
                BekukanFisika(modelBibit);
            }
            Destroy(bendaYangNyentuh.gameObject); 
        }

        // STEP 3: TUTUP TANAH
        else if (statusTanah == 2 && bendaYangNyentuh.CompareTag("cangkul"))
        {
            statusTanah = 3;
            Debug.Log("Tanah ditutup rapat!");
            if(modelTanahBerlubang != null) modelTanahBerlubang.SetActive(false);
            if(modelBibit != null) modelBibit.SetActive(true);
            if(modelTanahTertutup != null) modelTanahTertutup.SetActive(true);
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