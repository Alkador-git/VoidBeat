using UnityEngine;

public class DebugTeleport : MonoBehaviour
{
    // --- CONFIGURATION ---

    [Header("Références")]
    public GameObject player;

    public Transform checkpointsContainer;

    // --- STATE ---

    private bool resetVelocity = true;

    // --- UPDATE LOOP ---

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    TeleportToChild(i);
                }
            }
        }
    }

    // --- TELEPORTATION ---

    /// Teleports player to specified checkpoint child.
    private void TeleportToChild(int index)
    {
        if (checkpointsContainer == null || player == null) return;

        if (index < checkpointsContainer.childCount)
        {
            Transform targetCheckpoint = checkpointsContainer.GetChild(index);

            player.transform.position = targetCheckpoint.position;

            if (resetVelocity && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
            }

            BlackHoleManager bhManager = FindFirstObjectByType<BlackHoleManager>();
            if (bhManager != null)
            {
                bhManager.SnapToPosition();
            }

            Debug.Log($"Debug : Téléportation réussie. Néant-X synchronisé");
        }
    }
}