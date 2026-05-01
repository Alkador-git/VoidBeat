using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;

    public enum ZoneType { Checkpoint, Narratif, Action, DemoObstacle, Transition }

    [Header("État de la Zone Actuelle")]
    public ZoneSettings currentZone;
    public float currentBPM;

    [Header("Configuration Audio")]
    public AudioSource musicSource;
    public AudioClip musicClip;
    public AudioMixer mixer;
    public float originalMusicBPM = 90f;
    public string lowPassParam = "LowPassFreq";
    public string musicVolParam = "MusicVol";

    [Header("Réglages des Transitions")]
    public float normalVol = 0f;
    public float narrativeVol = -15f;
    public float normalCutoff = 22000f;
    public float narrativeCutoff = 2000f;
    public float fadeSpeed = 2f;

    [Header("Détection de Rythme")]
    public float beatWindow = 0.15f;

    private float beatInterval;
    private float lastBeatTime;
    private Coroutine audioTransitionCoroutine;
    private bool isMusicStarted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        mixer.SetFloat(lowPassParam, normalCutoff);
        mixer.SetFloat(musicVolParam, normalVol);

        currentBPM = originalMusicBPM;
        UpdateTempoCalculations();
    }

    void Update()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            float songTime = musicSource.time;
            if (songTime >= lastBeatTime + beatInterval)
            {
                lastBeatTime += beatInterval;
            }
        }
    }

    // --- DÉCLENCHEMENT PAR COLLIDER ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMusicStarted)
        {
            StartMusic();
        }
    }

    /// Force le démarrage de la musique et initialise le rythme.
    public void StartMusic()
    {
        if (isMusicStarted) return;

        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
            isMusicStarted = true;

            lastBeatTime = musicSource.time;
            UpdateTempoCalculations();

            Debug.Log("Musique de niveau démarrée par trigger");
        }
        else
        {
            Debug.LogWarning("BeatManager : Impossible de démarrer. AudioSource ou AudioClip manquant.");
        }
    }

    public void EnterNewZone(ZoneSettings newSettings)
    {
        currentZone = newSettings;
        ApplyZoneEffects();
    }

    private void ApplyZoneEffects()
    {
        if (currentZone == null) return;

        float targetVolume = currentZone.useLowPass ? narrativeVol : normalVol;
        float targetCutoff = currentZone.useLowPass ? narrativeCutoff : normalCutoff;

        if (audioTransitionCoroutine != null) StopCoroutine(audioTransitionCoroutine);
        audioTransitionCoroutine = StartCoroutine(FadeAudioRoutine(targetVolume, targetCutoff));

        Time.timeScale = currentZone.timeScale;
    }

    private IEnumerator FadeAudioRoutine(float targetdB, float targetFreq)
    {
        float currentdB;
        float currentFreq;

        mixer.GetFloat(musicVolParam, out currentdB);
        mixer.GetFloat(lowPassParam, out currentFreq);

        while (!Mathf.Approximately(currentdB, targetdB) || !Mathf.Approximately(currentFreq, targetFreq))
        {
            currentdB = Mathf.MoveTowards(currentdB, targetdB, fadeSpeed * 15f * Time.deltaTime);
            mixer.SetFloat(musicVolParam, currentdB);

            currentFreq = Mathf.MoveTowards(currentFreq, targetFreq, fadeSpeed * 15000f * Time.deltaTime);
            mixer.SetFloat(lowPassParam, currentFreq);

            yield return null;

            mixer.GetFloat(musicVolParam, out currentdB);
            mixer.GetFloat(lowPassParam, out currentFreq);
        }
    }

    // --- Tempo & Détection ---
    public void UpdateZoneProgress(float progress)
    {
        if (currentZone == null) return;
        currentBPM = Mathf.Lerp(currentZone.bpmStart, currentZone.bpmEnd, progress);
        UpdateTempoCalculations();
    }

    private void UpdateTempoCalculations()
    {
        beatInterval = 60f / currentBPM;
        if (musicSource != null)
        {
            musicSource.pitch = currentBPM / originalMusicBPM;
        }
    }

    public bool IsActionOnBeat()
    {
        if (musicSource == null || !musicSource.isPlaying) return false;
        float currentSongTime = musicSource.time;
        float timeSinceLastBeat = currentSongTime - lastBeatTime;
        float timeToNextBeat = beatInterval - timeSinceLastBeat;
        return (timeSinceLastBeat <= beatWindow || timeToNextBeat <= beatWindow);
    }
}