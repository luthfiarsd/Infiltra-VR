using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

public class TeleportationActivator : MonoBehaviour
{
    public XRRayInteractor teleportInteractor;
    public InputActionProperty teleportActivateAction;
    
    // Tambahkan delay sedikit untuk memberikan waktu Teleportation Provider
    // mengeksekusi teleport sebelum kita mematikan gameObject-nya.
    [Tooltip("Waktu tunda sebelum ray dimatikan. Biarkan 0.1 frame agar teleport sempat terjadi.")]
    public float disableDelay = 0.1f;

    void Start()
    {
        // Pastikan GameObject ray interactor dimatikan di awal
        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);

        // Mendaftarkan event saat tombol ditekan (Mulai teleport)
        if (teleportActivateAction != null && teleportActivateAction.action != null)
        {
            teleportActivateAction.action.Enable();
            teleportActivateAction.action.performed += ActionPerformed;
            // Gunakan event canceled untuk mendeteksi pelepasan tombol
            teleportActivateAction.action.canceled += ActionCanceled;
        }
    }

    private void OnDestroy()
    {
        // Bersihkan event saat script hancur untuk menghindari Memory Leak
        if (teleportActivateAction != null && teleportActivateAction.action != null)
        {
            teleportActivateAction.action.performed -= ActionPerformed;
            teleportActivateAction.action.canceled -= ActionCanceled;
        }
    }

    private void ActionPerformed(InputAction.CallbackContext obj)
    {
        // Munculkan Ray teleport saat tombol ditarik/ditekan
        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(true);
    }

    private void ActionCanceled(InputAction.CallbackContext obj)
    {
        // Mulai timer singkat sebelum mematikan Ray,
        // Ini memberi waktu sistem XR melihat kemana ray menunjuk,
        // lalu memicu proses pindah lokasi.
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(DisableRayDelayed());
        }
    }

    private IEnumerator DisableRayDelayed()
    {
        // Tunggu sepersekian detik atau tunggu hingga akhir frame. 
        // Wajib dipanggil WaitForEndOfFrame() agar XR System bisa memproses event teleportasi terlebih dahulu.
        yield return new WaitForEndOfFrame();
        
        if (disableDelay > 0)
            yield return new WaitForSeconds(disableDelay);

        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);
    }
}
