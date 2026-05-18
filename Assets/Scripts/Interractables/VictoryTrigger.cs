using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    // --- ETAT ET DETECTION ---

    private bool hasWon = false;

    /// Déclenche l'écran de victoire lorsque le joueur atteint l'objectif.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasWon)
        {
            hasWon = true;

            int finalScore = 0;
            if (ScoreManager.Instance != null)
            {
                finalScore = ScoreManager.Instance.currentScore;
            }

            int collected = 0;
            int total = 50;
            float finalAccuracy = 98.2f;

            GameUIManager.Instance.ShowVictoryScreen(finalScore, collected, total, finalAccuracy);
        }
    }
}