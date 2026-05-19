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

    /// Évalue la position de l'obstacle le long de sa spline avec barrière de sécurité NaN.
    private void Update()
    {
        if (isMoving && splineContainer != null && splineContainer.Splines.Count > 0)
        {
            timeElapsed += Time.deltaTime;

            float duration = movementDuration <= 0f ? 0.001f : movementDuration;
            float normalizedTime = Mathf.Clamp01(timeElapsed / duration);

            float curveValue = movementCurve.length > 0 ? movementCurve.Evaluate(normalizedTime) : normalizedTime;

            Vector3 localSplinePos = (Vector3)splineContainer.Splines[0].EvaluatePosition(curveValue);

            if (!float.IsNaN(localSplinePos.x) && !float.IsNaN(localSplinePos.y) && !float.IsNaN(localSplinePos.z))
            {
                Vector3 worldPos = splineContainer.transform.TransformPoint(localSplinePos);

                if (!float.IsNaN(worldPos.x) && !float.IsNaN(worldPos.y) && !float.IsNaN(worldPos.z))
                {
                    transform.position = worldPos;
                }
            }

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