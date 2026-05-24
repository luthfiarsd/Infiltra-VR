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

            // --- 3. FITUR RADAR PRO (SPHERECAST) ---
            // Menembakkan radar tebal (radius 0.5) ke arah depan semprotan air, bukan lurus ke bawah
            RaycastHit infoTabrakan;
            
            if (Physics.SphereCast(waterParticles.transform.position, 0.5f, waterParticles.transform.forward, out infoTabrakan, jarakSiram, Physics.AllLayers, QueryTriggerInteraction.Collide))
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