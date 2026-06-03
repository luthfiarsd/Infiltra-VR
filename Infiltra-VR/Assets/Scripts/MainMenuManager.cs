using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Main VR Scene");
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

/*using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panel UI (Tarik objek ke sini)")]
    public GameObject Tutorial_Panel;
    public GameObject MainMenu_Panel;
    public GameObject Profile_Panel;

    [Header("Sistem Locomotion")]
    public GameObject locomotionSystem; 

    public void StartGame()
    {
        // DEBUG: Cek apakah referensi objek terhubung dengan benar
        if (Tutorial_Panel == null) Debug.LogError("MainMenuManager: Tutorial_Panel BELUM DIISI di Inspector!");
        if (MainMenu_Panel == null) Debug.LogError("MainMenuManager: MainMenu_Panel BELUM DIISI di Inspector!");
        if (Profile_Panel == null) Debug.LogError("MainMenuManager: Profile_Panel BELUM DIISI di Inspector!");

        // 1. Matikan panel satu per satu dengan pengecekan aman
        if (Tutorial_Panel != null) Tutorial_Panel.SetActive(false);
        if (MainMenu_Panel != null) MainMenu_Panel.SetActive(false);
        if (Profile_Panel != null) Profile_Panel.SetActive(false);

        // 2. Aktifkan sistem pergerakan
        if (locomotionSystem != null)
        {
            locomotionSystem.SetActive(true);
        }
        else
        {
            Debug.LogWarning("MainMenuManager: Locomotion System tidak terhubung!");
        }

        Debug.Log("StartGame: Semua panel dimatikan dan pergerakan aktif.");
    }
}*/