using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class StoryUIManager : MonoBehaviour
{
    public static StoryUIManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("GameObject Panel yang menampung teks cerita")]
    [SerializeField] private GameObject storyPanel;
    [Tooltip("Komponen TextMeshProUGUI untuk menampilkan teks")]
    [SerializeField] private TextMeshProUGUI storyText;
    [Tooltip("Canvas Group untuk efek Fade In/Out (Otomatis diambil jika kosong)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Story Triggers (Drag & Drop)")]
    [Tooltip("Tarik StoryTrigger 1 (Welcome/Awal) ke sini")]
    public StoryTrigger storyTrigger1;
    [Tooltip("Tarik StoryTrigger 2 (Panduan Menanam) ke sini")]
    public StoryTrigger storyTrigger2;

    [Header("Settings")]
    [Tooltip("Durasi efek Fade In dan Fade Out (dalam detik)")]
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("Waktu default teks ditampilkan di layar (dalam detik)")]
    [SerializeField] private float defaultDisplayTime = 4f;

    private Coroutine currentStoryCoroutine;

    private void Awake()
    {
        // Setup Singleton agar mudah dipanggil dari script manapun
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
            if (canvasGroup == null)
            {
                canvasGroup = storyPanel.GetComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// Menampilkan teks cerita di layar.
    /// </summary>
    /// <param name="text">Teks yang ingin ditampilkan.</param>
    /// <param name="displayTime">Berapa lama teks ditampilkan. Jika kurang dari 0, menggunakan defaultDisplayTime.</param>
    /// <param name="onComplete">Fungsi yang akan dipanggil saat cerita selesai (opsional).</param>
    public void ShowStory(string text, float displayTime = -1f, Action onComplete = null)
    {
        if (displayTime <= 0)
        {
            displayTime = defaultDisplayTime;
        }

        if (currentStoryCoroutine != null)
        {
            StopCoroutine(currentStoryCoroutine);
        }

        currentStoryCoroutine = StartCoroutine(StoryRoutine(text, displayTime, onComplete));
    }

    private IEnumerator StoryRoutine(string text, float displayTime, Action onComplete)
    {
        storyText.text = text;
        storyPanel.SetActive(true);

        // Efek Fade In
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // Tunggu selama waktu display
        yield return new WaitForSeconds(displayTime);

        // Efek Fade Out
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        storyPanel.SetActive(false);
        onComplete?.Invoke();
    }
}
