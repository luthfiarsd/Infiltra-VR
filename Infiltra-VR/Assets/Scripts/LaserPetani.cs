using UnityEngine;

public class LaserPetani : MonoBehaviour
{
    [Header("Setup Objek & Layer")]
    public GameObject papanHologramBaru; 
    public LayerMask cumaTanah; // Layer khusus Karpet Gaib (GridSpot)

    [Header("Material Hologram")]
    public Material materialHijau; // Masukkan material hijau transparan dedek di sini
    public Material materialMerah; // Masukkan material merah transparan dedek di sini

    private Renderer hologramRenderer;

    void Start()
    {
        // Ambil komponen Renderer dari hologram biar kodingan bisa ganti-ganti warnanya
        if (papanHologramBaru != null)
        {
            hologramRenderer = papanHologramBaru.GetComponent<Renderer>();
        }
    }

    void Update()
    {
        Ray sinarLaser = new Ray(transform.position, transform.forward);
        RaycastHit titikYangKena;

        // Laser menembak sejauh 15 meter khusus mendeteksi layer "Tanah"
        if (Physics.Raycast(sinarLaser, out titikYangKena, 15f, cumaTanah, QueryTriggerInteraction.Collide))
        {
            if (papanHologramBaru != null)
            {
                papanHologramBaru.SetActive(true);

                // Taruh hologram di titik tabrakan (dinaikkan 0.08f biar gak gampang tenggelam di terrain)
                papanHologramBaru.transform.position = titikYangKena.point + (Vector3.up * 0.08f);
                papanHologramBaru.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                // === SENSOR DETEKSI OBSTACLE (TANAMAN / BARANG LAIN) ===
                // Bikin bola sensor radius 0.4 meter di titik tersebut.
                // Bola ini mendeteksi SEMUA objek KECUALI layer "Tanah" itu sendiri.
                Collider[] objekTabrakan = Physics.OverlapSphere(papanHologramBaru.transform.position, 0.4f);
                
                bool adaObstacle = false;

                foreach (var col in objekTabrakan)
                {
                    // Jika yang ketabrak bukan Karpet Gaib dirinya sendiri, berarti itu obstacle!
                    if (col.gameObject != titikYangKena.collider.gameObject && col.gameObject != papanHologramBaru)
                    {
                        adaObstacle = true;
                        break;
                    }
                }

                // Ganti warna berdasarkan ada rintangan atau tidak
                if (hologramRenderer != null)
                {
                    if (adaObstacle)
                    {
                        hologramRenderer.material = materialMerah; // BERUBAH MERAH!
                    }
                    else
                    {
                        hologramRenderer.material = materialHijau; // TETAP HIJAU AMAN!
                    }
                }
            }
        }
        else
        {
            // Jika laser keluar dari Karpet Gaib, otomatis disembunyikan
            if (papanHologramBaru != null) papanHologramBaru.SetActive(false);
        }
    }
}