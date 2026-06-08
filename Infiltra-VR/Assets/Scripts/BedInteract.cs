using UnityEngine;
using System.Collections;
using Unity.XR.CoreUtils; // Tambahkan namespace CoreUtils untuk mengakses XROrigin

public class BedInteract : MonoBehaviour
{
    [Header("Referensi UI Panel")]
    [Tooltip("Tarik Sleep_Panel dari Hierarchy ke sini.")]
    public GameObject sleepConfirmationPanel;

    [Header("Sistem VR (XR Origin / Rig)")]
    [Tooltip("Tarik objek XR Origin (Parent utama dari kamera VR) ke sini.")]
    public Transform xrOriginTransform;
    
    [Tooltip("Tarik objek kosong (Empty GameObject) sebagai penanda posisi berdiri di sebelah kasur saat bangun.")]
    public Transform wakeUpAnchor;

    [Tooltip("Tarik objek kosong (Empty GameObject) sebagai penanda posisi tidur di kasur (fallback).")]
    public Transform sleepTargetAnchor;

    [Header("Event Camera (Bird-Eye/Panoramic View)")]
    [Tooltip("Tarik objek kosong (Empty GameObject) yang diposisikan tinggi di langit menghadap ke bawah melihat seluruh lahan.")]
    public Transform overviewAnchor;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isSleeping = false;

    private void Start()
    {
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(false);
        }

        // Auto-assign XR Origin jika belum ditarik
        if (xrOriginTransform == null)
        {
            // Mencari komponen XROrigin di scene
            var origin = FindFirstObjectByType<XROrigin>();
            if (origin != null)
            {
                xrOriginTransform = origin.transform;
            }
        }
    }

    public void OpenConfirmationPanel()
    {
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(true);
            Debug.Log("[BedInteract] Membuka panel konfirmasi tidur.");
        }
    }

    public void OnConfirmSleep()
    {
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(false);
        }

        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        // 1. Teleport XR Origin ke kasur (posisi tidur) jika ada
        if (xrOriginTransform != null)
        {
            isSleeping = true;
            originalPosition = xrOriginTransform.position;
            originalRotation = xrOriginTransform.rotation;

            if (overviewAnchor != null)
            {
                // Teleport pemain ke langit agar bisa melihat simulasi dari atas (Bird-Eye View)
                xrOriginTransform.position = overviewAnchor.position;
                xrOriginTransform.rotation = overviewAnchor.rotation;
                Debug.Log("[BedInteract] Player diteleportasi ke langit (overviewAnchor) untuk melihat simulasi.");
            }
            else if (sleepTargetAnchor != null)
            {
                // Fallback jika tidak menyetel posisi langit, letakkan di kasur
                xrOriginTransform.position = sleepTargetAnchor.position;
                xrOriginTransform.rotation = Quaternion.Euler(0f, sleepTargetAnchor.eulerAngles.y, 0f);
                Debug.Log("[BedInteract] Player diteleportasi ke kasur (sleepTargetAnchor).");
            }
        }

        // Lock Locomotion System saat tidur agar player tidak bisa berjalan
        var uiManager = FindFirstObjectByType<GameUIManager>();
        if (uiManager != null && uiManager.locomotionSystem != null)
        {
            uiManager.locomotionSystem.SetActive(false);
            Debug.Log("[BedInteract] Locomotion System dimatikan saat tidur.");
        }

        // 2. Cari TimelapseManager di scene
        TimelapseManager timelapse = FindFirstObjectByType<TimelapseManager>();
        if (timelapse == null)
        {
            GameObject prefab = Resources.Load<GameObject>("TimelapseSystem");
            if (prefab != null)
            {
                GameObject instantiatedSystem = Instantiate(prefab);
                instantiatedSystem.name = "TimelapseSystem";
                timelapse = instantiatedSystem.GetComponent<TimelapseManager>();
            }
        }

        // 3. Jalankan fase simulasi/timelapse tidur
        if (timelapse != null)
        {
            timelapse.StartTimelapsePhase();

            // Tentukan waktu tunggu dinamis agar sinkron dengan hasil evaluasi (menang/kalah)
            // Default durasi tidur jika timelapse berjalan lancar
            float waitDuration = 5f; 
            
            // Evaluasi serapan air saat ini untuk memprediksi durasi timelapse
            if (GameManager.Instance != null && WaveManager.Instance != null)
            {
                int playerAbsorption = GameManager.Instance.totalWaterAbsorption;
                int currentThreat = WaveManager.Instance.waveWaterThreshold;
                
                if (playerAbsorption >= currentThreat)
                {
                    // Jalur Menang: Tumbuh (2s) + Hujan (15s) + Hujan Berhenti & Basah (2.5s)
                    waitDuration = timelapse.treeGrowthDuration + timelapse.rainDuration + 2.5f;
                }
                else
                {
                    // Jalur Kalah: Tumbuh (2s) + Hujan (1s) + Banjir (10s) + jeda (1.5s)
                    waitDuration = timelapse.treeGrowthDuration + 1.0f + timelapse.floodWaveDuration + 1.5f;
                }
            }

            yield return new WaitForSeconds(waitDuration); 
            
            // 4. Bangun Tidur ke posisi berdiri yang aman
            WakeUp();
        }
        else
        {
            Debug.LogError("[BedInteract] TimelapseManager tidak dapat ditemukan!");
            WakeUp();
        }
    }

    public void WakeUp()
    {
        if (isSleeping && xrOriginTransform != null)
        {
            Debug.Log("[BedInteract] Waktu tidur selesai. Mengembalikan posisi pemain.");

            // Prioritaskan wakeUpAnchor (di samping kasur) jika telah diatur di Unity Editor,
            // jika tidak, kembalikan ke posisi awal sebelum tidur.
            if (wakeUpAnchor != null)
            {
                xrOriginTransform.position = wakeUpAnchor.position;
                xrOriginTransform.rotation = wakeUpAnchor.rotation;
            }
            else
            {
                xrOriginTransform.position = originalPosition;
                xrOriginTransform.rotation = originalRotation;
            }
            
            isSleeping = false;
        }

        // Aktifkan kembali Locomotion System setelah bangun
        var uiManager = FindFirstObjectByType<GameUIManager>();
        if (uiManager != null && uiManager.locomotionSystem != null)
        {
            uiManager.locomotionSystem.SetActive(true);
            Debug.Log("[BedInteract] Locomotion System diaktifkan kembali.");
        }
    }

    public void OnCancelSleep()
    {
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        OpenConfirmationPanel();
    }
}
