using UnityEngine;

public class PapanPintar : MonoBehaviour
{
    // Ukuran 1 kotak dedek (0.5 meter)
    public float ukuranGrid = 0.5f; 
    
    // Biar papannya melayang dikiiiit di atas tanah biar gambarnya gak tenggelam
    public float jarakMelayang = 0.05f; 

    [Header("Baju Stiker Dedek")]
    public Material bajuAbuAbu;
    public Material bajuHijau;
    public Material bajuMerah;

    // Badannya si papan
    private MeshRenderer badanPapan;

    void Start()
    {
        // Pas gamenya mulai, kita raba badannya biar inget
        badanPapan = GetComponent<MeshRenderer>();
        
        // Pakaikan baju abu-abu dulu pas awal
        PakaiBaju(0);
    }

    // Fungsi buat mindahin papan ke tempat yang ditunjuk laser
    public void PindahKeTitik(Vector3 titikTunjuk)
    {
        // 1. Kita bulatkan angkanya biar melompatnya rapi kotak-kotak
        float xRapi = Mathf.Round(titikTunjuk.x / ukuranGrid) * ukuranGrid;
        float zRapi = Mathf.Round(titikTunjuk.z / ukuranGrid) * ukuranGrid;

        // 2. Kita tanya ke tanah, "Berapa tinggi bukit di titik ini?"
        float tinggiTanah = Terrain.activeTerrain.SampleHeight(new Vector3(xRapi, 0, zRapi));
        tinggiTanah = tinggiTanah + Terrain.activeTerrain.transform.position.y;

        // 3. Pindahkan papannya! (Tinggi tanah + melayang dikit)
        transform.position = new Vector3(xRapi, tinggiTanah + jarakMelayang, zRapi);
    }

    // Fungsi buat ganti baju stiker
    // 0 = Abu-abu, 1 = Hijau, 2 = Merah
    public void PakaiBaju(int nomorBaju)
    {
        if (nomorBaju == 0) badanPapan.material = bajuAbuAbu;
        else if (nomorBaju == 1) badanPapan.material = bajuHijau;
        else if (nomorBaju == 2) badanPapan.material = bajuMerah;
    }
}