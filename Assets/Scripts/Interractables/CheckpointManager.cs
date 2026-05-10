using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    // --- CHECKPOINT STATE ---

    public Vector3 lastCheckpointPosition;

    public float boostOnRespawn = 60f;

    // --- MUSIC STATE SAVING ---

    private float savedMusicTimer;

    private int savedTimeSamples;

    private float savedLastBeatTime;

    private float savedBPM;

    // --- INITIALIZATION ---

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Sets up initial checkpoint from player position.
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

    // --- CHECKPOINT MANAGEMENT ---

    /// Saves current checkpoint position and music state.
    public void SetCheckpoint(Vector3 newPos)
    {
        lastCheckpointPosition = newPos;

        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null && BeatManager.Instance.musicSource.clip != null)
        {
            savedMusicTimer = BeatManager.Instance.GetMusicTimer();
            savedTimeSamples = BeatManager.Instance.musicSource.timeSamples;
            savedLastBeatTime = BeatManager.Instance.GetLastBeatTime();
            savedBPM = BeatManager.Instance.currentBPM;
        }
    }

    // --- RESPAWN LOGIC ---

    /// Respawns player at last checkpoint with restored music state.
    public void RespawnPlayer(GameObject player)
    {
        player.transform.position = lastCheckpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (BoostManager.Instance != null)
        {
            BoostManager.Instance.currentBoost = boostOnRespawn;
        }

        // --- DIRECT SAMPLE RESTORATION ---
        if (BeatManager.Instance != null && BeatManager.Instance.musicSource != null && BeatManager.Instance.musicSource.clip != null)
        {
            BeatManager.Instance.RestorePlayback(savedMusicTimer, savedTimeSamples, savedLastBeatTime, savedBPM);
        }
    }
}