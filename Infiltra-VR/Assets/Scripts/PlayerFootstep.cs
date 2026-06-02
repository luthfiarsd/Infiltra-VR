using UnityEngine;

public class PlayerFootstep : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Target transform untuk mengecek pergerakan (Biasanya XR Rig atau Main Camera)")]
    public Transform playerTransform;
    [Tooltip("AudioSource untuk memainkan SFX berjalan")]
    public AudioSource footstepSource;

    [Header("Audio Clips")]
    [Tooltip("Daftar suara langkah kaki (akan dipilih secara acak agar lebih natural)")]
    public AudioClip[] footstepClips;

    [Header("Pengaturan")]
    [Tooltip("Seberapa jauh jarak (dalam meter) pemain harus bergerak sebelum suara langkah diputar")]
    public float distanceBetweenFootsteps = 1.5f;
    [Tooltip("Kecepatan minimum pemain bergerak agar dianggap berjalan")]
    public float minMoveThreshold = 0.1f;
    
    [Range(0f, 1f)] public float footstepVolume = 0.6f;

    private Vector3 lastPosition;
    private float accumulatedDistance = 0f;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform; // Gunakan transform ini jika tidak diset
        }

        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            // Penting untuk VR: Atur spatial blend jika ingin suara terdengar 3D/lokal
            footstepSource.spatialBlend = 0f; 
        }

        lastPosition = playerTransform.position;
        // Kita abaikan ketinggian (Y) agar lompatan/naik turun kepala tidak dianggap langkah secara berlebihan
        lastPosition.y = 0; 
    }

    private void Update()
    {
        // Pastikan game sedang dimainkan (bukan di pause atau main menu)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        Vector3 currentPosition = playerTransform.position;
        currentPosition.y = 0; // Abaikan sumbu Y

        float distanceMoved = Vector3.Distance(lastPosition, currentPosition);

        // Jika pemain bergerak melewati threshold pergerakan minimum
        if (distanceMoved > minMoveThreshold * Time.deltaTime)
        {
            accumulatedDistance += distanceMoved;

            if (accumulatedDistance >= distanceBetweenFootsteps)
            {
                PlayFootstep();
                accumulatedDistance = 0f;
            }
        }

        lastPosition = currentPosition;
    }

    private void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0 || footstepSource == null) return;

        // Pilih audio clip acak dari array
        int randomIndex = Random.Range(0, footstepClips.Length);
        AudioClip clipToPlay = footstepClips[randomIndex];

        if (clipToPlay != null)
        {
            // Acak sedikit pitch agar suara tidak monoton
            footstepSource.pitch = Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(clipToPlay, footstepVolume);
        }
    }
}
