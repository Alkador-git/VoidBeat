using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;
    public enum ZoneType { Checkpoint, Narratif, Action, DemoObstacle, Transition }
    public enum RecordingType { Automatic, Manual }

    // --- ENFANTS / EVENTS DU RYTHME ---
    public static System.Action OnBeat1;
    public static System.Action OnBeat2;
    public static System.Action OnBeat4;
    public static System.Action OnBeat8;

    [HideInInspector] public int beatCounter = 0;

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
        Time.timeScale = 1;
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

            if (isRecordingMode)
            {
                beatInterval = 60f / currentBPM;

                if (musicTimer >= lastBeatTime + beatInterval)
                {
                    lastBeatTime += beatInterval;
                    beatCounter++;
                    TriggerIntervalEvents(beatCounter);

                    if (currentRecordingType == RecordingType.Automatic && dataContainer != null && playerTransform != null)
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
            else
            {
                if (dataContainer != null && dataContainer.recordedBeats != null && dataContainer.recordedBeats.Count > 0)
                {
                    List<BeatPoint> beats = dataContainer.recordedBeats;
                    int nextIdx = -1;
                    for (int i = 0; i < beats.Count; i++)
                    {
                        if (beats[i].musicTime > musicTimer)
                        {
                            nextIdx = i;
                            break;
                        }
                    }

                    if (nextIdx > 0)
                    {
                        int currentIdx = nextIdx - 1;
                        lastBeatTime = beats[currentIdx].musicTime;
                        beatInterval = beats[nextIdx].musicTime - lastBeatTime;

                        int newBeatCount = currentIdx + 1;
                        if (newBeatCount > beatCounter)
                        {
                            for (int b = beatCounter + 1; b <= newBeatCount; b++)
                            {
                                TriggerIntervalEvents(b);
                            }
                            beatCounter = newBeatCount;
                        }
                    }
                }
                else
                {
                    beatInterval = 60f / currentBPM;
                    if (musicTimer >= lastBeatTime + beatInterval)
                    {
                        lastBeatTime += beatInterval;
                        beatCounter++;
                        TriggerIntervalEvents(beatCounter);
                    }
                }
            }
        }
    }

    /// Diffuse les actions de rythme globales aux abonnés selon le compteur de temps écoulé.
    private void TriggerIntervalEvents(int count)
    {
        OnBeat1?.Invoke();

        if (count % 2 == 0) OnBeat2?.Invoke();
        if (count % 4 == 0) OnBeat4?.Invoke();
        if (count % 8 == 0) OnBeat8?.Invoke();
    }

    /// Alterne l'activation du mode ralenti cinétique.
    private void ToggleSlowMotion()
    {
        isSlowMoActive = !isSlowMoActive;
        globalSpeedMultiplier = isSlowMoActive ? slowMoFactor : 1f;

        ApplyZoneEffects();
        UpdateTempoCalculations();
    }

    // --- BEAT RECORDING ---

    /// Effectue la capture et le tri ordonné d'un point rythmique saisi manuellement en outil de création.
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

    /// Assure l'écriture physique immédiate sur le disque dur si le mode enregistrement prend fin.
    private void OnDisable()
    {
        if (isRecordingMode && dataContainer != null)
        {
            dataContainer.SaveData();
        }
    }

    // --- NOUVEAUX OUTILS DE CENTRALISATION DES INPUTS ET DÉCOR ---

    /// Calcule la phase de progression normalisée (0 à 1) menant au battement à venir.
    public float GetDataDrivenBeatPhase()
    {
        if (musicSource == null || musicSource.clip == null || !musicSource.isPlaying) return 0f;

        float currentMusicTime = (float)musicSource.timeSamples / musicSource.clip.frequency;

        if (!isRecordingMode && dataContainer != null && dataContainer.recordedBeats != null && dataContainer.recordedBeats.Count > 0)
        {
            List<BeatPoint> beats = dataContainer.recordedBeats;
            int nextIdx = -1;
            for (int i = 0; i < beats.Count; i++)
            {
                if (beats[i].musicTime > currentMusicTime)
                {
                    nextIdx = i;
                    break;
                }
            }

            if (nextIdx >= 0)
            {
                float prevTime = (nextIdx == 0) ? 0f : beats[nextIdx - 1].musicTime;
                float nextTime = beats[nextIdx].musicTime;
                float duration = nextTime - prevTime;
                if (duration > 0f)
                {
                    return Mathf.Clamp01((currentMusicTime - prevTime) / duration);
                }
            }
            return 0f;
        }

        float fallbackInterval = 60f / (currentBPM > 0 ? currentBPM : 120f);
        float phase = (currentMusicTime - lastBeatTime) / fallbackInterval;
        return Mathf.Clamp01(phase);
    }

    /// Détermine le numéro d'indexation humain (1-basé) du battement ciblé par la progression.
    public int GetNextBeatIndex()
    {
        if (musicSource == null || musicSource.clip == null || !musicSource.isPlaying) return -1;

        float currentMusicTime = (float)musicSource.timeSamples / musicSource.clip.frequency;

        if (!isRecordingMode && dataContainer != null && dataContainer.recordedBeats != null && dataContainer.recordedBeats.Count > 0)
        {
            List<BeatPoint> beats = dataContainer.recordedBeats;
            for (int i = 0; i < beats.Count; i++)
            {
                if (beats[i].musicTime > currentMusicTime)
                {
                    return i + 1;
                }
            }
            return -1;
        }

        return beatCounter + 1;
    }

    // --- MUSIC TIMING ---

    /// Restitue la valeur temporelle de lecture de la source audio active.
    public float GetMusicTimer()
    {
        if (musicSource != null && musicSource.clip != null && isMusicStarted)
        {
            return (float)musicSource.timeSamples / musicSource.clip.frequency;
        }
        return musicTimer;
    }

    /// Restitue l'instant précis en secondes où le dernier battement valide est survenu.
    public float GetLastBeatTime() => lastBeatTime;

    /// Restitue l'état complet du morceau lors des phases de réapparition au point de contrôle.
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

            beatCounter = Mathf.FloorToInt(lastBeat / (60f / bpm));

            if (!musicSource.isPlaying) musicSource.Play();
        }
    }

    // --- ZONE MANAGEMENT ---

    /// Lance l'exécution musicale si K-Z0 pénètre dans le volume d'initialisation du niveau.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMusicStarted) StartMusic();
    }

    /// Initialise la configuration et débute la lecture de la piste audio principale.
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
            beatCounter = 0;
            UpdateTempoCalculations();
        }
    }

    /// Met à jour les données environnementales lors de l'accès à un nouveau segment narratif ou d'action.
    public void EnterNewZone(ZoneSettings newSettings)
    {
        currentZone = newSettings;
        ApplyZoneEffects();
    }

    /// Gère les transitions de filtres et de distorsion audio inhérentes à la zone actuelle.
    private void ApplyZoneEffects()
    {
        if (currentZone == null) return;
        float targetVolume = currentZone.useLowPass ? narrativeVol : normalVol;
        float targetCutoff = currentZone.useLowPass ? narrativeCutoff : normalCutoff;

        if (audioTransitionCoroutine != null)
            StopCoroutine(audioTransitionCoroutine);
        audioTransitionCoroutine = StartCoroutine(FadeAudioRoutine(targetVolume, targetCutoff));
        Time.timeScale = currentZone.timeScale * globalSpeedMultiplier;
    }

    /// Assure l'atténuation et la transition fluide des volumes et des fréquences de coupure du Mixer.
    private IEnumerator FadeAudioRoutine(float targetdB, float targetFreq)
    {
        float currentdB; float currentFreq;
        mixer.GetFloat(musicVolParam, out currentdB);
        mixer.GetFloat(lowPassParam, out currentFreq);
        while (!Mathf.Approximately(currentdB, targetdB) || !Mathf.Approximately(currentFreq, targetFreq))
        {
            currentdB = Mathf.MoveTowards(currentdB, targetdB, fadeSpeed * 15f * Time.deltaTime);
            mixer.SetFloat(musicVolParam, currentdB);
            currentFreq = Mathf.MoveTowards(currentFreq, targetFreq, fadeSpeed * 15000f * Time.deltaTime);
            mixer.SetFloat(lowPassParam, currentFreq); yield return null;
            mixer.GetFloat(musicVolParam, out currentdB);
            mixer.GetFloat(lowPassParam, out currentFreq);
        }
    }

    /// Alerte le gestionnaire de l'état d'avancement du joueur pour adapter dynamiquement la valeur du BPM.
    public void UpdateZoneProgress(float progress)
    {
        if (currentZone == null) return;
        currentBPM = Mathf.Lerp(currentZone.bpmStart, currentZone.bpmEnd, progress);
        UpdateTempoCalculations();
    }

    /// Recalcule et adapte la hauteur (pitch) du morceau pour pallier les effets de ralentissement.
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
                    float fullCompensation = 1f / targetPitch; compensatedPitch = Mathf.Lerp(1f, fullCompensation, speedUpCompFactor);
                }
                else compensatedPitch = 1f;
                compensatedPitch = Mathf.Clamp(compensatedPitch, 0.5f, 2.0f);
                mixer.SetFloat(pitchCompParam, compensatedPitch);
            }
        }
    }

    /// Analyse la précision temporelle de l'input utilisateur par rapport aux données du fichier pour attribuer le bonus.
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
                minDelta = delta; targetBeatTime = beat.musicTime;
            }
        }
        float deltaMs = minDelta * 1000f;
        string feedback = ""; float boostPercent = 0f;
        bool isIgnoredSuccess = false;
        if (deltaMs <= 30f)
        {
            feedback = "parfait"; boostPercent = 0.05f;
            consecutiveMisses = 0;
        }
        else if (deltaMs <= 70f)
        {
            feedback = "bien"; boostPercent = 0.025f; consecutiveMisses = 0;
        }
        else if (deltaMs <= 125f)
        {
            feedback = "juste"; boostPercent = 0.015f; consecutiveMisses = 0;
        }
        else
        {
            feedback = "raté"; consecutiveMisses++; boostPercent = (consecutiveMisses >= 2) ? -0.1f : -0.05f;
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
            Debug.Log("[DATA BEAT] Ecart: " + deltaMs.ToString("F1") + "ms (" + feedback + ")");
        }
        return (minDelta <= beatWindow);
    }
}