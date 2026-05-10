using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    // --- CONFIGURATION ---

    public ZoneSettings settings;

    // --- COMPONENTS ---

    private BoxCollider2D col;

    // --- INITIALIZATION ---

    void Awake() => col = GetComponent<BoxCollider2D>();

    // --- DETECTION ---

    /// Detects when the player enters a new zone.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BeatManager.Instance.EnterNewZone(settings);
        }
    }

    /// Updates the player's progression within the zone.
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