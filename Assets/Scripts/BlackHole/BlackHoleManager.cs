using UnityEngine;

public class BlackHoleManager : MonoBehaviour
{
    public static BlackHoleManager Instance;

    // --- PROXIMITY SETTINGS ---

    [Header("Paramètres de Proximité")]
    public Transform player;
    public float maxFollowDistance = 20f;
    public float deathDistance = 2f;

    // --- SPEED SETTINGS ---
    [Header("Réglages de Vitesse (Néant-X)")]
    public float speedAtZeroBoost = 5f;
    public float speedAtHalfBoost = 3.5f;
    public float speedAtFullBoost = 2.5f;

    // --- VISUALS ---

    [Header("Visuels (Néant-X)")]
    public SpriteRenderer blackHoleOverlay;
    public Color voidColor = new Color(0.12f, 0f, 0.12f, 0.8f);

    // --- COLLIDERS ---

    [Header("Colliders de défaite")]
    public Collider2D playerCollider;
    public Collider2D blackHoleCollider;

    // --- INTERNAL STATE ---
    private float currentMoveSpeed;
    private float targetX;

    // --- INITIALIZATION ---

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // --- UPDATE LOOP ---

    void Update()
    {
        if (player == null || BoostManager.Instance == null) return;

        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;

        if (boostFactor < 0.5f)
        {
            currentMoveSpeed = Mathf.Lerp(speedAtZeroBoost, speedAtHalfBoost, boostFactor * 2f);
        }
        else
        {
            currentMoveSpeed = Mathf.Lerp(speedAtHalfBoost, speedAtFullBoost, (boostFactor - 0.5f) * 2f);
        }

        float nextX = transform.position.x + (currentMoveSpeed * Time.deltaTime);

        float minAllowedX = player.position.x - maxFollowDistance;
        if (nextX < minAllowedX)
        {
            nextX = minAllowedX;
        }

        targetX = nextX;
        transform.position = new Vector2(targetX, 1.5f);
    }

    // --- INSTANT POSITIONING ---

    /// Positionne instantanément le trou noir derrière le joueur
    public void SnapToPosition()
    {
        if (player == null || BoostManager.Instance == null) return;

        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;

        float currentDistance = Mathf.Lerp(maxFollowDistance, deathDistance + 5f, boostFactor);

        float instantX = player.position.x - currentDistance;
        transform.position = new Vector2(instantX, 1.5f);

        targetX = instantX;

        Debug.Log("Néant-X repositionné derrière le joueur au checkpoint.");
    }

    // --- COLLISION DETECTION ---

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision == playerCollider && blackHoleCollider != null && collision.IsTouching(blackHoleCollider)) ||
            (collision == blackHoleCollider && playerCollider != null && collision.IsTouching(playerCollider)))
        {
            TriggerSpaghettification();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                CheckpointManager.Instance.RespawnPlayer(playerObj);
            }
        }
    }

    // --- EFFECTS ---

    void TriggerSpaghettification()
    {
        Debug.Log("K-Z0 a été absorbé par le Néant !");
    }
}