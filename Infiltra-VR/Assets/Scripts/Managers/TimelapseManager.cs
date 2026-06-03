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
    
    [Tooltip("Objek dummy untuk banjir dari sisi sungai (Lose).")]
    public GameObject floodWaveEffect;

    [Header("Pengaturan Waktu Animasi (Detik)")]
    public float treeGrowthDuration = 2f;
    public float rainDuration = 3f;
    public float floodWaveDuration = 2f;

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

        // FASE 1: Animasi Pohon Tumbuh (Scale up)
        Debug.Log("[TimelapseManager] Pohon mulai tumbuh...");
        float elapsedTime = 0f;
        while (elapsedTime < treeGrowthDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / treeGrowthDuration;
            
            // Lerp scale dari 0 ke 1 untuk dummyTrees
            foreach (var tree in dummyTrees)
            {
                if (tree != null)
                {
                    // Hanya scale up pohon yang sudah dewasa (dipupuk)
                    TanahBerkebun tanah = tree.GetComponentInParent<TanahBerkebun>();
                    if (tanah != null && tanah.statusTanah == 5)
                        tree.transform.localScale = Vector3.one * progress;
                }
            }

            // Lerp scale untuk model pohon dinamis pada setiap plot
            TanahBerkebun[] allPlots = FindObjectsByType<TanahBerkebun>(FindObjectsSortMode.None);
            foreach (var plot in allPlots)
            {
                if (plot != null && plot.statusTanah == 5)
                {
                    GameObject targetTree = (plot.activePohonInstance != null) ? plot.activePohonInstance : plot.modelPohonDewasa;
                    if (targetTree != null)
                    {
                        targetTree.transform.localScale = Vector3.one * progress;
                    }
                }
            }
            yield return null;
        }

        // Pastikan skala penuh pada akhirnya
        foreach (var tree in dummyTrees)
        {
            if (tree != null) tree.transform.localScale = Vector3.one;
        }

        TanahBerkebun[] finalPlots = FindObjectsByType<TanahBerkebun>(FindObjectsSortMode.None);
        foreach (var plot in finalPlots)
        {
            if (plot != null && plot.statusTanah == 5)
            {
                GameObject targetTree = (plot.activePohonInstance != null) ? plot.activePohonInstance : plot.modelPohonDewasa;
                if (targetTree != null)
                {
                    targetTree.transform.localScale = Vector3.one;
                }
            }
        }

        yield return new WaitForSeconds(0.5f); // Jeda sejenak

        // FASE 2: Hujan Turun
        Debug.Log("[TimelapseManager] Hujan turun...");
        if (weatherEffectController != null) weatherEffectController.StartRain();
        if (rainEffect != null) rainEffect.SetActive(true);
        
        yield return new WaitForSeconds(rainDuration);

        // Hujan berhenti atau biarkan menyala? Sementara kita matikan setelah durasi
        if (rainEffect != null) rainEffect.SetActive(false);
        if (weatherEffectController != null) weatherEffectController.StopRain(true);


        // FASE 3: Evaluasi Win/Lose
        Debug.Log("[TimelapseManager] Mengevaluasi apakah serapan cukup...");
        int playerAbsorption = gameManager.totalWaterAbsorption;
        int currentThreat = waveManager.waveWaterThreshold;

        if (playerAbsorption >= currentThreat)
        {
            // MENANG (Tanah hanya basah)
            Debug.Log("[TimelapseManager] Hasil: BERHASIL menahan air!");
            if (weatherEffectController != null) weatherEffectController.ShowWetGround();
            if (wetGroundEffect != null) wetGroundEffect.SetActive(true);
            
            yield return new WaitForSeconds(2f); // Beri waktu player melihat tanah basah

            // Beri hadiah uang ke PlayerInventory jika berhasil
            if (gameManager.playerInventory != null)
            {
                int reward = gameManager.CalculateWaveReward();
                gameManager.playerInventory.uang += reward;
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
                // Kamu juga bisa langsung panggil waveManager.StartNextWave() di sini 
                // jika tidak mau ada layar perantara.
                gameManager.ChangeState(GameState.WaveWon);
            }
        }
        else
        {
            // KALAH (Air banjir meluap)
            Debug.Log("[TimelapseManager] Hasil: GAGAL! Banjir terjadi!");
            if (floodWaveEffect != null) 
            {
                floodWaveEffect.SetActive(true);
                // Animasi sederhana gelombang datang (misal geser dari kiri ke kanan)
                // Kita anggap floodWaveEffect adalah objek air yang bergerak
                Vector3 startPos = floodWaveEffect.transform.position;
                Vector3 targetPos = startPos + new Vector3(10f, 0f, 0f); // Geser 10 unit ke arah X
                
                float floodTime = 0f;
                while (floodTime < floodWaveDuration)
                {
                    floodTime += Time.deltaTime;
                    float progress = floodTime / floodWaveDuration;
                    floodWaveEffect.transform.position = Vector3.Lerp(startPos, targetPos, progress);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(1.5f); // Jeda sebelum GameOver

            // Player bisa ulang dari awal atau wave sama melalui UI GameOver nantinya
            gameManager.ChangeState(GameState.GameOver);
        }

        isTimelapseRunning = false;
    }
}
