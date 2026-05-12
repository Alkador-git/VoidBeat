using UnityEngine;
using System.Collections;

public class SlideObstacleMoving : MonoBehaviour
{
    // --- CONFIGURATION ---

    [Header("Réglages")]
    public float boostPenalty = 5f;

    // --- COLLISION EFFECTS ---

    [Header("Effets de Collision")]
    public float shakeMagnitude = 0.1f;

    public float shakeDuration = 0.15f;

    // --- COLLISION DETECTION ---

    /// Handles collision with player.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<KZ0Controller>(out KZ0Controller controller))
            {
                ApplyPenalty(controller);
            }
        }
    }

    /// Applies boost penalty and knockback when enemy hits player.
    private void ApplyPenalty(KZ0Controller controller)
    {
        if (BoostManager.Instance != null)
            BoostManager.Instance.RemoveBoost(boostPenalty);

        if (CinemachineShake.Instance != null)
            StartCoroutine(CinemachineShake.Instance.Shake(shakeDuration, 0.5f, 10.0f));
    }
}