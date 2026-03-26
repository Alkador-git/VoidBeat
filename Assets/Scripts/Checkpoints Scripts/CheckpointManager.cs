using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("État du Checkpoint")]
    public Vector3 lastCheckpointPosition;
    public float boostOnRespawn = 60f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) lastCheckpointPosition = player.transform.position;
    }

    public void SetCheckpoint(Vector3 newPos)
    {
        lastCheckpointPosition = newPos;
        Debug.Log("Checkpoint");
    }

    public void RespawnPlayer(GameObject player)
    {
        player.transform.position = lastCheckpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (BoostManager.Instance != null)
        {
            BoostManager.Instance.currentBoost = boostOnRespawn;
        }

        Debug.Log("Récupération de K-Z0 effectuée.");
    }
}