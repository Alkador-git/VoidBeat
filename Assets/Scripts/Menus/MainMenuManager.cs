using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels (Canvas Groups)")]
    public CanvasGroup titleGroup;
    public CanvasGroup mainMenuGroup;
    public CanvasGroup creditsGroup;
    public CanvasGroup quitPopupGroup;
    public CanvasGroup levelSelectGroup;

    [Header("UI Global Map")]
    public TextMeshProUGUI totalFragmentsText;
    public LevelNode[] levelNodes;

    [Header("Animations du Titre")]
    public TextMeshProUGUI pressAnyKeyText;

    [Header("Réglages")]
    public float fadeDuration = 0.5f;
    private bool canProceedToMenu = false;
    private bool isTransitioning = false;

    void Start()
    {
        SetupPanel(titleGroup, false);
        SetupPanel(mainMenuGroup, false);
        SetupPanel(creditsGroup, false);
        SetupPanel(quitPopupGroup, false);
        SetupPanel(levelSelectGroup, false);

        UpdateGlobalProgressUI();
        StartCoroutine(StartGameFlow());
    }

    // --- NAVIGATION ---

    public void OpenLevelSelection() => StartCoroutine(SwitchPanel(mainMenuGroup, levelSelectGroup));
    public void CloseLevelSelection() => StartCoroutine(SwitchPanel(levelSelectGroup, mainMenuGroup));
    public void OpenCredits() => StartCoroutine(SwitchPanel(mainMenuGroup, creditsGroup));
    public void CloseCredits() => StartCoroutine(SwitchPanel(creditsGroup, mainMenuGroup));
    public void OpenQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 1, fadeDuration));
    public void CloseQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 0, fadeDuration));

    public void LoadLevel(string sceneName)
    {
        if (isTransitioning) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // --- LOGIQUE DES TRANSITIONS ---

    IEnumerator SwitchPanel(CanvasGroup from, CanvasGroup to)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        yield return StartCoroutine(Fade(from, 0, fadeDuration));

        yield return StartCoroutine(Fade(to, 1, fadeDuration));

        isTransitioning = false;
    }

    IEnumerator Fade(CanvasGroup cg, float targetAlpha, float duration)
    {
        if (cg == null) yield break;

        if (targetAlpha > 0)
        {
            cg.gameObject.SetActive(true);
            cg.alpha = 0;
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
        cg.interactable = (targetAlpha > 0);
        cg.blocksRaycasts = (targetAlpha > 0);

        if (targetAlpha == 0)
        {
            cg.gameObject.SetActive(false);
        }
    }

    // --- LOGIQUE DE JEU & ANIMATIONS ---

    public void UpdateGlobalProgressUI()
    {
        int totalFragments = PlayerPrefs.GetInt("TotalFragments", 0);
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        if (totalFragmentsText != null)
            totalFragmentsText.text = $"FRAGMENTS : {totalFragments}/50";

        for (int i = 0; i < levelNodes.Length; i++)
        {
            bool isUnlocked = (i + 1) <= levelReached;
            levelNodes[i].SetStatus(isUnlocked);
        }
    }

    IEnumerator StartGameFlow()
    {
        yield return StartCoroutine(Fade(titleGroup, 1, 1f));
        canProceedToMenu = true;
        StartCoroutine(PulsePressAnyKey());
    }

    void Update()
    {
        if (canProceedToMenu && Input.anyKeyDown && !isTransitioning)
        {
            StartCoroutine(TitleToMenuTransition());
        }
    }

    IEnumerator TitleToMenuTransition()
    {
        isTransitioning = true;
        canProceedToMenu = false;
        yield return StartCoroutine(Fade(titleGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    public void ConfirmQuit() => Application.Quit();

    void SetupPanel(CanvasGroup cg, bool startActive)
    {
        if (cg == null) return;
        cg.alpha = startActive ? 1 : 0;
        cg.interactable = startActive;
        cg.blocksRaycasts = startActive;
        cg.gameObject.SetActive(startActive);
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