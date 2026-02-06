using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Difficulty Settings")]
    public float gravityIncrement = 0.15f;
    public float bounceIncrement = 0.3f;
    public int pointsToIncrease = 10;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject pauseButton;
    public GameObject settingPanel;
    public GameObject settingButton;
    public GameObject resumeButton;
    public GameObject homeButton;
    public TextMeshProUGUI countdownText;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Game References")]
    public GameObject ball;
    public AudioClip booSound;
    private AudioSource _audioSource;
    private Rigidbody2D _ballRb;

    [Header("Scene Management")]
    public string mainMenuSceneName;

    [Header("Item Drop System")]
    public GameObject[] itemPrefabs;
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;
    public float spawnYLimit = 6f;
    public float spawnXLimit = 7f;

    private Coroutine _itemSpawnerCoroutine;
    private int _lastDifficultyMilestone = 0;
    private int _currentScore = 0;
    private bool _isGameActive = false;
    private bool _isPaused = false;
    private GameInputs _inputs;

    private void Awake()
    {
        Instance = this;
        _inputs = new GameInputs();
        _ballRb = ball.GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        Time.timeScale = 0f;
    }

    void OnEnable()
    {
        _inputs.Player.Pause.Enable();
        _inputs.Player.Pause.performed += _ => PauseButton();
    }

    void OnDisable()
    {
        _inputs.Player.Pause.Disable();
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSecondsRealtime(1f);
            count--;
        }
        countdownText.text = "Go!";
        yield return new WaitForSecondsRealtime(0.5f);
        countdownText.gameObject.SetActive(false);
        StartGame();
    }

    void StartGame()
    {
        _isGameActive = true;
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        _ballRb.linearVelocity = new Vector2(Random.Range(-2f, 2f), -6f);

        if (_itemSpawnerCoroutine != null)
        {
            StopCoroutine(_itemSpawnerCoroutine);
        }
        _itemSpawnerCoroutine = StartCoroutine(SpawnItemRoutine());
    }
    public void AddScore(int amount)
    {
        if (!_isGameActive) return;
        _currentScore += amount;
        if (_currentScore < 0) _currentScore = 0;
        scoreText.text = _currentScore.ToString();

        CheckDifficulty();
    }

    void CheckDifficulty()
    {
        int currentMilestone = _currentScore / pointsToIncrease;

        if (currentMilestone > _lastDifficultyMilestone)
        {
            _lastDifficultyMilestone = currentMilestone;
            DifficultyBoost();
        }
    }

    void DifficultyBoost()
    {
        if (ball != null)
        {
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            rb.gravityScale += gravityIncrement;
        }

        PaddleController paddle = FindFirstObjectByType<PaddleController>();
        if (paddle != null)
        {
            paddle.bounceForce += bounceIncrement;
        }
    }

    public void PauseButton()
    {
        if (!_isGameActive || gameOverPanel.activeSelf || countdownText.gameObject.activeSelf) return;

        if (!_isPaused)
        {
            _isPaused = true;
            pausePanel.SetActive(true);
            settingButton.SetActive(true);
            settingPanel.SetActive(false);
            pauseButton.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            ResumeButton();
        }
    }

    public void GameOver()
    {
        _isGameActive = false;
        Time.timeScale = 0f;
        pauseButton.SetActive(false);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (_currentScore > highScore)
        {
            highScore = _currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (_audioSource != null && booSound != null)
        {
            _audioSource.PlayOneShot(booSound);
        }

        if (_itemSpawnerCoroutine != null)
        {
            StopCoroutine(_itemSpawnerCoroutine);
        }

        gameOverPanel.SetActive(true);
        finalScoreText.text = _currentScore.ToString();
        highScoreText.text = highScore.ToString();
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void HomeButton()
    {
        SceneManager.LoadScene(mainMenuSceneName);
        Time.timeScale = 1f;
    }

    public void ResumeButton()
    {
        if (countdownText.gameObject.activeSelf) return;

        resumeButton.SetActive(true);
        homeButton.SetActive(true);

        _isPaused = false;
        pausePanel.SetActive(false);
        settingButton.SetActive(false);
        pauseButton.SetActive(true);

        StartCoroutine(ResumeCountdown());
    }

    IEnumerator ResumeCountdown()
    {
        countdownText.gameObject.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSecondsRealtime(1f);
            count--;
        }

        countdownText.gameObject.SetActive(false);
        pauseButton.SetActive(true);
        Time.timeScale = 1f;
    }

    IEnumerator SpawnItemRoutine()
    {
        while (_isGameActive)
        {
            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(waitTime);

            if (_isGameActive && !_isPaused)
            {
                SpawnRandomItem();
            }
        }
    }

    void SpawnRandomItem()
    {
        float randomX = Random.Range(-spawnXLimit, spawnXLimit);
        Vector2 spawnPos = new Vector2(randomX, spawnYLimit);

        int randomIndex = Random.Range(0, itemPrefabs.Length);
        Instantiate(itemPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }

    public void SettingButton()
    {
        if (settingPanel != null)
        {
            bool isActive = settingPanel.activeSelf;
            settingPanel.SetActive(!isActive);

            resumeButton.SetActive(isActive);
            homeButton.SetActive(isActive);
        }
    }
}
