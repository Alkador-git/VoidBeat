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
    public string pitchCompParam = "PitchComp";

    [Header("Dosing de la Compensation (0 à 1)")]
    [Range(0f, 1f)]
    public float speedUpCompFactor = 1.0f;

    [Header("Réglages des Transitions")]
    public float normalVol = 0f;
    public float narrativeVol = -15f;
    public float normalCutoff = 22000f;
    public float narrativeCutoff = 2000f;
    public float fadeSpeed = 2f;


    [Header("Enregistrement Beats")]
    public BeatData dataContainer;
    public Transform playerTransform;
    public bool isRecordingMode = false;

    [Header("Détection de Rythme")]
    public float beatWindow = 0.15f;

    private float beatInterval;
    private float lastBeatTime;
    private float musicTimer = 0f;
    private Coroutine audioTransitionCoroutine;
    private bool isMusicStarted = false;

    private int consecutiveMisses = 0;
    private float lastRewardedBeatTime = -1f;

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
        if (musicSource != null && musicSource.isPlaying && musicSource.clip != null)
        {
            musicTimer = (float)musicSource.timeSamples / musicSource.clip.frequency;
            beatInterval = 60f / currentBPM;

            if (musicTimer >= lastBeatTime + beatInterval)
            {
                lastBeatTime += beatInterval;

                if (isRecordingMode && dataContainer != null && playerTransform != null)
                {
                    dataContainer.recordedBeats.Add(playerTransform.position.x);
                }
            }
        }
    }

    public float GetMusicTimer()
    {
        if (musicSource != null && musicSource.clip != null && isMusicStarted)
        {
            return (float)musicSource.timeSamples / musicSource.clip.frequency;
        }
        return musicTimer;
    }

    public float GetLastBeatTime() => lastBeatTime;

    public void RestorePlayback(float timer, int sampleTarget, float lastBeat, float bpm)
    {
        if (!isMusicStarted) return;

        if (musicSource != null && musicClip != null)
        {
            currentBPM = bpm;
            UpdateTempoCalculations();

            musicSource.timeSamples = Mathf.Clamp(sampleTarget, 0, musicClip.samples - 1);

            musicTimer = (float)musicSource.timeSamples / musicClip.frequency;
            lastBeatTime = lastBeat;
            consecutiveMisses = 0;
            lastRewardedBeatTime = -1f;

            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }

            Debug.Log($"[Respawn Audio] Musique recalée exactement à : {musicSource.timeSamples} samples");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMusicStarted)
        {
            StartMusic();
        }
    }

    public void StartMusic()
    {
        if (isMusicStarted) return;

        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
            isMusicStarted = true;

            musicTimer = 0f;
            lastBeatTime = 0f;
            consecutiveMisses = 0;
            lastRewardedBeatTime = -1f;
            UpdateTempoCalculations();

            Debug.Log("Musique de niveau démarrée");
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
            float targetPitch = currentBPM / originalMusicBPM;
            musicSource.pitch = targetPitch;

            if (mixer != null && targetPitch > 0)
            {
                float compensatedPitch = 1f;

                if (targetPitch >= 1f)
                {
                    float fullCompensation = 1f / targetPitch;
                    compensatedPitch = Mathf.Lerp(1f, fullCompensation, speedUpCompFactor);
                }
                else
                {
                    compensatedPitch = 1f;
                }

                compensatedPitch = Mathf.Clamp(compensatedPitch, 0.5f, 2.0f);
                mixer.SetFloat(pitchCompParam, compensatedPitch);
            }
        }
    }

    public bool IsActionOnBeat()
    {
        if (musicSource == null || !musicSource.isPlaying || musicSource.clip == null) return false;

        musicTimer = (float)musicSource.timeSamples / musicSource.clip.frequency;

        float timeSinceLastBeat = musicTimer - lastBeatTime;
        float timeToNextBeat = beatInterval - timeSinceLastBeat;

        float closestDistance = Mathf.Min(timeSinceLastBeat, timeToNextBeat);
        float deltaMs = closestDistance * 1000f;

        float closestBeatTarget = (timeSinceLastBeat < timeToNextBeat) ? lastBeatTime : lastBeatTime + beatInterval;

        string feedback = "";
        float boostPercent = 0f;
        bool isIgnoredSuccess = false;

        if (deltaMs <= 35f)
        {
            feedback = "parfait";
            boostPercent = 0.05f;
            consecutiveMisses = 0;
        }
        else if (deltaMs <= 75f)
        {
            feedback = "bien";
            boostPercent = 0.025f;
            consecutiveMisses = 0;
        }
        else if (deltaMs <= 150f)
        {
            feedback = "juste";
            boostPercent = 0.015f;
            consecutiveMisses = 0;
        }
        else
        {
            feedback = "raté";
            consecutiveMisses++;
            boostPercent = (consecutiveMisses >= 2) ? -0.1f : -0.05f;
        }

        if (deltaMs <= 150f)
        {
            if (Mathf.Approximately(closestBeatTarget, lastRewardedBeatTime))
            {
                isIgnoredSuccess = true;
            }
            else
            {
                lastRewardedBeatTime = closestBeatTarget;
            }
        }

        if (BoostManager.Instance != null && !isIgnoredSuccess)
        {
            float max = BoostManager.Instance.maxBoost;
            float rewardAmount = max * boostPercent;

            if (rewardAmount >= 0f) BoostManager.Instance.AddBoost(rewardAmount);
            else BoostManager.Instance.RemoveBoost(Mathf.Abs(rewardAmount));

            Debug.Log($"Rhythm Input - Ecart: {deltaMs:F1} ms ({feedback}), Ratés d'affilée: {consecutiveMisses}");
        }
        else if (isIgnoredSuccess)
        {
            Debug.Log($"Rhythm Input ignoré (Déjà validé pour ce beat).");
        }

        return (timeSinceLastBeat <= beatWindow || timeToNextBeat <= beatWindow);
    }
}