using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class ParkourObstacle : MonoBehaviour
{
    // --- CONFIGURATION DE BASE ---

    [Header("Réglages")]
    public float boostPenalty = 5f;

    // --- EFFETS DE COLLISION ---

    [Header("Effets de Collision")]
    public float shakeMagnitude = 0.25f;
    public float shakeDuration = 0.35f;

    // --- DÉPLACEMENT VIA SPLINE ---

    [Header("Mouvement par Spline (Optionnel)")]
    public SplineContainer splineContainer;

    public float movementDuration = 2f;

    public AnimationCurve movementCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private bool isMoving = false;
    private float timeElapsed = 0f;

    // --- BOUCLE DE MISE À JOUR ---

    private void Update()
    {
        if (isMoving && splineContainer != null)
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timeElapsed / movementDuration);

            float curveValue = movementCurve.Evaluate(normalizedTime);

            Vector3 localSplinePos = splineContainer.EvaluatePosition(curveValue);
            transform.position = splineContainer.transform.TransformPoint(localSplinePos);

            if (normalizedTime >= 1f)
            {
                isMoving = false;
            }
        }
    }

    // --- NOUVEAU : DÉCLENCHEUR VISÉ VIA INSPECTOR ---

    public void StartObstacleMovement()
    {
        if (!isMoving && splineContainer != null)
        {
            isMoving = true;
            timeElapsed = 0f;
        }
    }

    // --- COLLISION DETECTION ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<KZ0Controller>(out KZ0Controller controller))
            {
                ApplyPenalty(controller);
            }
        }
    }

    private void ApplyPenalty(KZ0Controller controller)
    {
        if (BoostManager.Instance != null)
            BoostManager.Instance.RemoveBoost(boostPenalty);

        if (CinemachineShake.Instance != null)
            StartCoroutine(CinemachineShake.Instance.Shake(shakeDuration, shakeMagnitude, 10.0f));
    }
}

// --- DÉCLENCHEMENT ---

public class ParkourObstacleTrigger : MonoBehaviour
{
    [Header("Cible à Activer")]
    public ParkourObstacle targetObstacle;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (targetObstacle != null)
            {
                targetObstacle.StartObstacleMovement();
                hasTriggered = true;
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}