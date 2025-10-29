using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;

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
    public float fadeDuration = 3f; //seconds for fade


    [Header("Sprites")]
    public SpriteRenderer victimSprite;
    public SpriteRenderer monsterSprite;


    [Header("Game Settings")]
    public int startingLives = 5;
    public float minDelay = 1f; //Minimum time before the victim image pops up
    public float maxDelay = 3f; // Maximum time before the next victim image pops up
    public float reactionWindow = 0.8f; // Time window to react

    [Header("Internal Variables")]
    private int score = 0;
    private int lives;
    private bool targetIsVictim = false; // Tracks if the current sprite is the victim
    private bool targetActive = false; // Tracks if the sprites can currently be seen 
    private bool gameStarted = false;
    private bool gameOver = false;


    public void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Starting values
        score = 0;
        lives = startingLives;

        //Hiding the gameover screen until end game
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        //Update all of the text UI elements on the scene
        UpdateUI();

        //Shows an intro message to begin the game!
        messageText.text = "Press Any Key to Begin";

        //Make sure that both my sprites are disabled at the beginning. 
        if (victimSprite != null)
            victimSprite.enabled = false;

        if (monsterSprite != null)
            monsterSprite.enabled = false;
        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //if the game is over stop
        if (gameOver) return;

        //set the initial game start and warn the player to get ready. 
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

        // regular gameplay input below
        if (Input.anyKeyDown)
        {
            if (targetActive)
            {
                if (targetIsVictim)
                {
                    BiteSuccess();
                }
                else
                {
                    Miss("HEY! Why did you bite me? I'm here to HELP you!");
                }
               
            }
            else
            {
                Miss("Too soon!");
            }
        }
    }

    IEnumerator SpawnVictim()
        {

        if (gameOver) yield break;
            


       


            //wait a random time before showing the next victim
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

        // 70% chance to spawn a Victim and 30% chance to spawn a monster
        float roll = Random.value; // 0-1 
        targetIsVictim = roll <= 0.7f; // 70% chance it's a victim

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


            //Player's reaction time window
            yield return new WaitForSeconds(reactionWindow);

            //If still active after the window, player has missed the bite
            if (targetActive)
            {
                Miss(targetIsVictim ?"Too slow!" : "Hesitated!");
            }

        }

        void BiteSuccess()
        {
        if (gameOver) return;
            score++;
            targetActive = false;
            victimSprite.enabled = false;
            monsterSprite.enabled = false;
            messageText.text = "Nice Bite!";
            UpdateUI();
            StartCoroutine(SpawnVictim());
        }

        void Miss(string msg)
        {
            lives--;
             if (lives < 0) lives = 0; //clamps lives to 0 to avoid negative lives 
            messageText.text = msg;

            //stop showing the sprites for the round
            targetActive = false;
            victimSprite.enabled = false;
            monsterSprite.enabled = false;

            UpdateUI();

            //Check for game over
            if (lives <= 0)
            {
                gameOver = true;
                targetActive = false;

                StopAllCoroutines(); //Added so the victims stop spawning after last life is done

                 if (victimSprite != null) victimSprite.enabled = false;
                 if (monsterSprite != null) monsterSprite.enabled = false;
                 if (instructionsText != null)
                instructionsText.gameObject.SetActive(false);

            // Show Game Over screen with fade
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Reset the alpha before fade
                if (gameOverCanvasGroup != null)
                {
                    gameOverCanvasGroup.alpha = 0f;
                    StartCoroutine(FadeInGameOver());
                }
            }

            // Update final score message
            if (gameOverText != null)
                gameOverText.text = $"Game Over!\nFinal Score: {score}";

            if (messageText != null)
                messageText.text = "";

            return; // Fully stop here — no more spawns
        }

        // If not game over, continue spawning
        StartCoroutine(SpawnVictim());

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
        Debug.Log("Restart Button Clicked");
        //Stop all spawns
        StopAllCoroutines();

        //Reset the core of the game
        score = 0;
        lives = startingLives;
        gameStarted = false;
        gameOver = false;
        targetActive = false;
        targetIsVictim = false;

        //Hide all the sprites
        if (victimSprite != null) victimSprite.enabled = false;
        if (monsterSprite != null) monsterSprite.enabled = false;

        if (instructionsText != null)
            instructionsText.gameObject.SetActive(false);

        // Hide the Game Over panel
        if (gameOverPanel != null) gameOverPanel.SetActive(false);


        // Refresh UI and prompt
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
}
