using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public GameObject scoreboardUI;

    public Slider brightnessSlider;
    public Slider volumeSlider;
    public Image brightnessOverlay;

    public TMP_InputField playerNameInputField;

    public ScoreboardManager scoreboardManager;
    public PlayerController playerController;

    private string playerName = "Player"; // Default name
    private bool isPaused = false;
    public int currentScore;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        scoreboardUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Load initial name if any
        if (playerNameInputField != null)
        {
            playerNameInputField.text = playerName;
        }

        brightnessSlider.value = 1f;
        brightnessSlider.onValueChanged.AddListener(SetBrightness);

        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenuUI.activeSelf)
            {
                OpenPauseMenu();
            }
            else if (scoreboardUI.activeSelf)
            {
                CloseScoreboard();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        scoreboardUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        scoreboardUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void OpenPauseMenu()
    {
        optionsMenuUI.SetActive(false);
        scoreboardUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void OpenScoreboard()
    {
        if (scoreboardManager != null)
        {
            scoreboardManager.LoadScores();              
            scoreboardManager.UpdateScoreboardDisplay();  
        }

        pauseMenuUI.SetActive(false);
        scoreboardUI.SetActive(true);
    }


    public void CloseScoreboard()
    {
        scoreboardUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void ChangePlayerName()
    {
        if (playerNameInputField != null && !string.IsNullOrWhiteSpace(playerNameInputField.text))
        {
            playerName = playerNameInputField.text;
            Debug.Log("Player name set to: " + playerName);
            if (playerController != null)
            {
                playerController.playerName = playerName;
            }

        }
    }

    public void QuitGame()
    {
        if (playerController != null)
            currentScore = playerController.GetScore();

        if (playerNameInputField != null && !string.IsNullOrWhiteSpace(playerNameInputField.text))
            playerName = playerNameInputField.text;

        if (scoreboardManager != null)
        {
            scoreboardManager.TryAddScore(currentScore, playerName);
            PlayerPrefs.Save();
        }

        Debug.Log($"Saving score: {currentScore} for {playerName}");

        Time.timeScale = 1f; // Reset in case it's still paused
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Main Menu"); // ✅ Ensure it's in build settings
    }




    private void SetBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            Color color = brightnessOverlay.color;
            color.a = 1f - Mathf.Clamp01(value);
            brightnessOverlay.color = color;
        }
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
    }
}
