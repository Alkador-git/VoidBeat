using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Menus Canvas")]
    public GameObject pauseMenuUI;
    public GameObject victoryMenuUI;

    [Header("Écran de Victoire - Stats")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI fragmentsText;
    public TextMeshProUGUI accuracyText;

    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
        pauseMenuUI.SetActive(false);
        victoryMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // --- LOGIQUE PAUSE ---

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

    // --- LOGIQUE VICTOIRE ---

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

    // --- NAVIGATION ---

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}