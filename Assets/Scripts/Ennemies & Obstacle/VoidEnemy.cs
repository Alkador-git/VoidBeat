using UnityEngine;
using System.Collections;

public class VoidEnemy : MonoBehaviour
{
    // --- CONFIGURATION ---

    [Header("Réglages")]
    public float boostReward = 10f;

    public float boostPenalty = 5f;

    public GameObject deathEffect;

    // --- COLLISION EFFECTS ---

    [Header("Effets de Collision")]
    public float shakeMagnitude = 0.1f;

    public float shakeDuration = 0.15f;

    public Vector2 knockbackForce = new Vector2(-1.5f, 0.5f);

    // --- INTERNAL STATE ---

    private bool wasHit = false;

    // --- HIT HANDLING ---

    /// Applies hit damage and rewards on beat.
    public void TakeHit()
    {
        if (wasHit) return;
        wasHit = true;

        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
        {
            BoostManager.Instance.AddBoost(boostReward);
        }

        Die();
    }

    // --- COLLISION DETECTION ---

    /// Handles collision with player.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !wasHit)
        {
            if (other.TryGetComponent<KZ0Controller>(out KZ0Controller controller))
            {
                StartCoroutine(GracePeriodCoroutine(controller));
            }
        }
    }

    // --- GRACE PERIOD ---

    /// Provides collision tolerance window before penalty.
    private IEnumerator GracePeriodCoroutine(KZ0Controller controller)
    {
        controller.NotifyEnemyCollision();

        yield return new WaitForSeconds(controller.collisionGraceDuration);

        if (!wasHit)
        {
            ApplyPenalty(controller);
        }
    }

    /// Applies boost penalty and knockback when enemy hits player.
    private void ApplyPenalty(KZ0Controller controller)
    {
        if (BoostManager.Instance != null)
            BoostManager.Instance.RemoveBoost(boostPenalty);

        controller.ApplyKnockback(knockbackForce);

        if (CinemachineShake.Instance != null)
            StartCoroutine(CinemachineShake.Instance.Shake(shakeDuration, 0.5f  , 10.0f));

        Die();
    }

    // --- DESTRUCTION ---

    /// Destroys enemy and spawns death effect.
    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}