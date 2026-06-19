using UnityEngine;
using UnityEngine.InputSystem;

public class PauseToggleVR : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Masukkan GameObject Panel Pause kamu di sini")]
    public GameObject pausePanel; 

    [Header("Input VR")]
    [Tooltip("Pilih tombol Pause dari XRI Default Input Actions")]
    public InputActionReference pauseButton;

    private void OnEnable()
    {
        if (pauseButton != null)
        {
            pauseButton.action.Enable();
            pauseButton.action.started += TogglePauseMenu;
        }
    }

    private void OnDisable()
    {
        if (pauseButton != null)
        {
            pauseButton.action.started -= TogglePauseMenu;
        }
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        if (pausePanel != null)
        {
            // Balikkan status nyala/mati panel pause
            bool isActive = pausePanel.activeSelf;
            pausePanel.SetActive(!isActive);

            // Opsional: Pause waktu game jika panel terbuka
            if (!isActive) 
            {
                Time.timeScale = 0f; // Game berhenti
            }
            else 
            {
                Time.timeScale = 1f; // Game jalan lagi
            }
        }
    }
}