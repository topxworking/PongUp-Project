using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Management")]
    public string gameSceneName;

    [Header("UI Panels")]
    public GameObject settingsPanel;
    public GameObject playButton;
    public GameObject exitButton;

    public void PlayButton()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void SettingButton()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);

            playButton.SetActive(isActive);
            exitButton.SetActive(isActive);
        }
    }
}
