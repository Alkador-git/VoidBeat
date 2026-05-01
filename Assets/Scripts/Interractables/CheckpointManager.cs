using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    // --- ÉTAT ---

    [Header("État du Checkpoint")]
    public Vector3 lastCheckpointPosition;
    public float boostOnRespawn = 60f;

    // --- INITIALISATION ---

    /// Initialise l'instance singleton
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Enregistre la position initiale du joueur comme checkpoint
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) lastCheckpointPosition = player.transform.position;
    }

    // --- GESTION ---

    /// Définit la position du dernier checkpoint
    public void SetCheckpoint(Vector3 newPos)
    {
        lastCheckpointPosition = newPos;
    }

    /// Réapparaît le joueur au dernier checkpoint avec boost restauré
    public void RespawnPlayer(GameObject player)
    {
        player.transform.position = lastCheckpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (BoostManager.Instance != null)
        {
            BoostManager.Instance.currentBoost = boostOnRespawn;
        }
    }
}