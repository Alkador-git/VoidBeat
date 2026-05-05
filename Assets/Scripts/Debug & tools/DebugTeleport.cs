using UnityEngine;

public class DebugTeleport : MonoBehaviour
{
    [Header("Références")]
    public GameObject player;
    public Transform checkpointsContainer;

    private bool resetVelocity = true;

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

    private void TeleportToChild(int index)
    {
        if (checkpointsContainer == null || player == null) return;

        if (index < checkpointsContainer.childCount)
        {
            Transform targetCheckpoint = checkpointsContainer.GetChild(index);

            // 1. Téléportation du joueur
            player.transform.position = targetCheckpoint.position;

            // 2. Reset de la vélocité
            if (resetVelocity && player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
            }

            // 3. SYNCHRONISATION DU TROU NOIR
            BlackHoleManager bhManager = FindFirstObjectByType<BlackHoleManager>();
            if (bhManager != null)
            {
                bhManager.SnapToPosition();
            }

            Debug.Log($"Debug : Téléportation réussie. Néant-X synchronisé");
        }
    }
}