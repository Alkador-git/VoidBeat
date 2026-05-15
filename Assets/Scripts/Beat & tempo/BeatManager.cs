using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;
    public enum ZoneType { Checkpoint, Narratif, Action, DemoObstacle, Transition }
    public enum RecordingType { Automatic, Manual }

    // --- CURRENT ZONE STATE ---
    public ZoneSettings currentZone;
    public float currentBPM;

    // --- SLOW MOTION ---
    public float slowMoFactor = 0.5f;
    private bool isSlowMoActive = false;
    private float globalSpeedMultiplier = 1f;

    // --- AUDIO CONFIGURATION ---
    public AudioSource musicSource;
    public AudioClip musicClip;
    public AudioMixer mixer;
    public float originalMusicBPM = 90f;
    public string lowPassParam = "LowPassFreq";
    public string musicVolParam = "MusicVol";
    public string pitchCompParam = "PitchComp";

    // --- SPEED COMPENSATION ---
    [Range(0f, 1f)]
    public float speedUpCompFactor = 1.0f;

    // --- AUDIO TRANSITIONS ---
    public float normalVol = 0f;
    public float narrativeVol = -15f;
    public float normalCutoff = 22000f;
    public float narrativeCutoff = 2000f;
    public float fadeSpeed = 2f;

    // --- BEAT RECORDING ---
    public BeatData dataContainer;
    public Transform playerTransform;
    public bool isRecordingMode = false;
    public RecordingType currentRecordingType = RecordingType.Automatic;

    // --- BEAT DETECTION ---
    public float beatWindow = 0.15f;

    private float beatInterval;
    private float lastBeatTime;
    private float musicTimer = 0f;
    private Coroutine audioTransitionCoroutine;
    private bool isMusicStarted = false;

    private int consecutiveMisses = 0;
    private float lastRewardedBeatTime = -1f;

    // --- INITIALIZATION ---
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (mixer != null)
        {
            mixer.SetFloat(lowPassParam, normalCutoff);
            mixer.SetFloat(musicVolParam, normalVol);
        }

        currentBPM = originalMusicBPM;
        UpdateTempoCalculations();
    }

    // --- UPDATE LOOP ---
    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.B))
        {
            ToggleSlowMotion();
        }

        if (musicSource != null && musicSource.isPlaying && musicSource.clip != null)
        {
            musicTimer = (float)musicSource.timeSamples / musicSource.clip.frequency;
            beatInterval = 60f / currentBPM;

            if (musicTimer >= lastBeatTime + beatInterval)
            {
                lastBeatTime += beatInterval;

                if (isRecordingMode && currentRecordingType == RecordingType.Automatic && dataContainer != null && playerTransform != null)
                {
                    BeatPoint newBeat = new BeatPoint
                    {
                        xPos = playerTransform.position.x,
                        musicTime = musicTimer
                    };
                    dataContainer.recordedBeats.Add(newBeat);
                }
            }
        }
    }

    private void ToggleSlowMotion()
    {
        isSlowMoActive = !isSlowMoActive;
        globalSpeedMultiplier = isSlowMoActive ? slowMoFactor : 1f;

        ApplyZoneEffects();
        UpdateTempoCalculations();
    }

    // --- BEAT RECORDING ---
    public void RecordManualBeat()
    {
        if (!isRecordingMode || currentRecordingType != RecordingType.Manual) return;
        if (dataContainer == null || playerTransform == null) return;

        BeatPoint manualBeat = new BeatPoint
        {
            xPos = playerTransform.position.x,
            musicTime = (float)musicSource.timeSamples / musicSource.clip.frequency
        };

        dataContainer.recordedBeats.Add(manualBeat);
        dataContainer.recordedBeats.Sort((a, b) => a.musicTime.CompareTo(b.musicTime));
    }

    // --- MUSIC TIMING ---
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

            if (!musicSource.isPlaying) musicSource.Play();
        }
    }

    // --- ZONE MANAGEMENT ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMusicStarted) StartMusic();
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

        Time.timeScale = currentZone.timeScale * globalSpeedMultiplier;
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
            float targetPitch = (currentBPM / originalMusicBPM) * globalSpeedMultiplier;
            musicSource.pitch = targetPitch;

            if (mixer != null && targetPitch > 0)
            {
                float compensatedPitch = 1f;
                if (targetPitch >= 1f)
                {
                    float fullCompensation = 1f / targetPitch;
                    compensatedPitch = Mathf.Lerp(1f, fullCompensation, speedUpCompFactor);
                }
                else compensatedPitch = 1f;

                compensatedPitch = Mathf.Clamp(compensatedPitch, 0.5f, 2.0f);
                mixer.SetFloat(pitchCompParam, compensatedPitch);
            }
        }
    }

    // --- BEAT DETECTION ---
    public bool IsActionOnBeat()
    {
        if (musicSource == null || !musicSource.isPlaying || musicSource.clip == null || dataContainer == null) return false;

        float currentMusicTime = (float)musicSource.timeSamples / musicSource.clip.frequency;

        float minDelta = float.MaxValue;
        float targetBeatTime = -1f;

        foreach (var beat in dataContainer.recordedBeats)
        {
            float delta = Mathf.Abs(currentMusicTime - beat.musicTime);
            if (delta < minDelta)
            {
                minDelta = delta;
                targetBeatTime = beat.musicTime;
            }
        }

        float deltaMs = minDelta * 1000f;
        string feedback = "";
        float boostPercent = 0f;
        bool isIgnoredSuccess = false;

        if (deltaMs <= 35f) { feedback = "parfait"; boostPercent = 0.05f; consecutiveMisses = 0; }
        else if (deltaMs <= 75f) { feedback = "bien"; boostPercent = 0.025f; consecutiveMisses = 0; }
        else if (deltaMs <= 150f) { feedback = "juste"; boostPercent = 0.015f; consecutiveMisses = 0; }
        else
        {
            feedback = "raté";
            consecutiveMisses++;
            boostPercent = (consecutiveMisses >= 2) ? -0.1f : -0.05f;
        }

        if (deltaMs <= 150f)
        {
            if (Mathf.Approximately(targetBeatTime, lastRewardedBeatTime)) isIgnoredSuccess = true;
            else lastRewardedBeatTime = targetBeatTime;
        }

        if (BoostManager.Instance != null && !isIgnoredSuccess)
        {
            float max = BoostManager.Instance.maxBoost;
            float rewardAmount = max * boostPercent;
            if (rewardAmount >= 0f) BoostManager.Instance.AddBoost(rewardAmount);
            else BoostManager.Instance.RemoveBoost(Mathf.Abs(rewardAmount));

            Debug.Log($"[DATA BEAT] Écart: {deltaMs:F1}ms ({feedback})");
        }

        return (minDelta <= beatWindow);
    }
}