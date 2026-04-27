
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class VoidBeatMenuManager : MonoBehaviour
{
    [Header("Panels (Canvas Groups)")]
    public CanvasGroup titleGroup;
    public CanvasGroup mainMenuGroup;
    public CanvasGroup creditsGroup;
    public CanvasGroup quitPopupGroup;

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
        StartCoroutine(StartGameFlow());
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
        titleGroup.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    public void OpenCredits() => StartCoroutine(SwitchPanel(mainMenuGroup, creditsGroup));

    public void CloseCredits() => StartCoroutine(SwitchPanel(creditsGroup, mainMenuGroup));

    public void OpenQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 1, fadeDuration));

    public void CloseQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 0, fadeDuration));

    public void ConfirmQuit() => Application.Quit();

    public void NewGame() => SceneManager.LoadScene("Whiteroom test level");

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
        if (targetAlpha > 0) cg.gameObject.SetActive(true);
        float startAlpha = cg.alpha;
        float time = 0;
        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = targetAlpha;
        cg.interactable = (targetAlpha > 0);
        cg.blocksRaycasts = (targetAlpha > 0);
        if (targetAlpha == 0) cg.gameObject.SetActive(false);
    }

    void SetupPanel(CanvasGroup cg, bool startActive)
    {
        cg.alpha = startActive ? 1 : 0;
        cg.interactable = startActive;
        cg.blocksRaycasts = startActive;
        cg.gameObject.SetActive(startActive);
    }

    IEnumerator PulsePressAnyKey()
    {
        while (canProceedToMenu)
        {
            float alpha = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
            pressAnyKeyText.alpha = alpha;
            yield return null;
        }
    }
}