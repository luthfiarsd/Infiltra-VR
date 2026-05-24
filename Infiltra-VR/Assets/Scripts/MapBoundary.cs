using UnityEngine;

/// <summary>
/// Membatasi area bermain pemain VR agar tidak keluar dari peta.
/// Cara Pakai:
/// 1. Buat Empty GameObject baru, beri nama "MapBoundary"
/// 2. Pasang script ini ke GameObject tersebut
/// 3. Drag XR Origin ke field "Player Rig"
/// 4. Atur batas Min/Max X dan Z di Inspector sesuai area peta kamu
/// 5. (Opsional) Centang "Show Boundary In Editor" untuk melihat garis batas di Scene View
/// </summary>
public class MapBoundary : MonoBehaviour
{
    [Header("Referensi Player")]
    [Tooltip("Drag 'XR Origin' atau root GameObject pemain VR ke sini")]
    public Transform playerRig;

    [Header("Batas Area (World Position)")]
    [Tooltip("Batas paling kiri (X terkecil)")]
    public float minX = 0f;
    
    [Tooltip("Batas paling kanan (X terbesar)")]
    public float maxX = 100f;
    
    [Tooltip("Batas paling bawah/depan (Z terkecil)")]
    public float minZ = 0f;
    
    [Tooltip("Batas paling atas/belakang (Z terbesar)")]
    public float maxZ = 100f;

    [Header("Pengaturan Tambahan")]
    [Tooltip("Jarak dari batas sebelum peringatan muncul (meter)")]
    public float warningDistance = 5f;

    [Tooltip("Tampilkan garis batas di Scene View Unity (untuk debugging)")]
    public bool showBoundaryInEditor = true;

    [Tooltip("Warna garis batas di editor")]
    public Color boundaryColor = Color.red;

    private void Awake()
    {
        // Cari otomatis Player Rig jika lupa di-assign
        if (playerRig == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerRig = player.transform;
            }
            else
            {
                // Alternatif pencarian via PlayerInventory
                PlayerInventory inv = FindAnyObjectByType<PlayerInventory>();
                if (inv != null) playerRig = inv.transform.root;
            }
        }
    }

    void LateUpdate()
    {
        if (playerRig == null) return;

        // Ambil posisi pemain saat ini
        Vector3 pos = playerRig.position;

        // Clamp (paksa) posisi agar tetap di dalam batas
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        // Terapkan posisi yang sudah dibatasi
        playerRig.position = pos;
    }

    /// <summary>
    /// Cek apakah pemain sedang dekat dengan batas peta.
    /// Bisa digunakan untuk memunculkan efek visual peringatan.
    /// </summary>
    public bool IsNearBoundary()
    {
        if (playerRig == null) return false;

        Vector3 pos = playerRig.position;
        return (pos.x - minX < warningDistance) ||
               (maxX - pos.x < warningDistance) ||
               (pos.z - minZ < warningDistance) ||
               (maxZ - pos.z < warningDistance);
    }

    /// <summary>
    /// Mengembalikan seberapa dekat pemain ke batas (0 = jauh, 1 = di batas).
    /// Berguna untuk efek vignette yang makin gelap saat makin dekat ke batas.
    /// </summary>
    public float GetBoundaryProximity()
    {
        if (playerRig == null) return 0f;

        Vector3 pos = playerRig.position;

        float distToMinX = pos.x - minX;
        float distToMaxX = maxX - pos.x;
        float distToMinZ = pos.z - minZ;
        float distToMaxZ = maxZ - pos.z;

        float closestDist = Mathf.Min(distToMinX, distToMaxX, distToMinZ, distToMaxZ);
        
        if (closestDist >= warningDistance) return 0f;
        
        return 1f - (closestDist / warningDistance);
    }

#if UNITY_EDITOR
    // Menggambar garis batas di Scene View agar mudah dilihat saat mendesain level
    private void OnDrawGizmos()
    {
        if (!showBoundaryInEditor) return;

        Gizmos.color = boundaryColor;

        // Tinggi garis batas (agar terlihat jelas)
        float height = 10f;
        float y = 0f;

        // Gambar 4 garis vertikal di setiap sudut
        Vector3 bottomLeft  = new Vector3(minX, y, minZ);
        Vector3 bottomRight = new Vector3(maxX, y, minZ);
        Vector3 topLeft     = new Vector3(minX, y, maxZ);
        Vector3 topRight    = new Vector3(maxX, y, maxZ);

        Vector3 bottomLeftUp  = new Vector3(minX, y + height, minZ);
        Vector3 bottomRightUp = new Vector3(maxX, y + height, minZ);
        Vector3 topLeftUp     = new Vector3(minX, y + height, maxZ);
        Vector3 topRightUp    = new Vector3(maxX, y + height, maxZ);

        // Garis bawah (lantai)
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);

        // Garis atas
        Gizmos.DrawLine(bottomLeftUp, bottomRightUp);
        Gizmos.DrawLine(bottomRightUp, topRightUp);
        Gizmos.DrawLine(topRightUp, topLeftUp);
        Gizmos.DrawLine(topLeftUp, bottomLeftUp);

        // Garis vertikal di sudut
        Gizmos.DrawLine(bottomLeft, bottomLeftUp);
        Gizmos.DrawLine(bottomRight, bottomRightUp);
        Gizmos.DrawLine(topLeft, topLeftUp);
        Gizmos.DrawLine(topRight, topRightUp);

        // Area warning (garis kuning di dalam)
        Gizmos.color = Color.yellow;
        Vector3 wBL = new Vector3(minX + warningDistance, y, minZ + warningDistance);
        Vector3 wBR = new Vector3(maxX - warningDistance, y, minZ + warningDistance);
        Vector3 wTL = new Vector3(minX + warningDistance, y, maxZ - warningDistance);
        Vector3 wTR = new Vector3(maxX - warningDistance, y, maxZ - warningDistance);

        Gizmos.DrawLine(wBL, wBR);
        Gizmos.DrawLine(wBR, wTR);
        Gizmos.DrawLine(wTR, wTL);
        Gizmos.DrawLine(wTL, wBL);
    }
#endif
}
