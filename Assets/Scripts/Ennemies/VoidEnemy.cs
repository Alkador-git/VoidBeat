using UnityEngine;
using System.Collections;

public class VoidEnemy : MonoBehaviour
{
    // --- RÉGLAGES ---
    [Header("Réglages")]
    public float boostReward = 10f;
    public float boostPenalty = 5f;
    public GameObject deathEffect;

    // --- EFFETS DE COLLISION ---
    [Header("Effets de Collision")]
    public float shakeMagnitude = 0.2f;
    public float shakeDuration = 0.15f;
    public Vector2 knockbackForce = new Vector2(-1.5f, 0.5f);

    // --- LOGIQUE INTERNE ---
    private bool wasHit = false; // Empêche la punition si l'ennemi meurt pendant la grâce

    /// Gère l'impact quand l'ennemi est frappé par le sabre du joueur
    public void TakeHit()
    {
        if (wasHit) return;
        wasHit = true;

        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
        {
            BoostManager.Instance.AddBoost(boostReward);
            Debug.Log("<color=cyan>Extraction réussie !</color>");
        }
        else
        {
            Debug.Log("<color=orange>Coup hors-tempo</color>");
        }

        Die();
    }

    // --- DÉTECTION ---

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

    /// Fenêtre de tolérance : on attend un peu avant de punir
    private IEnumerator GracePeriodCoroutine(KZ0Controller controller)
    {
        controller.NotifyEnemyCollision();

        yield return new WaitForSeconds(controller.collisionGraceDuration);

        if (!wasHit)
        {
            ApplyPenalty(controller);
        }
    }

    private void ApplyPenalty(KZ0Controller controller)
    {
        if (BoostManager.Instance != null)
            BoostManager.Instance.RemoveBoost(boostPenalty);

        controller.ApplyKnockback(knockbackForce);

        if (CameraShake.Instance != null)
            StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeMagnitude));

        Debug.Log("<color=red>Punition : Fenêtre de grâce expirée.</color>");

        Die();
    }

    // --- DESTRUCTION ---

    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}