using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    // --- RÉGLAGES ---

    public ZoneSettings settings;

    // --- COMPOSANTS ---

    private BoxCollider2D col;

    // --- INITIALISATION ---

    /// Récupère le composant BoxCollider2D
    void Awake() => col = GetComponent<BoxCollider2D>();

    // --- DÉTECTION ---

    /// Détecte quand le joueur entre dans une nouvelle zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BeatManager.Instance.EnterNewZone(settings);
        }
    }

    /// Met à jour la progression du joueur dans la zone
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float leftEdge = col.bounds.min.x;
            float width = col.bounds.size.x;
            float currentX = other.transform.position.x;

            float progress = Mathf.Clamp01((currentX - leftEdge) / width);

            BeatManager.Instance.UpdateZoneProgress(progress);
        }
    }
}