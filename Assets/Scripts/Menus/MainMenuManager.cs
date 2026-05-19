using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [Header("Panels (Canvas Groups)")]
    public CanvasGroup titleGroup;
    public CanvasGroup mainMenuGroup;
    public CanvasGroup submainMenuGroup;
    public CanvasGroup optionsGroup;
    public CanvasGroup currentOptionsTab;
    public CanvasGroup creditsGroup;
    public CanvasGroup subcreditsGroup;
    public CanvasGroup quitPopupGroup;
    public CanvasGroup levelSelectGroup;

    [Header("Sub-Options Tabs")]
    public CanvasGroup videoTab;
    public CanvasGroup audioTab;
    public CanvasGroup calibrationTab;
    public CanvasGroup controlsTab;

    private List<CanvasGroup> allOptionsTabs;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Video Settings")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    [Header("Calibration Settings")]
    public TextMeshProUGUI latencyText;
    private List<float> latencyOffsets = new List<float>();

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
        Time.timeScale = 1;
        allOptionsTabs = new List<CanvasGroup> { videoTab, audioTab, calibrationTab, controlsTab };

        SetupPanel(titleGroup, false);
        SetupPanel(mainMenuGroup, false);
        SetupPanel(submainMenuGroup, false);
        SetupPanel(optionsGroup, false);
        SetupPanel(creditsGroup, false);
        SetupPanel(subcreditsGroup, false);
        SetupPanel(quitPopupGroup, false);
        SetupPanel(levelSelectGroup, false);

        foreach (var tab in allOptionsTabs) SetupPanel(tab, tab == videoTab);
        currentOptionsTab = videoTab;

        InitAudioSliders();

        InitResolutionDropdown();

        UpdateGlobalProgressUI();
        StartCoroutine(StartGameFlow());
    }

    private void InitAudioSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0.0001f;
            musicSlider.maxValue = 1f;
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    // --- LOGIQUE DES ONGLETS ---

    public void SwitchOptionsTab(CanvasGroup newTab)
    {
        if (newTab == currentOptionsTab) return;

        foreach (var tab in allOptionsTabs)
        {
            if (tab == newTab)
            {
                StartCoroutine(Fade(tab, 1, fadeDuration));
            }
            else if (tab.alpha > 0 || tab.gameObject.activeSelf)
            {
                StartCoroutine(Fade(tab, 0, fadeDuration));
            }
        }

        currentOptionsTab = newTab;
    }

    // --- NAVIGATION ---

    public void OpenOptions() => StartCoroutine(TransitionToOptions());
    public void CloseOptions() => StartCoroutine(TransitionBackFromOptions());
    public void OpenLevelSelection() => StartCoroutine(TransitionToLevelSelection());
    public void CloseLevelSelection() => StartCoroutine(TransitionBackFromLevelSelection());
    public void OpenCredits() => StartCoroutine(TransitionToCredits());
    public void CloseCredits() => StartCoroutine(TransitionBackFromCredits());

    // --- LOGIQUE VIDÉO & RÉSOLUTIONS ---

    void InitResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        resolutions = Screen.resolutions.Select(res => new Resolution { width = res.width, height = res.height }).Distinct().ToArray();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index) => Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreen);
    public void SetFullScreen(bool isFull) => Screen.fullScreen = isFull;
    public void SetVSync(int index) => QualitySettings.vSyncCount = index;

    // --- LOGIQUE AUDIO ---

    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    // --- FONCTIONS DE BASE & FADE ---

    public IEnumerator Fade(CanvasGroup cg, float targetAlpha, float duration)
    {
        if (cg == null) yield break;

        if (targetAlpha > 0)
        {
            cg.gameObject.SetActive(true);
            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
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

        if (targetAlpha <= 0.05f) cg.gameObject.SetActive(false);
    }

    // --- TRANSITIONS ---

    IEnumerator TransitionToOptions()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        StartCoroutine(Fade(submainMenuGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(optionsGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    IEnumerator TransitionBackFromOptions()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        StartCoroutine(Fade(optionsGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(submainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    IEnumerator TransitionToCredits()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        StartCoroutine(Fade(submainMenuGroup, 0, fadeDuration));
        StartCoroutine(Fade(mainMenuGroup, 0, fadeDuration));
        StartCoroutine(Fade(creditsGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(subcreditsGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    IEnumerator TransitionBackFromCredits()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        StartCoroutine(Fade(subcreditsGroup, 0, fadeDuration));
        StartCoroutine(Fade(creditsGroup, 0, fadeDuration));
        StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(submainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    IEnumerator TransitionToLevelSelection()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        StartCoroutine(Fade(submainMenuGroup, 0, fadeDuration));
        StartCoroutine(Fade(mainMenuGroup, 0, fadeDuration));
        yield return StartCoroutine(Fade(levelSelectGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    IEnumerator TransitionBackFromLevelSelection()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        yield return StartCoroutine(Fade(levelSelectGroup, 0, fadeDuration));
        StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(submainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
    }

    // --- LOGIQUE DE JEU ---

    public void LoadLevel(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("LevelReached", 1);
        PlayerPrefs.SetInt("TotalFragments", 0);
        LoadLevel(firstLevelSceneName);
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
                StartCoroutine(TitleToMenuTransition());
        }

        if (calibrationTab.alpha > 0.9f && Input.GetKeyDown(KeyCode.Space))
            TestLatencyInput();
    }

    IEnumerator TitleToMenuTransition()
    {
        isTransitioning = true;
        canProceedToMenu = false;
        yield return StartCoroutine(Fade(titleGroup, 0, fadeDuration));
        titleGroup.gameObject.SetActive(false);
        StartCoroutine(Fade(mainMenuGroup, 1, fadeDuration));
        yield return StartCoroutine(Fade(submainMenuGroup, 1, fadeDuration));
        isTransitioning = false;
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
                pressAnyKeyText.alpha = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
            yield return null;
        }
    }

    public void TestLatencyInput()
    {
        if (BeatManager.Instance != null)
        {
            float musicTimer = BeatManager.Instance.GetMusicTimer();
            float lastBeat = BeatManager.Instance.GetLastBeatTime();
            float offset = (musicTimer - lastBeat) * 1000f;
            latencyOffsets.Add(offset);
            if (latencyOffsets.Count > 5) latencyOffsets.RemoveAt(0);
            float average = latencyOffsets.Average();
            latencyText.text = $"LATENCE DÉTECTÉE : {Mathf.RoundToInt(average)}ms";
        }
    }
}