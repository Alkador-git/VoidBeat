using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("État du Checkpoint")]
    public Vector3 lastCheckpointPosition;
    public float boostOnRespawn = 60f;

    // --- SAUVEGARDE DE LA MUSIQUE ---
    private float savedMusicTimer;
    private int savedTimeSamples;
    private float savedLastBeatTime;
    private float savedBPM;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) lastCheckpointPosition = player.transform.position;

        savedMusicTimer = 0f;
        savedTimeSamples = 0;
        savedLastBeatTime = 0f;
        if (BeatManager.Instance != null)
        {
            savedBPM = BeatManager.Instance.originalMusicBPM;
        }
    }

    /// Définit la position du dernier checkpoint et capture l'état exact de la musique
    public void SetCheckpoint(Vector3 newPos)
    {
        lastCheckpointPosition = newPos;

        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null && BeatManager.Instance.musicSource.clip != null)
        {
            savedMusicTimer = BeatManager.Instance.GetMusicTimer();

            savedTimeSamples = BeatManager.Instance.musicSource.timeSamples;

            savedLastBeatTime = BeatManager.Instance.GetLastBeatTime();
            savedBPM = BeatManager.Instance.currentBPM;

            Debug.Log($"[Checkpoint] Musique enregistrée : Timer={savedMusicTimer}s, Sample={savedTimeSamples}");
        }
    }

    /// Réapparaît le joueur au dernier checkpoint avec boost et musique restaurés
    public void RespawnPlayer(GameObject player)
    {
        player.transform.position = lastCheckpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (BoostManager.Instance != null)
        {
            BoostManager.Instance.currentBoost = boostOnRespawn;
        }

        // --- RESTAURATION DE LA MUSIQUE ---
        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null && BeatManager.Instance.musicSource.clip != null)
        {
            float audioTimeInSeconds = (float)savedTimeSamples / BeatManager.Instance.musicSource.clip.frequency;

            BeatManager.Instance.RestorePlayback(savedMusicTimer, audioTimeInSeconds, savedLastBeatTime, savedBPM);
        }
    }
}