using UnityEngine;

public class CangkulPintar : MonoBehaviour
{
    [Header("Masukkan Prefab TanahSatuKotak!")]
    public GameObject prefabTanah; 
    public float ukuranGrid = 0.5f;

    // Kakak pasang dua sensor sekaligus biar pasti kena!
    void OnTriggerEnter(Collider bendaKena) { CekTabrakan(bendaKena.gameObject); }
    void OnCollisionEnter(Collision tabrakan) { CekTabrakan(tabrakan.gameObject); }

    void CekTabrakan(GameObject bendaKena)
    {
        // 1. Tulis di Console setiap kali cangkul nyentuh APAPUN
        Debug.Log("Cangkul menyentuh benda: " + bendaKena.name + " (Layer: " + LayerMask.LayerToName(bendaKena.layer) + ")");

        // 2. Cek apakah benda itu adalah Tanah
        if (bendaKena.layer == LayerMask.NameToLayer("Tanah"))
        {
            Debug.Log("HORE! Cangkul berhasil mendeteksi Karpet Gaib!");
            
            Vector3 titikPukul = transform.position; 
            float xRapi = Mathf.Round(titikPukul.x / ukuranGrid) * ukuranGrid;
            float zRapi = Mathf.Round(titikPukul.z / ukuranGrid) * ukuranGrid;

            float tinggiTanah = Terrain.activeTerrain.SampleHeight(new Vector3(xRapi, 0, zRapi));
            tinggiTanah += Terrain.activeTerrain.transform.position.y;
            Vector3 titikTanam = new Vector3(xRapi, tinggiTanah, zRapi);

            Collider[] cekAdaTanah = Physics.OverlapSphere(titikTanam, 0.1f);
            bool tempatKosong = true;

            foreach (Collider col in cekAdaTanah)
            {
                if (col.GetComponent<TanahBerkebun>() != null)
                {
                    tempatKosong = false;
                    break;
                }
            }

            if (tempatKosong)
            {
                Instantiate(prefabTanah, titikTanam, Quaternion.identity);
                Debug.Log("BAM! Sihir Cangkul berhasil, tanah terlahir!");
            }
            else
            {
                Debug.Log("Titik ini udah ada tanahnya, cari tempat lain!");
            }
        }
    }
}