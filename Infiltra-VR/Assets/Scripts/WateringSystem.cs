using UnityEngine;

public class WateringSystem : MonoBehaviour
{
    // Pastikan nama class di atas SAMA PERSIS dengan nama file .cs kamu
    public ParticleSystem waterParticles;
    public float tiltThreshold = 40f;
    
    [Header("Pengaturan Radar Siram")]
    public float jarakSiram = 3f; // Seberapa panjang radar nembak ke bawah bumi

    void Start()
    {
        if (waterParticles != null) waterParticles.Stop();
    }

    void Update()
    {
        // --- 1. RUMUS KEMIRINGAN ASLIMU (Jangan diubah, udah pas!) ---
        float currentTilt = transform.eulerAngles.x;

        if (currentTilt > 180) currentTilt -= 360;

        if (currentTilt > tiltThreshold)
        {
            // --- 2. KELUARKAN AIR VISUAL ---
            if (!waterParticles.isEmitting) waterParticles.Play();

            // --- 3. FITUR RADAR PRO (RAYCAST) ---
            // Nembak laser matematika dari teko lurus ke bawah tanah
            RaycastHit infoTabrakan;
            
            if (Physics.Raycast(waterParticles.transform.position, Vector3.down, out infoTabrakan, jarakSiram, Physics.AllLayers, QueryTriggerInteraction.Collide))
            {
                // Ngecek: Apakah benda yang kena radar ini punya script TanahBerkebun?
                TanahBerkebun tanah = infoTabrakan.collider.GetComponent<TanahBerkebun>();
                if (tanah != null)
                {
                    // Boom! Panggil fungsi siram langsung ke tanahnya
                    tanah.DisiramOlehTeko();
                }
            }
        }
        else
        {
            // Matikan air kalau teko ditegakkan
            if (waterParticles.isEmitting) waterParticles.Stop();
        }
    }
}