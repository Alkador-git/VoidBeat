using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class ParkourObstacle : MonoBehaviour
{
    [Header("Réglages")]
    public float boostPenalty = 5f;

    [Header("Effets de Collision")]
    public float shakeMagnitude = 0.25f;
    public float shakeDuration = 0.35f;

    [Header("Mouvement par Spline (Optionnel)")]
    public SplineContainer splineContainer;
    public float movementDuration = 2f;
    public AnimationCurve movementCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private bool isMoving = false;
    private float timeElapsed = 0f;

    // --- MOUVEMENT ET PHYSIQUE ---

    /// Évalue la position de l'obstacle le long de sa spline de déplacement.
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

    /// Active l'état de déplacement linéaire de l'élément de décor.
    public void StartObstacleMovement()
    {
        if (!isMoving && splineContainer != null)
        {
            isMoving = true;
            timeElapsed = 0f;
        }
    }

    /// Inspecte les volumes d'entrée pour intercepter le contact avec le joueur.
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

    /// Transmet les modifications de ressources et de points négatifs aux gestionnaires.
    private void ApplyPenalty(KZ0Controller controller)
    {
        if (BoostManager.Instance != null)
            BoostManager.Instance.RemoveBoost(boostPenalty);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.DeductObstacleCollisionScore();

        if (CinemachineShake.Instance != null)
            StartCoroutine(CinemachineShake.Instance.Shake(shakeDuration, shakeMagnitude, 10.0f));
    }
}