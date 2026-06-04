using UnityEngine;
using UnityEngine.Events;

public class StoryTrigger : MonoBehaviour
{
    [Header("Story Content")]
    [TextArea(3, 5)]
    [Tooltip("Teks yang akan muncul di layar")]
    [SerializeField] private string storyText = "Teks cerita Anda di sini...";
    
    [Tooltip("Berapa lama teks ditampilkan. (Biarkan 0 untuk memakai default dari Manager)")]
    [SerializeField] private float displayDuration = 4f;

    [Header("Trigger Settings")]
    [Tooltip("Centang jika teks harus muncul otomatis saat game pertama kali play/objek aktif")]
    [SerializeField] private bool triggerOnStart = false;
    
    [Tooltip("Centang jika teks muncul saat player menyentuh area (OnTriggerEnter)")]
    [SerializeField] private bool triggerOnEnter = true;
    
    [Tooltip("Centang jika teks hanya boleh muncul sekali saja (tidak berulang setiap kali player lewat)")]
    [SerializeField] private bool showOnlyOnce = true;
    
    [Tooltip("Tag dari objek pemain (Player atau XRRig/XR Origin)")]
    [SerializeField] private string playerTag = "Player";

    [Header("Events")]
    [Tooltip("Event yang akan dijalankan setelah teks ini selesai ditampilkan")]
    public UnityEvent OnStoryFinished;

    [Header("Phase 2 Settings (Optional)")]
    [Tooltip("Kumpulan teks petunjuk untuk Fase 2 (misalnya 5 langkah menanam)")]
    [TextArea(3, 5)]
    [SerializeField] private string[] storyTextsPhase2;
    
    [Tooltip("Berapa lama masing-masing teks Fase 2 ditampilkan.")]
    [SerializeField] private float displayDurationPhase2 = 4f;

    [Tooltip("Event yang akan dijalankan setelah semua langkah cerita selesai ditampilkan")]
    public UnityEvent OnPhase2Finished;

    public static StoryTrigger Instance { get; private set; }

    private bool hasBeenTriggered = false;
    private bool hasTriggeredPhase1 = false;
    private bool hasTriggeredPhase2 = false;

    private int currentPhase2Index = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (triggerOnStart)
        {
            TriggerStory();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && other.CompareTag(playerTag))
        {
            TriggerStory();
        }
    }

    /// <summary>
    /// Memanggil StoryUIManager untuk menampilkan teks.
    /// </summary>
    public void TriggerStory()
    {
        // Jika tidak ada teks Fase 2, gunakan perilaku original StoryTrigger
        if (storyTextsPhase2 == null || storyTextsPhase2.Length == 0)
        {
            if (showOnlyOnce && hasBeenTriggered) return;

            if (StoryUIManager.Instance != null)
            {
                StoryUIManager.Instance.ShowStory(storyText, displayDuration, () => 
                {
                    OnStoryFinished?.Invoke();
                });
                hasBeenTriggered = true;
            }
            else
            {
                Debug.LogWarning("StoryUIManager Instance tidak ditemukan di scene! Pastikan ada objek StoryUIManager di dalam hierarchy.");
            }
            return;
        }

        // Perilaku Multi-stage (Fase 1 & Fase 2 dengan 5-step menanam)
        if (!hasTriggeredPhase1)
        {
            if (StoryUIManager.Instance != null)
            {
                StoryUIManager.Instance.ShowStory(storyText, displayDuration, () => 
                {
                    OnStoryFinished?.Invoke();
                });
                hasTriggeredPhase1 = true;
                hasBeenTriggered = true;
            }
            else
            {
                Debug.LogWarning("StoryUIManager Instance tidak ditemukan di scene! Pastikan ada objek StoryUIManager di dalam hierarchy.");
            }
        }
        else if (!hasTriggeredPhase2)
        {
            // Cek apakah pemain sudah membuka peti
            if (ChestInteract.HasOpenedChest)
            {
                // Tampilkan langkah Fase 2 saat ini (dimulai dari 0)
                TriggerPhase2Step(currentPhase2Index);
            }
            else
            {
                // Pengingat untuk masuk ke rumah & mengambil bibit/alat
                if (StoryUIManager.Instance != null)
                {
                    StoryUIManager.Instance.ShowStory(storyText, displayDuration, () => 
                    {
                        OnStoryFinished?.Invoke();
                    });
                }
            }
        }
        else
        {
            // Jika Fase 2 sudah pernah selesai ditampilkan dan showOnlyOnce adalah true, jangan lakukan apa-apa
            if (showOnlyOnce) return;

            // Jika showOnlyOnce adalah false, izinkan memutar ulang Fase 2 dari awal
            TriggerPhase2Step(0);
        }
    }

    /// <summary>
    /// Menampilkan langkah Fase 2 tertentu berdasarkan index.
    /// </summary>
    public void TriggerPhase2Step(int index)
    {
        if (storyTextsPhase2 == null || index < 0 || index >= storyTextsPhase2.Length)
        {
            return;
        }

        currentPhase2Index = index;

        if (StoryUIManager.Instance != null)
        {
            StoryUIManager.Instance.ShowStory(storyTextsPhase2[index], displayDurationPhase2, () =>
            {
                // Jika ini adalah langkah terakhir yang ditampilkan, selesaikan Fase 2
                if (index == storyTextsPhase2.Length - 1)
                {
                    hasTriggeredPhase2 = true;
                    OnPhase2Finished?.Invoke();
                }
            });
        }
        else
        {
            Debug.LogWarning("StoryUIManager Instance tidak ditemukan saat menampilkan langkah Fase 2.");
        }
    }

    /// <summary>
    /// Menampilkan langkah Fase 2 berikutnya secara berurutan.
    /// </summary>
    public void TriggerNextPhase2Step()
    {
        if (storyTextsPhase2 == null) return;

        int nextIndex = currentPhase2Index + 1;
        if (nextIndex < storyTextsPhase2.Length)
        {
            TriggerPhase2Step(nextIndex);
        }
        else
        {
            hasTriggeredPhase2 = true;
            OnPhase2Finished?.Invoke();
        }
    }
}
