using UnityEngine;
using Unity.Cinemachine;

public class CinemachineCameraController : MonoBehaviour
{
    public static CinemachineCameraController Instance;

    private CinemachineCamera vCam;
    private CinemachineFollow followComponent;

    [Header("Paramètres de Suivi (GDD)")]
    public Vector3 defaultOffset = new Vector3(2.5f, 2f, -10f);
    public float smoothSpeed = 5f;

    private Vector3 targetOffset;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        vCam = GetComponent<CinemachineCamera>();

        followComponent = GetComponent<CinemachineFollow>();

        if (followComponent != null)
        {
            followComponent.FollowOffset = defaultOffset;
        }
        else
        {
            Debug.LogError("Aucun composant Cinemachine Follow trouvé sur cette caméra !");
        }

        targetOffset = defaultOffset;
    }

    void Update()
    {
        if (followComponent == null) return;

        followComponent.FollowOffset = Vector3.Lerp(
            followComponent.FollowOffset,
            targetOffset,
            Time.deltaTime * smoothSpeed
        );
    }

    public void SetOffset(Vector3 newOffset)
    {
        targetOffset = newOffset;
    }

    public void ResetOffset()
    {
        targetOffset = defaultOffset;
    }
}