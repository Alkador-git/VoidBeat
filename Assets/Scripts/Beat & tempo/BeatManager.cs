using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;
    public enum ZoneType { Checkpoint, Narratif, Action, DemoObstacle, Transition }
    public enum RecordingType { Automatic, Manual }

    public static System.Action OnBeat1;
    public static System.Action OnBeat2;
    public static System.Action OnBeat4;
    public static System.Action OnBeat8;
    public static System.Action<string> OnInputFeedback;

    [HideInInspector] public int beatCounter = 0;

    public ZoneSettings currentZone;
    public float currentBPM;

    public float slowMoFactor = 0.5f;
    private bool isSlowMoActive = false;
    private float globalSpeedMultiplier = 1f;

    public AudioSource musicSource;
    public AudioClip musicClip;
    public AudioMixer mixer;
    public float originalMusicBPM = 90f;
    public string lowPassParam = "LowPassFreq";
    public string musicVolParam = "MusicVol";
    public string pitchCompParam = "PitchComp";

    [Range(0f, 1f)]
    public float speedUpCompFactor = 1.0f;

    public float normalVol = 0f;
    public float narrativeVol = -15f;
    public float normalCutoff = 22000f;
    public float narrativeCutoff = 2000f;
    public float fadeSpeed = 2f;

    public BeatData dataContainer;
    public Transform playerTransform;
    public bool isRecordingMode = false;
    public RecordingType currentRecordingType = RecordingType.Automatic;

    public float beatWindow = 0.15f;

    private float beatInterval;
    private float lastBeatTime;
    private float musicTimer = 0f;
    private Coroutine audioTransitionCoroutine;
    private bool isMusicStarted = false;

    private int consecutiveMisses = 0;
    private float lastRewardedBeatTime = -1f;

    // --- INITIALISATION ET MISE A JOUR ---

    /// Vérification et instanciation du Singleton au lancement.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Configuration initiale des filtres et du tempo de départ.
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

    /// Suivi chronométrique de la piste audio et distribution des battements.
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

    /// Déclenchement des événements C# abonnés aux sous-intervalles rythmiques.
    private void TriggerIntervalEvents(int count)
    {
        OnBeat1?.Invoke();

        if (count % 2 == 0) OnBeat2?.Invoke();
        if (count % 4 == 0) OnBeat4?.Invoke();
        if (count % 8 == 0) OnBeat8?.Invoke();
    }

    // --- GESTION AUDIO ET TEMPO ---

    /// Commutation des variables de vitesse pour le mode ralenti.
    private void ToggleSlowMotion()
    {
        isSlowMoActive = !isSlowMoActive;
        globalSpeedMultiplier = isSlowMoActive ? slowMoFactor : 1f;

        ApplyZoneEffects();
        UpdateTempoCalculations();
    }

    /// Indique si le cycle musical de gameplay est actif et en cours de lecture.
    public bool IsMusicActive()
    {
        return isMusicStarted && musicSource != null && musicSource.isPlaying;
    }

    /// Restitue la position temporelle précise de lecture de la musique.
    public float GetMusicTimer()
    {
        if (musicSource != null && musicSource.clip != null && isMusicStarted)
        {
            return (float)musicSource.timeSamples / musicSource.clip.frequency;
        }
        return musicTimer;
    }

    /// Renvoie l'instant exact en secondes du dernier battement calculé.
    public float GetLastBeatTime() => lastBeatTime;

    /// Réaligne la position et les compteurs temporels après un respawn.
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

    /// Initialise et démarre la lecture physique du morceau sélectionné.
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

    /// Lissage logarithmique de l'atténuation des filtres audio du mixeur.
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

    /// Calcule et applique le pitch compensatoire de la piste audio.
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

    // --- GESTION DES ZONES ---

    /// Initialise la musique si le joueur entre en collision avec le point de départ.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isMusicStarted) StartMusic();
    }

    /// Charge et propage les modifications inhérentes au changement de zone.
    public void EnterNewZone(ZoneSettings newSettings)
    {
        currentZone = newSettings;
        ApplyZoneEffects();
    }

    /// Applique les échelles de temps et lance la coroutine de fondu du mixeur.
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

    /// Adapte linéairement la valeur du BPM selon l'avancement géographique du joueur.
    public void UpdateZoneProgress(float progress)
    {
        if (currentZone == null) return;
        currentBPM = Mathf.Lerp(currentZone.bpmStart, currentZone.bpmEnd, progress);
        UpdateTempoCalculations();
    }

    // --- VALIDATION ET ENREGISTREMENT DU RYTHME ---

    /// Insère un repère de battement manuel à la position actuelle du personnage.
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

    /// Exécute l'écriture physique sécurisée du fichier YAML à la désactivation.
    private void OnDisable()
    {
        if (isRecordingMode && dataContainer != null)
        {
            dataContainer.SaveData();
        }
    }

    /// Calcule le ratio décimal de progression séparant le morceau entre deux balises de données.
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

    /// Renvoie le numéro ordonné 1-basé du battement vers lequel converge la lecture.
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

    /// Évalue la précision temporelle d'un input utilisateur et neutralise les requêtes doublonnées sur un même beat.
    public bool IsActionOnBeat(bool applyBoost = true)
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

        if (Mathf.Approximately(targetBeatTime, lastRewardedBeatTime))
        {
            return false;
        }

        string feedback = ""; float boostPercent = 0f;
        if (deltaMs <= 30f)
        {
            feedback = "parfait"; boostPercent = 0.14f;
            consecutiveMisses = 0;
        }
        else if (deltaMs <= 70f)
        {
            feedback = "bien"; boostPercent = 0.1f; consecutiveMisses = 0;
        }
        else if (deltaMs <= 125f)
        {
            feedback = "juste"; boostPercent = 0.075f; consecutiveMisses = 0;
        }
        else
        {
            feedback = "raté"; consecutiveMisses++; boostPercent = (consecutiveMisses >= 2) ? -0.1f : -0.05f;
        }

        lastRewardedBeatTime = targetBeatTime;

        OnInputFeedback?.Invoke(feedback);

        if (BoostManager.Instance != null && applyBoost)
        {
            float max = BoostManager.Instance.maxBoost;
            float rewardAmount = max * boostPercent;
            if (rewardAmount >= 0f) BoostManager.Instance.AddBoost(rewardAmount);
            else BoostManager.Instance.RemoveBoost(Mathf.Abs(rewardAmount));
        }
        return (minDelta <= beatWindow);
    }
}