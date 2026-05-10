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
    public CanvasGroup creditsGroup;
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
    public float fadeDuration = 0.5f;
    private bool canProceedToMenu = false;
    private bool isTransitioning = false;

    // Initialise le singleton au réveil du script
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Configure l'état initial des panneaux et lance l'animation de départ
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

    // Gère les raccourcis clavier (Alt + 1,2,3,4) pour débloquer les niveaux en test
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

    // Force la sauvegarde de la progression pour le débogage
    private void UnlockLevelDebug(int level)
    {
        PlayerPrefs.SetInt("LevelReached", level);
        UpdateGlobalProgressUI();
    }

    // Réinitialise la progression et charge le premier niveau (Bunker)
    public void NewGame()
    {
        PlayerPrefs.SetInt("LevelReached", 1);
        PlayerPrefs.SetInt("TotalFragments", 0);
        LoadLevel(firstLevelSceneName);
    }

    // Charge la scène correspondant au dernier niveau atteint par le joueur
    public void ContinueGame()
    {
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        if (levelReached <= levelNodes.Length)
        {
            string sceneToLoad = levelNodes[levelReached - 1].sceneToLoad;
            LoadLevel(sceneToLoad);
        }
        else
        {
            LoadLevel(firstLevelSceneName);
        }
    }

    // Gère le chargement technique d'une scène et remet le temps à la normale
    public void LoadLevel(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Fonctions de navigation pour ouvrir/fermer les menus via des transitions
    public void OpenLevelSelection() => StartCoroutine(SwitchPanel(mainMenuGroup, levelSelectGroup));
    public void CloseLevelSelection() => StartCoroutine(SwitchPanel(levelSelectGroup, mainMenuGroup));
    public void OpenCredits() => StartCoroutine(SwitchPanel(mainMenuGroup, creditsGroup));
    public void CloseCredits() => StartCoroutine(SwitchPanel(creditsGroup, mainMenuGroup));
    public void OpenQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 1, fadeDuration));
    public void CloseQuitPopup() => StartCoroutine(Fade(quitPopupGroup, 0, fadeDuration));

    // Enchaîne le fondu sortant d'un panneau et le fondu entrant d'un autre
    IEnumerator SwitchPanel(CanvasGroup from, CanvasGroup to)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        yield return StartCoroutine(Fade(from, 0, fadeDuration));
        yield return StartCoroutine(Fade(to, 1, fadeDuration));
        isTransitioning = false;
    }

    // Modifie progressivement l'opacité (alpha) d'un panneau pour un effet de fondu
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
        if (targetAlpha == 0) cg.gameObject.SetActive(false);
    }

    // Met à jour l'affichage des fragments collectés et l'état des nœuds de niveaux
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

    // Animation d'apparition de l'écran titre au lancement du jeu
    IEnumerator StartGameFlow()
    {
        yield return StartCoroutine(Fade(titleGroup, 1, 1f));
        canProceedToMenu = true;
        StartCoroutine(PulsePressAnyKey());
    }

    // Vérifie les entrées clavier pour le débug ou pour quitter l'écran titre
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

    // Transition visuelle entre l'écran titre et le menu principal
    IEnumerator TitleToMenuTransition()
    {
        isTransitioning = true;
        canProceedToMenu = false;
        yield return StartCoroutine(Fade(titleGroup, 0, fadeDuration));
        titleGroup.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    // Ferme complètement l'application
    public void ConfirmQuit() => Application.Quit();

    // Définit les paramètres d'affichage de base d'un panneau (visibilité et interaction)
    void SetupPanel(CanvasGroup cg, bool startActive)
    {
        if (cg == null) return;
        cg.alpha = startActive ? 1 : 0;
        cg.interactable = startActive;
        cg.blocksRaycasts = startActive;
        cg.gameObject.SetActive(startActive);
    }

    // Fait pulser l'opacité du texte "Appuyer sur une touche" sur l'écran titre
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