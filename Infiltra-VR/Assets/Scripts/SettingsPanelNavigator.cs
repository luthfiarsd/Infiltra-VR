using UnityEngine;

public class SettingsPanelNavigator : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsPanel;

    GameObject previousPanel;

    public void OpenFromMainMenu()
    {
        OpenSettings(mainMenuPanel);
    }

    public void OpenFromPause()
    {
        OpenSettings(pausePanel);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (previousPanel != null)
            previousPanel.SetActive(true);
    }

    void OpenSettings(GameObject sourcePanel)
    {
        previousPanel = sourcePanel;

        if (settingsPanel != null && sourcePanel != null && settingsPanel.transform.IsChildOf(sourcePanel.transform))
        {
            Debug.LogWarning("SettingsPanel berada di dalam panel asal. Pindahkan SettingsPanel agar sejajar dengan MainMenuPanel dan PausePanel, bukan menjadi child salah satunya.", this);
            settingsPanel.SetActive(true);
            return;
        }

        if (sourcePanel != null)
            sourcePanel.SetActive(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            PositionPanelInFrontOfPlayer();
        }
    }

    private void PositionPanelInFrontOfPlayer()
    {
        if (Camera.main == null || settingsPanel == null) return;

        Transform camTransform = Camera.main.transform;
        
        // 1. Taruh posisi panel sekitar 1.2 meter tepat di depan arah pandang Kamera VR
        Vector3 targetPosition = camTransform.position + (camTransform.forward * 1.2f);
        
        // 2. Setel tingginya sedikit di bawah mata (sejajar dada) agar tidak terlalu mendongak ke atas
        targetPosition.y = camTransform.position.y - 0.1f; 
        settingsPanel.transform.position = targetPosition;

        // 3. Hadapkan panel lurus ke wajah pemain (Efek Billboard)
        Vector3 lookAtTarget = camTransform.position;
        lookAtTarget.y = settingsPanel.transform.position.y; // Kunci sumbu Y agar panel tidak mendongak/nunduk kaku
        
        settingsPanel.transform.LookAt(lookAtTarget);
        settingsPanel.transform.Rotate(0, 180, 0); // Balik 180 derajat agar teks Canvas UI-mu tidak tercermin terbalik
    }
}
