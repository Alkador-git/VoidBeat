using UnityEngine;

public class GhostTrigger : MonoBehaviour
{
    [Header("Référence")]
    public GhostReplayer replayer;

    [Header("Options")]
    public bool onlyOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (onlyOnce && hasTriggered) return;

            if (replayer != null)
            {
                replayer.StartDemo();
                hasTriggered = true;

                Debug.Log("Démarrage de la zone de démo.");
            }
        }
    }
}