using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    private bool isActivated = false;
    public Color activeColor = Color.cyan;
    private SpriteRenderer sr;

    void Start() => sr = GetComponent<SpriteRenderer>();

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