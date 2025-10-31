using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{

    [Header("UI Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;

    public void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(() =>
            {
                PlayButtonSound();
                StartGame();
            });

        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() =>
            {
                PlayButtonSound();
                QuitGame();
            });
        }
    }


    void PlayButtonSound()
    {
        Debug.Log("Button Click Detected");
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
        else
            Debug.LogWarning("AudioSource or ButtonClickSound is missing!");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    public void StartGame()
    {
        PlayButtonSound();
        StartCoroutine(LoadSceneWithDelay());
        //SceneManager.LoadScene("MainScene");
    }

    IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(0.15f);
        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        PlayButtonSound();
        StartCoroutine(QuitWithDelay());
    }

    IEnumerator QuitWithDelay()
    {
        yield return new WaitForSeconds(0.15f);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


}
