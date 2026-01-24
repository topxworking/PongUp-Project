using TMPro;
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
    public GameObject hignScore;
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + highScore.ToString();
        }
    }

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
            hignScore.SetActive(isActive);
        }
    }
}
