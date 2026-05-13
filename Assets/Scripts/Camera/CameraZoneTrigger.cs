using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Réglages Zone Lore")]
    public Vector3 zoneOffset = new Vector3(2.5f, 5f, -10f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CinemachineCameraController.Instance?.SetOffset(zoneOffset);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CinemachineCameraController.Instance?.ResetOffset();
        }
    }
}