using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panels (Tarik dari Hierarchy)")]
    public GameObject mainMenuPanel;
    public GameObject gameplayHUDPanel; // Panel seperti Inventory, Serapan Air
    public GameObject pauseMenuPanel;
    public GameObject winPanel;
    public GameObject waveWonPanel; // Panel untuk menang per wave
    public GameObject losePanel;

    [Header("Input System (Tarik Action dari XR Default Input Actions)")]
    public InputActionReference pauseAction;

    private void OnEnable()
    {
        // Berlangganan event saat state berubah
        GameManager.OnGameStateChanged += HandleGameStateChanged;

        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPauseActionTriggered;
        }
    }

    private void OnDisable()
    {
        // Berhenti berlangganan saat object ini mati
        GameManager.OnGameStateChanged -= HandleGameStateChanged;

        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPauseActionTriggered;
            pauseAction.action.Disable();
        }
    }

    private void OnPauseActionTriggered(InputAction.CallbackContext context)
    {
        if (GameManager.Instance == null) return;

        // Toggle pause hanya jika sedang bermain atau sedang pause
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.ChangeState(GameState.Paused);
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }
    }

    private void Start()
    {
        // Saat game mulai, pastikan panel sesuai state awal GameManager
        if (GameManager.Instance != null)
        {
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        // Matikan semua panel dulu
        HideAllPanels();

        // Nyalakan panel sesuai state
        switch (newState)
        {
            case GameState.MainMenu:
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                break;
            case GameState.Playing:
                if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(true);
                break;
            case GameState.Paused:
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
                // HUD bisa tetap nyala atau mati saat pause, saya biarkan nyala di background
                if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(true); 
                break;
            case GameState.GameOver:
                if (losePanel != null) losePanel.SetActive(true);
                break;
            case GameState.GameWon:
                if (winPanel != null) winPanel.SetActive(true);
                break;
            case GameState.WaveWon:
                if (waveWonPanel != null) waveWonPanel.SetActive(true);
                break;
        }
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (waveWonPanel != null) waveWonPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    // --- FUNGSI UNTUK TOMBOL UI ---

    public void Button_StartGame()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void Button_PauseGame()
    {
        GameManager.Instance.ChangeState(GameState.Paused);
    }

    public void Button_ResumeGame()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void Button_BackToMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    public void Button_QuitGame()
    {
        Debug.Log("Quit Game...");
        Application.Quit();
    }

    public void Button_NextWave()
    {
        // Pindah ke wave selanjutnya
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartNextWave();
        }
        // Lanjut main
        GameManager.Instance.ChangeState(GameState.Playing);
    }
}
