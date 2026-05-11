using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [Header("Panels (Canvas Groups)")]
    public CanvasGroup titleGroup;
    public CanvasGroup mainMenuGroup;
    public CanvasGroup submainMenuGroup;
    public CanvasGroup creditsGroup;
    public CanvasGroup subcreditsGroup;
    public CanvasGroup quitPopupGroup;
    public CanvasGroup levelSelectGroup;

    [Header("UI Global Map")]
    public TextMeshProUGUI totalFragmentsText;
    public LevelNode[] levelNodes;

    [Header("Configuration des Scènes")]
    public string firstLevelSceneName = "Level1_Bunker";

    [Header("Animations du Titre")]
    public TextMeshProUGUI pressAnyKeyText;

    [Header("Réglages")]
    public float fadeDuration = 0.4f;
    private bool canProceedToMenu = false;
    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupPanel(titleGroup, false);
        SetupPanel(mainMenuGroup, false);
        SetupPanel(submainMenuGroup, true);
        SetupPanel(creditsGroup, false);
        SetupPanel(subcreditsGroup, true);
        SetupPanel(quitPopupGroup, false);
        SetupPanel(levelSelectGroup, false);

        UpdateGlobalProgressUI();
        StartCoroutine(StartGameFlow());
    }

    // --- NAVIGATION ---

    public void OpenLevelSelection() => StartCoroutine(SwitchPanel(submainMenuGroup, levelSelectGroup));
    public void CloseLevelSelection() => StartCoroutine(SwitchPanel(levelSelectGroup, submainMenuGroup));

    public void OpenCredits() => StartCoroutine(TransitionToCredits());
    public void CloseCredits() => StartCoroutine(TransitionBackFromCredits());

    // --- LOGIQUE DES COROUTINES DE TRANSITION ---

    IEnumerator TitleToMenuTransition()
    {
        isTransitioning = true;
        canProceedToMenu = false;

        yield return StartCoroutine(Fade(titleGroup, 0, fadeDuration));
        titleGroup.gameObject.SetActive(false);

        yield return StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(submainMenuGroup, 1, fadeDuration));

        isTransitioning = false;
    }

    IEnumerator TransitionToCredits()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        yield return StartCoroutine(Fade(submainMenuGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(mainMenuGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(creditsGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(subcreditsGroup, 1, fadeDuration));

        isTransitioning = false;
    }

    IEnumerator TransitionBackFromCredits()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        yield return StartCoroutine(Fade(subcreditsGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(creditsGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(submainMenuGroup, 1, fadeDuration));

        isTransitioning = false;
    }

    // --- FONCTIONS DE BASE ---

    public IEnumerator SwitchPanel(CanvasGroup from, CanvasGroup to)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        yield return StartCoroutine(Fade(from, 0, fadeDuration));
        yield return StartCoroutine(Fade(to, 1, fadeDuration));
        isTransitioning = false;
    }

    public IEnumerator Fade(CanvasGroup cg, float targetAlpha, float duration)
    {
        if (cg == null) yield break;

        if (targetAlpha > 0)
        {
            cg.gameObject.SetActive(true);
        }
        else
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        float startAlpha = cg.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;

        bool isActive = (targetAlpha > 0.9f);
        cg.interactable = isActive;
        cg.blocksRaycasts = isActive;

        if (targetAlpha <= 0.05f)
        {
            cg.gameObject.SetActive(false);
        }
    }

    // --- LOGIQUE DE JEU ---

    public void NewGame()
    {
        PlayerPrefs.SetInt("LevelReached", 1);
        PlayerPrefs.SetInt("TotalFragments", 0);
        LoadLevel(firstLevelSceneName);
    }

    public void ContinueGame()
    {
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        if (levelReached <= levelNodes.Length)
        {
            string sceneToLoad = levelNodes[levelReached - 1].sceneToLoad;
            LoadLevel(sceneToLoad);
        }
        else LoadLevel(firstLevelSceneName);
    }

    public void LoadLevel(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void OpenQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 1, fadeDuration));
    public void CloseQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 0, fadeDuration));
    public void ConfirmQuit() => Application.Quit();

    void SetupPanel(CanvasGroup cg, bool startActive)
    {
        if (cg == null) return;
        cg.alpha = startActive ? 1 : 0;
        cg.interactable = startActive;
        cg.blocksRaycasts = startActive;
        cg.gameObject.SetActive(startActive);
    }

    IEnumerator StartGameFlow()
    {
        yield return StartCoroutine(Fade(titleGroup, 1, 1f));
        canProceedToMenu = true;
        StartCoroutine(PulsePressAnyKey());
    }

    void Update()
    {
        HandleDebugInputs();
        if (canProceedToMenu && Input.anyKeyDown && !isTransitioning)
        {
            if (!Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
            {
                StartCoroutine(TitleToMenuTransition());
            }
        }
    }

    private void HandleDebugInputs()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) UnlockLevelDebug(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) UnlockLevelDebug(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UnlockLevelDebug(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) UnlockLevelDebug(4);
        }
    }

    private void UnlockLevelDebug(int level)
    {
        PlayerPrefs.SetInt("LevelReached", level);
        UpdateGlobalProgressUI();
    }

    public void UpdateGlobalProgressUI()
    {
        int totalFragments = PlayerPrefs.GetInt("TotalFragments", 0);
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        if (totalFragmentsText != null) totalFragmentsText.text = $"FRAGMENTS : {totalFragments}/50";
        for (int i = 0; i < levelNodes.Length; i++)
        {
            bool isUnlocked = (i + 1) <= levelReached;
            levelNodes[i].SetStatus(isUnlocked);
        }
    }

    IEnumerator PulsePressAnyKey()
    {
        while (canProceedToMenu)
        {
            if (pressAnyKeyText != null)
            {
                float alpha = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
                pressAnyKeyText.alpha = alpha;
            }
            yield return null;
        }
    }
}