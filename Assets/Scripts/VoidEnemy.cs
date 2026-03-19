using UnityEngine;

public class VoidEnemy : MonoBehaviour
{
    [Header("Réglages")]
    public float boostReward = 10f;
    public float boostPenalty = 5f;
    public GameObject deathEffect;

    public void TakeHit()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
        {
            BoostManager.Instance.AddBoost(boostReward);
            Debug.Log("<color=cyan>Extraction réussie !</color>");
        }
        else
        {
            Debug.Log("<color=orange>Coup hors-tempo...</color>");
        }
        Die();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // On vérifie si c'est le joueur
        if (other.CompareTag("Player"))
        {
            if (BoostManager.Instance != null)
            {
                BoostManager.Instance.RemoveBoost(boostPenalty);
                Debug.Log("<color=red>Collision ! Perte de Boost.</color>");
            }

            Die();
        }
    }

    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}