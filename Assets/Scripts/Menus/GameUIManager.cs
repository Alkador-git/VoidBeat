using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    // --- PAUSE MENU ---

    [Header("Menus Canvas")]
    public GameObject pauseMenuUI;

    public GameObject victoryMenuUI;

    // --- VICTORY SCREEN DISPLAYS ---

    [Header("Écran de Victoire - Stats")]
    public TextMeshProUGUI totalScoreText;

    public TextMeshProUGUI fragmentsText;

    public TextMeshProUGUI accuracyText;

    private bool isPaused = false;

    // --- INITIALIZATION ---

    /// Initializes singleton and hides menus.
    void Awake()
    {
        Instance = this;
        pauseMenuUI.SetActive(false);
        victoryMenuUI.SetActive(false);
    }

    // --- UPDATE LOOP ---

    /// Handles pause input
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // --- PAUSE LOGIC ---

    /// Pauses game and shows pause menu.
    public void Pause()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;

        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null)
        {
            BeatManager.Instance.musicSource.Pause();
        }
    }

    /// Resumes game and hides pause menu.
    public void Resume()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);

        if (BeatManager.Instance != null && BeatManager.Instance.currentZone != null)
            Time.timeScale = BeatManager.Instance.currentZone.timeScale;
        else
            Time.timeScale = 1f;

        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null)
        {
            BeatManager.Instance.musicSource.UnPause();
        }
    }

    // --- VICTORY SCREEN ---

    /// Displays victory screen with game statistics.
    public void ShowVictoryScreen(int score, int fragments, int totalFragments, float accuracy)
    {
        victoryMenuUI.SetActive(true);
        Time.timeScale = 0f;

        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null)
            BeatManager.Instance.musicSource.Stop();

        totalScoreText.text = "SCORE TOTAL : " + score.ToString("N0");
        fragmentsText.text = $"FRAGMENTS : {fragments} / {totalFragments}";
        accuracyText.text = $"PRÉCISION RYTHMIQUE : {accuracy:F1}%";
    }

    // --- SCENE NAVIGATION ---

    /// Loads the next level in sequence.
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// Returns to main menu.
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}