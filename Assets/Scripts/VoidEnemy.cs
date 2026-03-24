using UnityEngine;

public class VoidEnemy : MonoBehaviour
{
    [Header("Réglages")]
    public float boostReward = 10f;
    public float boostPenalty = 5f;
    public GameObject deathEffect;

    // Gère l'impact de l'ennemi lorsqu'il est touché
    public void TakeHit()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
        {
            BoostManager.Instance.AddBoost(boostReward);
            Debug.Log("Extraction réussie");
        }
        else
        {
            Debug.Log("Coup hors-tempo");
        }
        Die();
    }

    [Header("Effets de Collision")]
    public float shakeMagnitude = 0.2f;
    public float shakeDuration = 0.15f;
    public Vector2 knockbackForce = new Vector2(-1.5f, 0.5f);

    // Gère la collision avec le joueur
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Perte de Boost
            if (BoostManager.Instance != null)
                BoostManager.Instance.RemoveBoost(boostPenalty);

            // Knockback
            if (other.TryGetComponent<KZ0Controller>(out KZ0Controller controller))
                controller.ApplyKnockback(knockbackForce);

            // Screen Shake
            if (CameraShake.Instance != null)
                StartCoroutine(CameraShake.Instance.Shake(shakeDuration, shakeMagnitude));

            Debug.Log("Collision ennemie");
            Die();
        }
    }


    // Détruit l'ennemi et affiche l'effet de mort
    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}