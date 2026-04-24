using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    // --- ÉTAT ---

    private bool isActivated = false;

    // --- VISUELS ---

    public Color activeColor = Color.cyan;
    private SpriteRenderer sr;

    // --- INITIALISATION ---

    /// Récupère le composant SpriteRenderer
    void Start() => sr = GetComponent<SpriteRenderer>();

    // --- DÉTECTION ---

    /// Gère l'entrée du joueur dans la zone de checkpoint
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            CheckpointManager.Instance.SetCheckpoint(transform.position);

            if (sr != null) sr.color = activeColor;
        }
    }
}