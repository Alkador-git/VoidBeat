using UnityEngine;

public class BlackHoleManager : MonoBehaviour
{
    // --- PROXIMITY SETTINGS ---

    [Header("Paramètres de Proximité")]
    public Transform player;

    public float safeDistance = 15f;

    public float deathDistance = 2f;

    // --- VISUALS ---

    [Header("Visuels (Néant-X)")]
    public SpriteRenderer blackHoleOverlay;

    public Color voidColor = new Color(0.12f, 0f, 0.12f, 0.8f);

    // --- COLLIDERS ---

    [Header("Colliders de défaite")]
    public Collider2D playerCollider;

    public Collider2D blackHoleCollider;

    private float targetX;

    // --- UPDATE LOOP ---

    void Update()
    {
        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float currentDistance = Mathf.Lerp(deathDistance, safeDistance, boostFactor);

        targetX = player.position.x - currentDistance;

        transform.position = new Vector2(Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * 2f), 1.5f);
    }

    // --- INSTANT POSITIONING ---

    /// Instantly snaps black hole to correct position.
    public void SnapToPosition()
    {
        if (player == null) return;

        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float currentDistance = Mathf.Lerp(deathDistance, safeDistance, boostFactor);

        float instantX = player.position.x - currentDistance;
        transform.position = new Vector2(instantX, transform.position.y);

        targetX = instantX;
    }

    // --- COLLISION DETECTION ---

    /// Handles black hole collision with player.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision == playerCollider && blackHoleCollider != null && collision.IsTouching(blackHoleCollider)) ||
            (collision == blackHoleCollider && playerCollider != null && collision.IsTouching(playerCollider)))
        {
            TriggerSpaghettification();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            CheckpointManager.Instance.RespawnPlayer(player);
        }
    }

    // --- EFFECTS ---

    /// Triggers spaghettification visual effect.
    void TriggerSpaghettification()
    {
    }
}