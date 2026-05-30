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
            settingsPanel.SetActive(true);
    }
}
