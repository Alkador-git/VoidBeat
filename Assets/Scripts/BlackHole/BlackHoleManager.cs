using UnityEngine;
using System.Collections.Generic;

public class BlackHoleManager : MonoBehaviour
{
    public static BlackHoleManager Instance;

    [Header("Paramètres de Proximité")]
    public Transform player;
    public float maxFollowDistance = 20f;
    public float deathDistance = 2f;

    [Header("Réglages de Vitesse (Néant-X)")]
    public float speedAtZeroBoost = 5f;
    public float speedAtHalfBoost = 3.5f;
    public float speedAtFullBoost = 2.5f;

    [Header("Visuels (Néant-X)")]
    public MeshRenderer blackHoleRenderer;

    [Header("Colliders de défaite")]
    public Collider2D playerCollider;
    public Collider2D blackHoleCollider;

    private float currentMoveSpeed;
    private float targetX;
    private Camera mainCamera;

    // --- INITIALISATION ---

    /// Établissement du singleton et mise en mémoire de la caméra principale.
    void Awake()
    {
        if (Instance == null) Instance = this;
        mainCamera = Camera.main;
    }

    // --- PHYSIQUE ET RENDU SHADER ---

    /// Traitement du déplacement cinétique et projection des coordonnées mondiales sur l'écran.
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

        UpdateShaderGlobalParameters();
    }

    /// Calcule et injecte la position et l'intensité réelles du trou noir dans la mémoire globale d'URP.
    private void UpdateShaderGlobalParameters()
    {
        if (mainCamera == null) return;

        Vector3 screenPos = mainCamera.WorldToViewportPoint(transform.position);

        Shader.SetGlobalVector("_CustomBlackHoleScreenPos", new Vector4(screenPos.x, screenPos.y, 0f, 0f));

        float sizeFactor = transform.localScale.x;
        Shader.SetGlobalFloat("_CustomBlackHoleIntensity", sizeFactor);
    }

    // --- COMPORTEMENT ---

    /// Positionne instantanément le trou noir derrière le joueur.
    public void SnapToPosition()
    {
        if (player == null || BoostManager.Instance == null) return;

        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float currentDistance = Mathf.Lerp(maxFollowDistance, deathDistance + 5f, boostFactor);

        float instantX = player.position.x - currentDistance;
        transform.position = new Vector2(instantX, 1.5f);

        targetX = instantX;
        UpdateShaderGlobalParameters();
    }

    /// Analyse les volumes d'entrée pour intercepter le contact physique destructeur.
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

    /// Déclenche les effets de distorsion critiques de spaghettification.
    void TriggerSpaghettification()
    {
    }
}