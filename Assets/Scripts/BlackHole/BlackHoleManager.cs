using UnityEngine;

public class BlackHoleManager : MonoBehaviour
{
    // --- PROXIMITÉ ---

    [Header("Paramètres de Proximité")]
    public Transform player;
    public float safeDistance = 15f;
    public float deathDistance = 2f;

    // --- VISUELS ---

    [Header("Visuels (Néant-X)")]
    public SpriteRenderer blackHoleOverlay;
    public Color voidColor = new Color(0.12f, 0f, 0.12f, 0.8f);

    // --- COLLISIONS ---

    [Header("Colliders de défaite")]
    public Collider2D playerCollider;
    public Collider2D blackHoleCollider;

    private float targetX;

    // --- MISE À JOUR ---

    /// Met à jour la position du trou noir selon le boost du joueur
    void Update()
    {
        // Calculer la position cible basée sur le Boost Cinétique 
        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float currentDistance = Mathf.Lerp(deathDistance, safeDistance, boostFactor);

        // Positionner le trou noir par rapport au joueur 
        targetX = player.position.x - currentDistance;

        // Interpolation fluide pour un effet de "souffle" organique
        transform.position = new Vector2(Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * 2f), 1.5f);
    }

    /// Met à jour la position instantanément la position du trou ooir
    public void SnapToPosition()
    {
        if (player == null) return;

        // On recalcule immédiatement la distance voulue selon le boost actuel
        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float currentDistance = Mathf.Lerp(deathDistance, safeDistance, boostFactor);

        // On force la position X sans Lerp
        float instantX = player.position.x - currentDistance;
        transform.position = new Vector2(instantX, transform.position.y);

        // On met à jour targetX pour éviter que le Lerp ne reprenne de l'ancienne valeur
        targetX = instantX;
    }

    // --- DÉTECTION ---

    /// Gère la collision du trou noir avec le joueur
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Collision entre les deux colliders assignés
        if ((collision == playerCollider && blackHoleCollider != null && collision.IsTouching(blackHoleCollider)) ||
            (collision == blackHoleCollider && playerCollider != null && collision.IsTouching(playerCollider)))
        {
            TriggerSpaghettification();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            CheckpointManager.Instance.RespawnPlayer(player);
        }
    }

    // --- EFFETS ---

    /// Déclenche l'absorption du joueur par le trou noir
    void TriggerSpaghettification()
    {
        Debug.Log("K-Z0 est absorbé par le Néant-X...");
    }
}