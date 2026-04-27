
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;



public class VoidBeatMenuManager : MonoBehaviour
{
    [Header("Panels (Canvas Groups)")]
    public CanvasGroup titleGroup;
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;

    [Header("Animations du Titre")]
    public TextMeshProUGUI pressAnyKeyText;
    private bool canProceedToMenu = false;
    private bool isTransitioning = false;

    void Start()
    {
        mainMenuPanel.SetActive(true);
        titleGroup.alpha = 0;
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(false);
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
            StartCoroutine(TransitionToMainMenu());
        }
    }

    IEnumerator TransitionToMainMenu()
    {
        isTransitioning = true;
        canProceedToMenu = false;
        yield return StartCoroutine(Fade(titleGroup, 0, 0.5f));
        titleGroup.gameObject.SetActive(false);
        mainMenuPanel.SetActive(true);
        isTransitioning = false;
    }

    public void NewGame() => SceneManager.LoadScene("Level1_Bunker");

    public void ShowCredits() => creditsPanel.SetActive(true);

    public void HideCredits() => creditsPanel.SetActive(false);

    public void QuitGame() => Application.Quit();

    IEnumerator Fade(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float time = 0;
        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cg.alpha = targetAlpha;
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