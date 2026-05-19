using UnityEngine;

public class ParkourObstacleTrigger : MonoBehaviour
{
    [Header("Cible à Activer")]
    public ParkourObstacle targetObstacle;

    private bool hasTriggered = false;

    // --- CONFIGURATION DU DECLENCHEUR ---

    /// Déclenche l'animation de l'obstacle lié au passage du joueur.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (targetObstacle != null)
            {
                targetObstacle.StartObstacleMovement();
                hasTriggered = true;
            }
        }
    }

    /// Restitue l'état d'activation d'origine du déclencheur de spline.
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}