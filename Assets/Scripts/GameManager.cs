using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Image victimImage;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI messageText;


    [Header("Game Settings")]
    public int startingLives = 3;
    public float minDelay = 1f; //Minimum time before the victim image pops up
    public float maxDelay = 3f; // Maximum time before the next victim image pops up
    public float reactionWindow = 0.8f; // Time window to react

    [Header("Internal Variables")]
    private int score = 0;
    private int lives;
    private bool victimActive = false; // Tracks if the victim can currently be seen 
    private bool gameStarted = false;
    private bool gameOver = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Starting values
        score = 0;
        lives = startingLives;

        //Update all of the text UI elements on the scene
        UpdateUI();

        //Shows an intro message
        messageText.text = "Press Any Key to Begin";
        victimImage.enabled = false;


    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            if (Input.anyKeyDown)
            {
                gameStarted = true;
                messageText.text = "Get ready...";
                StartCoroutine(SpawnVictim());
            }
            return;
        }

        // regular gameplay input below
        if (Input.anyKeyDown)
        {
            if (victimActive)
            {
                BiteSuccess();
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
            Debug.Log("SpawnVictim coroutine running...");

            //wait a random time before showing the next victim
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            //Show the victim
            victimImage.enabled = true;
            victimActive = true;
            messageText.text = "BITE!";

            //Player's reaction time window
            yield return new WaitForSeconds(reactionWindow);

            //If still active after the window, player has missed the bite
            if (victimActive)
            {
                Miss("Too slow!");
            }

        }

        void BiteSuccess()
        {
        if (gameOver) return;
            score++;
            victimActive = false;
            victimImage.enabled = false;
            messageText.text = "Nice Bite!";
            UpdateUI();
            StartCoroutine(SpawnVictim());
        }

        void Miss(string msg)
        {
            lives--;
             if (lives < 0) lives = 0; //clamps lives to 0 to avoid negative lives 
            messageText.text = msg;
            victimActive = false;
            victimImage.enabled = false;
            UpdateUI();

            if (lives <= 0)
            {
                gameOver = true;
                messageText.text = "You let too many victim's get away! Final Score: " + score;
                return;

            }
        StartCoroutine(SpawnVictim());

        }


        void UpdateUI()
        {
            scoreText.text = "Score: " + score;
            livesText.text = "Lives: " + lives;
        }
    
}
