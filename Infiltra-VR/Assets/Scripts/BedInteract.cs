using UnityEngine;

public class BedInteract : MonoBehaviour
{
    [Header("Referensi UI Panel")]
    [Tooltip("Tarik Sleep_Panel dari Hierarchy ke sini.")]
    public GameObject sleepConfirmationPanel;

    private void Start()
    {
        // Pastikan panel konfirmasi mati di awal permainan
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(false);
        }
    }

    // --- FUNGSI UNTUK MEMBUKA PANEL ---
    // Panggil fungsi ini saat Kasur diklik (dihubungkan lewat event klik di Inspector)
    public void OpenConfirmationPanel()
    {
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(true);
            Debug.Log("[BedInteract] Membuka panel konfirmasi tidur.");
        }
        else
        {
            Debug.LogError("[BedInteract] Gagal membuka panel karena sleepConfirmationPanel belum ditarik di Inspector!");
        }
    }

    // --- FUNGSI UNTUK TOMBOL "TIDUR" (Confirm_Button) ---
    // Hubungkan fungsi ini ke event On Click() tombol Confirm_Button di Inspector
    public void OnConfirmSleep()
    {
        Debug.Log("[BedInteract] Pemain memilih konfirmasi TIDUR. Memulai fase simulasi...");
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(false);
        }

        // 1. Cari TimelapseManager di scene
        TimelapseManager timelapse = FindObjectOfType<TimelapseManager>();
        if (timelapse == null)
        {
            Debug.Log("[BedInteract] TimelapseManager tidak ditemukan di scene. Menginstansiasi TimelapseSystem dari Resources...");
            
            // Muat dari folder Resources
            GameObject prefab = Resources.Load<GameObject>("TimelapseSystem");
            if (prefab != null)
            {
                GameObject instantiatedSystem = Instantiate(prefab);
                instantiatedSystem.name = "TimelapseSystem";
                timelapse = instantiatedSystem.GetComponent<TimelapseManager>();
                Debug.Log("[BedInteract] Sukses memuat dan menginstansiasi TimelapseSystem prefab!");
            }
            else
            {
                Debug.LogError("[BedInteract] Gagal memuat prefab 'TimelapseSystem' dari folder Resources! Pastikan file berada di Assets/Resources/TimelapseSystem.prefab");
                return;
            }
        }

        // 2. Jalankan fase simulasi/timelapse
        if (timelapse != null)
        {
            timelapse.StartTimelapsePhase();
            Debug.Log("[BedInteract] Sukses memanggil StartTimelapsePhase pada TimelapseManager.");
        }
        else
        {
            Debug.LogError("[BedInteract] Komponen TimelapseManager tidak dapat ditemukan pada prefab yang diinstansiasi!");
        }
    }

    // --- FUNGSI UNTUK TOMBOL "BATAL" (Cancel_Button) ---
    // Hubungkan fungsi ini ke event On Click() tombol Cancel_Button di Inspector
    public void OnCancelSleep()
    {
        Debug.Log("[BedInteract] Pemain membatalkan konfirmasi tidur.");
        if (sleepConfirmationPanel != null)
        {
            sleepConfirmationPanel.SetActive(false);
        }
    }

    // --- FALLBACK UNTUK TESTING MOUSE DESKTOP DI EDITOR ---
    private void OnMouseDown()
    {
        // Membuka panel saat di-klik kiri oleh mouse di Unity Editor
        OpenConfirmationPanel();
    }
}
