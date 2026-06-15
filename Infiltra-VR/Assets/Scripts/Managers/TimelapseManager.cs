using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelapseManager : MonoBehaviour
{
    [Header("Referensi Sistem")]
    [Tooltip("Jika kosong, akan mencoba mencari GameManager dan WaveManager yang aktif di scene.")]
    public GameManager gameManager;
    public WaveManager waveManager;
    public WeatherEffectController weatherEffectController;

    [Header("Referensi Objek Animasi (Bisa di Drag & Drop)")]
    [Tooltip("Daftar pohon dummy yang akan membesar (scale up). Nantinya bisa diganti Animator.")]
    public List<GameObject> dummyTrees;
    
    [Tooltip("Objek/Particle System untuk Hujan.")]
    public GameObject rainEffect;
    
    [Tooltip("Objek dummy untuk tanah basah (Win).")]
    public GameObject wetGroundEffect;
    
    [Tooltip("Tarik objek 'AirNaik' yang memiliki animasi banjir (Lose) ke sini.")]
    public GameObject floodWaveEffect;

    [Tooltip("Tarik gameObjek 'wave' yang memiliki animasi gerakWave (Lose) ke sini.")]
    public GameObject riverWaveEffect;

    [Header("Pengaturan Waktu Animasi (Detik)")]
    public float treeGrowthDuration = 2f;
    public float rainDuration = 3f;
    public float floodWaveDuration = 10f; // Sesuai dengan durasi file animasi gerakairNaik (10 detik)

    private bool isTimelapseRunning = false;

    private void Start()
    {
        // Cari otomatis jika belum di-assign di Inspector
        if (gameManager == null) gameManager = GameManager.Instance;
        if (waveManager == null) waveManager = WaveManager.Instance;
        if (weatherEffectController == null) weatherEffectController = FindFirstObjectByType<WeatherEffectController>();

        // Pastikan semua efek mati di awal
        if (rainEffect != null) rainEffect.SetActive(false);
        if (wetGroundEffect != null) wetGroundEffect.SetActive(false);
        if (floodWaveEffect != null) floodWaveEffect.SetActive(false);
        if (riverWaveEffect != null) riverWaveEffect.SetActive(false);
        if (weatherEffectController != null) weatherEffectController.ResetWeather();
        
        // Atur skala awal pohon menjadi 0 (hanya yang belum dewasa)
        foreach (var tree in dummyTrees)
        {
            if (tree != null)
            {
                TanahBerkebun tanah = tree.GetComponentInParent<TanahBerkebun>();
                if (tanah == null || tanah.statusTanah < 5)
                    tree.transform.localScale = Vector3.zero;
            }
        }

        TanahBerkebun[] allPlotsStart = FindObjectsByType<TanahBerkebun>(FindObjectsSortMode.None);
        foreach (var plot in allPlotsStart)
        {
            if (plot != null && plot.statusTanah < 5)
            {
                if (plot.activePohonInstance != null)
                    plot.activePohonInstance.transform.localScale = Vector3.zero;
                else if (plot.modelPohonDewasa != null)
                    plot.modelPohonDewasa.transform.localScale = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Panggil fungsi ini dari tombol UI Canvas atau Interaksi VR Raycast
    /// untuk memulai proses mengakhiri wave.
    /// </summary>
    public void StartTimelapsePhase()
    {
        if (isTimelapseRunning) return;
        
        if (gameManager == null) gameManager = GameManager.Instance;
        if (waveManager == null) waveManager = WaveManager.Instance;
        if (weatherEffectController == null) weatherEffectController = FindFirstObjectByType<WeatherEffectController>();

        StartCoroutine(TimelapseSequence());
    }

    private IEnumerator TimelapseSequence()
    {
        isTimelapseRunning = true;
        Debug.Log("[TimelapseManager] Memulai Fase Timelapse...");

        // Capture snapshot dari serapan dan reward saat ini (untuk evaluasi wave ini)
        int playerAbsorptionForThisWave = (gameManager != null) ? gameManager.totalWaterAbsorption : 0;
        int playerRewardBonusForThisWave = (gameManager != null) ? gameManager.totalPlantRewardBonus : 0;

        // Cari semua plot tanah yang belum dipupuk/dewasa tapi sudah ditanami bibit (statusTanah >= 2 dan <= 4)
        List<TanahBerkebun> newlyGrownPlots = new List<TanahBerkebun>();
        TanahBerkebun[] allPlots = FindObjectsByType<TanahBerkebun>(FindObjectsSortMode.None);
        foreach (var plot in allPlots)
        {
            if (plot != null && plot.statusTanah >= 2 && plot.statusTanah <= 4)
            {
                newlyGrownPlots.Add(plot);
                plot.TumbuhkanKeDewasa(startAtZeroScale: true);
            }
        }

        // FASE 1: Animasi Pohon Tumbuh (Scale up)
        Debug.Log("[TimelapseManager] Pohon mulai tumbuh...");
        float elapsedTime = 0f;
        while (elapsedTime < treeGrowthDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / treeGrowthDuration;
            
            // Lerp scale dari 0 ke target scale untuk dummyTrees yang baru tumbuh
            foreach (var tree in dummyTrees)
            {
                if (tree != null)
                {
                    TanahBerkebun tanah = tree.GetComponentInParent<TanahBerkebun>();
                    if (tanah != null && newlyGrownPlots.Contains(tanah))
                    {
                        tree.transform.localScale = tanah.targetPohonScale * progress;
                    }
                }
            }

            // Lerp scale untuk model pohon dinamis pada plot yang baru tumbuh
            foreach (var plot in newlyGrownPlots)
            {
                if (plot != null)
                {
                    GameObject targetTree = (plot.activePohonInstance != null) ? plot.activePohonInstance : plot.modelPohonDewasa;
                    if (targetTree != null)
                    {
                        targetTree.transform.localScale = plot.targetPohonScale * progress;
                    }
                }
            }
            yield return null;
        }

        // Pastikan skala penuh untuk newlyGrownPlots pada akhirnya
        foreach (var tree in dummyTrees)
        {
            if (tree != null)
            {
                TanahBerkebun tanah = tree.GetComponentInParent<TanahBerkebun>();
                if (tanah != null && newlyGrownPlots.Contains(tanah))
                {
                    tree.transform.localScale = tanah.targetPohonScale;
                }
            }
        }

        foreach (var plot in newlyGrownPlots)
        {
            if (plot != null)
            {
                GameObject targetTree = (plot.activePohonInstance != null) ? plot.activePohonInstance : plot.modelPohonDewasa;
                if (targetTree != null)
                {
                    targetTree.transform.localScale = plot.targetPohonScale;
                }
            }
        }

        yield return new WaitForSeconds(0.5f); // Jeda sejenak

        // FASE 2 & FASE 3: Evaluasi Hasil & Animasi Sesuai State
        Debug.Log("[TimelapseManager] Mengevaluasi apakah serapan cukup...");
        int playerAbsorption = playerAbsorptionForThisWave;
        int currentThreat = waveManager.waveWaterThreshold;

        if (playerAbsorption >= currentThreat)
        {
            // --- JALUR MENANG ---
            Debug.Log("[TimelapseManager] Hasil: BERHASIL menahan air!");
            
            // Hujan mulai turun
            Debug.Log("[TimelapseManager] Hujan turun...");
            if (weatherEffectController != null) weatherEffectController.StartRain();
            if (rainEffect != null) rainEffect.SetActive(true);
            
            yield return new WaitForSeconds(rainDuration);

            // Hujan berhenti
            if (rainEffect != null) rainEffect.SetActive(false);
            if (weatherEffectController != null) weatherEffectController.StopRain(true);

            // Efek tanah basah muncul
            if (weatherEffectController != null) weatherEffectController.ShowWetGround();
            if (wetGroundEffect != null) wetGroundEffect.SetActive(true);
            
            yield return new WaitForSeconds(2f); // Beri waktu player melihat tanah basah

            // Beri hadiah uang ke PlayerInventory jika berhasil
            if (gameManager.playerInventory != null)
            {
                int reward = gameManager.baseWaveReward + (gameManager.currentWave - 1) * gameManager.rewardPerWaveLevel + playerRewardBonusForThisWave;
                gameManager.playerInventory.uang += reward;
                gameManager.UpdateUI();
                Debug.Log($"Dapat hadiah {reward} koin! Total uang: {gameManager.playerInventory.uang}");
            }
            
            // Lanjut ke wave selanjutnya atau menang penuh
            if (waveManager.currentWave >= waveManager.maxWaves)
            {
                gameManager.ChangeState(GameState.GameWon);
            }
            else
            {
                // Naikkan wave dan panggil layar WaveWon
                gameManager.ChangeState(GameState.WaveWon);
            }
        }
        else
        {
            // --- JALUR KALAH ---
            Debug.Log("[TimelapseManager] Hasil: GAGAL! Banjir terjadi!");
            
            // Hujan mulai turun
            Debug.Log("[TimelapseManager] Hujan turun...");
            if (weatherEffectController != null) weatherEffectController.StartRain();
            if (rainEffect != null) rainEffect.SetActive(true);

            yield return new WaitForSeconds(1.0f); // Hujan selama 1 detik sebelum banjir naik

            // Aktifkan objek banjir (AirNaik / floodWaveEffect) untuk memutar animasinya secara otomatis
            if (floodWaveEffect != null) 
            {
                floodWaveEffect.SetActive(true);
                Debug.Log("[TimelapseManager] Mengaktifkan objek AirNaik (Banjir)...");
            }

            // Aktifkan objek wave (wave / riverWaveEffect) untuk memutar animasinya secara otomatis
            if (riverWaveEffect != null) 
            {
                riverWaveEffect.SetActive(true);
                Debug.Log("[TimelapseManager] Mengaktifkan objek wave (Gerak Wave)...");
            }

            // Tunggu selama durasi animasi banjir
            yield return new WaitForSeconds(floodWaveDuration);

            // Hujan berhenti
            if (rainEffect != null) rainEffect.SetActive(false);
            if (weatherEffectController != null) weatherEffectController.StopRain(true);

            yield return new WaitForSeconds(1.0f); // Jeda sejenak sebelum memunculkan panel kalah

            // Tampilkan Layar GameOver (Panel Kalah)
            gameManager.ChangeState(GameState.GameOver);
        }

        isTimelapseRunning = false;
    }

    public void ResetEnvironment()
    {
        // Hentikan coroutine yang sedang berjalan
        StopAllCoroutines();
        isTimelapseRunning = false;

        // Matikan semua efek visual cuaca dan banjir
        if (rainEffect != null) rainEffect.SetActive(false);
        if (wetGroundEffect != null) wetGroundEffect.SetActive(false);
        if (floodWaveEffect != null) floodWaveEffect.SetActive(false);
        if (riverWaveEffect != null) riverWaveEffect.SetActive(false);

        if (weatherEffectController != null)
        {
            weatherEffectController.StopRain(true);
            weatherEffectController.ResetWeather();
        }
    }
}
