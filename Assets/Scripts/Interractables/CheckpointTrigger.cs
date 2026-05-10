using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    // --- STATE ---

    private bool isActivated = false;

    // --- VISUALS ---

    public Color activeColor = Color.cyan;
    private SpriteRenderer sr;

    // --- INITIALIZATION ---

    void Start() => sr = GetComponent<SpriteRenderer>();

    // --- COLLISION DETECTION ---

    /// Activates checkpoint when player enters the zone.
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