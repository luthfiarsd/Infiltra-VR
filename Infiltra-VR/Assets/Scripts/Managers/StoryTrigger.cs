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

    private bool hasBeenTriggered = false;

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
    }
}
