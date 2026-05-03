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

    [Header("Détection de Rythme")]
    public float beatWindow = 0.15f;

    private float beatInterval;
    private float lastBeatTime;
    private float musicTimer = 0f;
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
            musicTimer += Time.deltaTime;

            beatInterval = 60f / currentBPM;

            if (musicTimer >= lastBeatTime + beatInterval)
            {
                lastBeatTime += beatInterval;
            }
        }
    }

    // --- GETTERS POUR LE CHECKPOINT ---
    public float GetMusicTimer() => musicTimer;
    public float GetAudioTime()
    {
        if (musicSource != null && musicSource.clip != null && isMusicStarted)
        {
            return musicSource.time;
        }
        return 0f;
    }

    public float GetLastBeatTime() => lastBeatTime;

    // --- RESTAURATION DE LA MUSIQUE ---
    public void RestorePlayback(float timer, float audioTime, float lastBeat, float bpm)
    {
        // Sécurité : Ne rien faire si la musique n'a pas encore démarré
        if (!isMusicStarted) return;

        if (musicSource != null && musicClip != null)
        {
            // 1. On restaure les variables de rythme internes
            musicTimer = timer;
            lastBeatTime = lastBeat;
            currentBPM = bpm;

            // 2. On recalcule le pitch et le beat interval
            UpdateTempoCalculations();

            // 3. On repositionne la tête de lecture exacte de la musique
            musicSource.time = Mathf.Clamp(audioTime, 0f, musicClip.length - 0.001f);

            // 4. On force la lecture si elle s'était arrêtée
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }

            Debug.Log($"[Respawn Audio] Musique recalée à : {audioTime}s");
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
        if (musicSource == null || !musicSource.isPlaying) return false;

        float timeSinceLastBeat = musicTimer - lastBeatTime;
        float timeToNextBeat = beatInterval - timeSinceLastBeat;

        return (timeSinceLastBeat <= beatWindow || timeToNextBeat <= beatWindow);
    }
}