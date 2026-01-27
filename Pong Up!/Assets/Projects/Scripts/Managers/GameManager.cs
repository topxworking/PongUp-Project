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
    public TextMeshProUGUI countdownText;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Game References")]
    public GameObject ball;
    public AudioClip booSound;
    private AudioSource audioSource;
    private Rigidbody2D ballRb;

    [Header("Scene Management")]
    public string mainMenuSceneName;

    [Header("Item Drop System")]
    public GameObject[] itemPrefabs;
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 15f;
    public float spawnYLimit = 6f;
    public float spawnXLimit = 7f;

    private Coroutine itemSpawnerCoroutine;
    private int lastDifficultyMilestone = 0;
    private int currentScore = 0;
    private bool isGameActive = false;
    private bool isPaused = false;
    private GameInputs inputs;

    private void Awake()
    {
        Instance = this;
        inputs = new GameInputs();
        ballRb = ball.GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        Time.timeScale = 0f;
    }

    void OnEnable()
    {
        inputs.Player.Pause.Enable();
        inputs.Player.Pause.performed += _ => PauseButton();
    }

    void OnDisable()
    {
        inputs.Player.Pause.Disable();
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
        isGameActive = true;
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        ballRb.linearVelocity = new Vector2(Random.Range(-2f, 2f), -6f);

        if (itemSpawnerCoroutine != null)
        {
            StopCoroutine(itemSpawnerCoroutine);
        }
        itemSpawnerCoroutine = StartCoroutine(SpawnItemRoutine());
    }
    public void AddScore(int amount)
    {
        if (!isGameActive) return;
        currentScore += amount;
        scoreText.text = currentScore.ToString();

        CheckDifficulty();
    }

    void CheckDifficulty()
    {
        int currentMilestone = currentScore / pointsToIncrease;

        if (currentMilestone > lastDifficultyMilestone)
        {
            lastDifficultyMilestone = currentMilestone;
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
        if (!isGameActive || gameOverPanel.activeSelf || countdownText.gameObject.activeSelf) return;

        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        pauseButton.SetActive(!isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        pauseButton.SetActive(false);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (audioSource != null && booSound != null)
        {
            audioSource.PlayOneShot(booSound);
        }

        if (itemSpawnerCoroutine != null)
        {
            StopCoroutine(itemSpawnerCoroutine);
        }

        gameOverPanel.SetActive(true);
        finalScoreText.text = currentScore.ToString();
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
        isPaused = false;
        pausePanel.SetActive(false);
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
        while (isGameActive)
        {
            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(waitTime);

            if (isGameActive && !isPaused)
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
        Debug.Log("Spawned Item at: " + spawnPos);
    }
}
