using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class VRButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Animasi Skala (Membesar/Mengecil)")]
    [Tooltip("Seberapa besar tombol memuai saat disorot laser (misal: 1.05 = 5% lebih besar)")]
    public float hoverScaleMultiplier = 1.05f;
    [Tooltip("Seberapa kecil tombol menyusut saat ditekan")]
    public float clickScaleMultiplier = 0.95f;
    [Tooltip("Kecepatan transisi animasi membesar/mengecil")]
    public float animationSpeed = 15f;

    [Header("Efek Suara")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;
    private AudioSource audioSource;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetScale = originalScale;

        // Otomatis tambahkan komponen AudioSource jika belum ada
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Agar suaranya jelas terdengar 2D di telinga pemain
        }
    }

    private void OnEnable()
    {
        // Kembalikan ke ukuran normal jika UI ditutup lalu dibuka lagi
        rectTransform.localScale = originalScale;
        targetScale = originalScale;
    }

    private void OnDisable()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        rectTransform.localScale = originalScale;
    }

    // Dipanggil otomatis saat laser VR / Kursor masuk ke area tombol
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScaleMultiplier;
        StartScaleAnimation();
        PlaySound(hoverSound);
    }

    // Dipanggil otomatis saat laser VR / Kursor keluar dari area tombol
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        StartScaleAnimation();
    }

    // Dipanggil otomatis saat pelatuk VR ditekan pada tombol
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * clickScaleMultiplier;
        StartScaleAnimation();
        PlaySound(clickSound);
    }

    // Dipanggil otomatis saat pelatuk VR dilepas pada tombol
    public void OnPointerUp(PointerEventData eventData)
    {
        // Jika dilepas namun laser masih menyorot tombol, kembalikan ke ukuran hover
        targetScale = originalScale * hoverScaleMultiplier;
        StartScaleAnimation();
    }

    private void StartScaleAnimation()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine());
    }

    // Logika animasi agar pergerakannya mulus (Smooth)
    private IEnumerator ScaleRoutine()
    {
        while (Vector3.Distance(rectTransform.localScale, targetScale) > 0.001f)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null;
        }
        rectTransform.localScale = targetScale;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
