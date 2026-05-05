using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    private bool hasWon = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasWon)
        {
            hasWon = true;

            int finalScore = 0;
            int collected = 0;
            int total = 50;

            float finalAccuracy = 98.2f;

            GameUIManager.Instance.ShowVictoryScreen(finalScore, collected, total, finalAccuracy);
        }
    }
}