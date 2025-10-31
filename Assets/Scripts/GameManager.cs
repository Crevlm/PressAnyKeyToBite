using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI instructionsText;

    [Header("EndGameUI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button quitButton;
    public CanvasGroup gameOverCanvasGroup;
    public float fadeDuration = 3f;

    [Header("Sprites")]
    public SpriteRenderer victimSprite;
    public SpriteRenderer monsterSprite;

    [Header("Game Settings")]
    public int startingLives = 5;
    public float minDelay = 1f;
    public float maxDelay = 3f;
    public float reactionWindow = 0.8f;

    [Header("Internal Variables")]
    private int score = 0;
    private int lives;
    private bool targetIsVictim = false;
    private bool targetActive = false;
    private bool gameStarted = false;
    private bool gameOver = false;
    private bool canAcceptInput = false;
    private bool waitingForNextTarget = false;

    [Header("Audio")]
    public AudioSource biteAudioSource;
    public AudioClip biteSound;
    public AudioSource orcAudioSource;
    public AudioClip orcSound;

    public void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    void Start()
    {
        score = 0;
        lives = startingLives;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateUI();
        messageText.text = "Press Any Key to Begin";

        if (victimSprite != null) victimSprite.enabled = false;
        if (monsterSprite != null) monsterSprite.enabled = false;
        if (instructionsText != null) instructionsText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (gameOver) return;

        // Wait for player to start
        if (!gameStarted)
        {
            if (Input.anyKeyDown)
            {
                gameStarted = true;
                messageText.text = "Get ready...";
                if (instructionsText != null)
                    instructionsText.gameObject.SetActive(true);
                StartCoroutine(SpawnVictim());
            }
            return;
        }

        // Gameplay input
        if (Input.anyKeyDown)
        {
            // Too early (before target appears)
            if (waitingForNextTarget && canAcceptInput && !targetActive)
            {
                Miss("Too soon!");
                return;
            }

            // Valid input window (target visible)
            if (targetActive && canAcceptInput)
            {
                if (targetIsVictim)
                {
                    PlayBiteSound();
                    BiteSuccess();
                }
                else
                {
                    PlayOrcSound();
                    BiteMonster();
                }
            }
        }
    }

    IEnumerator SpawnVictim()
    {
        if (gameOver) yield break;

        // Small cooldown to prevent false inputs right after result
        canAcceptInput = false;
        waitingForNextTarget = false;
        yield return new WaitForSeconds(0.3f);

        // Waiting phase — can trigger “Too soon!”
        waitingForNextTarget = true;
        canAcceptInput = true;

        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

        waitingForNextTarget = false;

        // Pick victim or monster
        float roll = Random.value;
        targetIsVictim = roll <= 0.7f;

        if (targetIsVictim)
        {
            victimSprite.enabled = true;
            monsterSprite.enabled = false;
            messageText.text = "BITE THEM!";
        }
        else
        {
            victimSprite.enabled = false;
            monsterSprite.enabled = true;
            messageText.text = "DON'T BITE!";
        }

        targetActive = true;
        canAcceptInput = true;

        yield return new WaitForSeconds(reactionWindow);

        // Player missed window
        if (targetActive)
        {
            if (targetIsVictim)
            {
                Miss("Too slow!");
            }
            else
            {
                messageText.text = "Nice, the Orc was let through!";
                targetActive = false;
                canAcceptInput = false;
                victimSprite.enabled = false;
                monsterSprite.enabled = false;
                StartCoroutine(SpawnVictim());
            }
        }
    }

    void BiteSuccess()
    {
        if (gameOver) return;
        score++;
        targetActive = false;
        canAcceptInput = false;
        victimSprite.enabled = false;
        monsterSprite.enabled = false;
        messageText.text = "Nice Bite!";
        UpdateUI();
        StartCoroutine(SpawnVictim());
    }

    void BiteMonster()
    {
        messageText.text = "HEY! Why did you bite me? I'm here to HELP you!";
        targetActive = false;
        canAcceptInput = false;
        victimSprite.enabled = false;
        monsterSprite.enabled = false;
        LoseLife();
    }

    void Miss(string msg)
    {
        lives--;
        if (lives < 0) lives = 0;
        messageText.text = msg;
        targetActive = false;
        canAcceptInput = false;
        victimSprite.enabled = false;
        monsterSprite.enabled = false;
        UpdateUI();

        if (lives <= 0)
        {
            EndGame();
            return;
        }

        // Shorter delay between messages and next round
        StartCoroutine(NextRoundAfterDelay(0.4f));
    }

    IEnumerator NextRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SpawnVictim());
    }

    void LoseLife()
    {
        lives--;
        if (lives < 0) lives = 0;
        UpdateUI();

        if (lives <= 0)
        {
            EndGame();
            return;
        }

        StartCoroutine(SpawnVictim());
    }

    void EndGame()
    {
        gameOver = true;
        targetActive = false;
        canAcceptInput = false;
        StopAllCoroutines();

        if (victimSprite != null) victimSprite.enabled = false;
        if (monsterSprite != null) monsterSprite.enabled = false;
        if (instructionsText != null) instructionsText.gameObject.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                StartCoroutine(FadeInGameOver());
            }
        }

        if (gameOverText != null)
            gameOverText.text = $"Game Over!\nFinal Score: {score}";

        if (messageText != null)
            messageText.text = "";
    }

    IEnumerator FadeInGameOver()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, normalized);
            yield return null;
        }
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        livesText.text = "Lives: " + lives;
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        score = 0;
        lives = startingLives;
        gameStarted = false;
        gameOver = false;
        targetActive = false;
        targetIsVictim = false;
        canAcceptInput = false;
        waitingForNextTarget = false;

        if (victimSprite != null) victimSprite.enabled = false;
        if (monsterSprite != null) monsterSprite.enabled = false;
        if (instructionsText != null) instructionsText.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateUI();
        if (messageText != null) messageText.text = "Press Any Key to Begin";
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void PlayBiteSound()
    {
        if (biteAudioSource != null && biteSound != null)
            biteAudioSource.PlayOneShot(biteSound);
    }

    void PlayOrcSound()
    {
        if (orcAudioSource != null && orcSound != null)
            orcAudioSource.PlayOneShot(orcSound);
    }
}
