using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// Sesuaikan namespace jika menggunakan XRI versi 3.x ke atas
// using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class VRLocomotionManager : MonoBehaviour
{
    public enum LocomotionMode
    {
        Teleportation,
        SmoothLocomotion
    }

    [Header("Current Mode")]
    public LocomotionMode currentMode = LocomotionMode.SmoothLocomotion;

    [Header("Smooth Locomotion Setup")]
    [Tooltip("Masukkan komponen Continuous Move Provider (Action-based) dari XR Origin")]
    public MonoBehaviour continuousMoveProvider;
    
    [Header("Teleportation Setup")]
    [Tooltip("Masukkan GameObject yang memiliki XR Ray Interactor khusus untuk Teleport (misal: Ray Interactor di tangan kanan)")]
    public GameObject teleportRayInteractor;
    [Tooltip("Masukkan komponen Teleportation Provider dari XR Origin")]
    public MonoBehaviour teleportationProvider;

    private void Start()
    {
        // Terapkan mode awal saat game dimulai
        ApplyLocomotionMode(currentMode);
    }

    /// <summary>
    /// Panggil fungsi ini (misalnya lewat UI Button atau Input Action) untuk mengganti mode.
    /// </summary>
    public void SetLocomotionMode(int modeIndex)
    {
        ApplyLocomotionMode((LocomotionMode)modeIndex);
    }

    public void ApplyLocomotionMode(LocomotionMode mode)
    {
        currentMode = mode;

        switch (currentMode)
        {
            case LocomotionMode.Teleportation:
                // Matikan Smooth Locomotion
                if (continuousMoveProvider != null) continuousMoveProvider.enabled = false;
                
                // Nyalakan Teleportation
                if (teleportRayInteractor != null) teleportRayInteractor.SetActive(true);
                if (teleportationProvider != null) teleportationProvider.enabled = true;
                
                Debug.Log("Mode Gerak: Teleportation Aktif");
                break;

            case LocomotionMode.SmoothLocomotion:
                // Nyalakan Smooth Locomotion
                if (continuousMoveProvider != null) continuousMoveProvider.enabled = true;
                
                // Matikan Teleportation
                if (teleportRayInteractor != null) teleportRayInteractor.SetActive(false);
                if (teleportationProvider != null) teleportationProvider.enabled = false;
                
                Debug.Log("Mode Gerak: Smooth Locomotion Aktif");
                break;
        }
    }
}
