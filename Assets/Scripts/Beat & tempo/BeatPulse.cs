using System.Collections;
using UnityEngine;

public class BeatPulse : MonoBehaviour
{
    public enum BeatInterval { Every1Beat = 1, Every2Beats = 2, Every4Beats = 4, Every8Beats = 8 }

    [Header("Configuration du Rythme")]
    [SerializeField] private BeatInterval pulseInterval = BeatInterval.Every1Beat;

    [Header("Réglages de l'Animation")]
    [SerializeField] private float pulseSize = 1.15f;
    [SerializeField][Range(0.01f, 0.5f)] private float anticipationPercent = 0.1f;
    [SerializeField][Range(0.01f, 0.5f)] private float shrinkPercent = 0.1f;

    [Header("Isolation Visuelle")]
    [SerializeField] private Transform visualTarget;

    private Vector3 _startSize;

    private void Start()
    {
        if (visualTarget == null)
        {
            SpriteRenderer childSR = GetComponentInChildren<SpriteRenderer>();
            if (childSR != null && childSR.transform != transform)
            {
                visualTarget = childSR.transform;
            }
            else
            {
                visualTarget = transform;
                Debug.LogWarning($"[BeatPulse] Aucun visualTarget assigné sur '{name}'");
            }
        }

        _startSize = visualTarget.localScale;
    }

    private void Update()
    {
        if (BeatManager.Instance == null || visualTarget == null) return;

        float beatPhase = BeatManager.Instance.GetDataDrivenBeatPhase();

        int nextBeatNumber = BeatManager.Instance.GetNextBeatIndex();

        if (nextBeatNumber < 0)
        {
            visualTarget.localScale = _startSize;
            return;
        }

        int interval = (int)pulseInterval;
        float anticipateThreshold = 1f - anticipationPercent;

        if (beatPhase >= anticipateThreshold)
        {
            if (nextBeatNumber % interval == 0)
            {
                float progress = (beatPhase - anticipateThreshold) / anticipationPercent;
                progress = Mathf.Clamp01(progress);

                visualTarget.localScale = Vector3.Lerp(_startSize, _startSize * pulseSize, progress);
                return;
            }
        }
        else if (beatPhase <= shrinkPercent)
        {
            int passedBeatNumber = nextBeatNumber - 1;
            if (passedBeatNumber % interval == 0)
            {
                float progress = beatPhase / shrinkPercent;
                progress = Mathf.Clamp01(progress);

                visualTarget.localScale = Vector3.Lerp(_startSize * pulseSize, _startSize, progress);
                return;
            }
        }

        visualTarget.localScale = _startSize;
    }

    public void pulse()
    {
        if (visualTarget != null)
        {
            visualTarget.localScale = _startSize * pulseSize;
        }
    }
}