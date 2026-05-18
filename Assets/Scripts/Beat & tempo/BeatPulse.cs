using System.Collections;
using UnityEngine;

public class BeatPulse : MonoBehaviour
{
    public enum BeatInterval { Every1Beat = 1, Every2Beats = 2, Every4Beats = 4, Every8Beats = 8 }

    [Header("Configuration du Rythme")]
    [SerializeField] private BeatInterval pulseInterval = BeatInterval.Every1Beat;

    [Header("Réglages de l'Animation")]
    [SerializeField] private float pulseSize = 1.15f;
    [SerializeField] private float _returnSpeed = 5f;

    [Header("Isolation Visuelle (Anti-Collider Bug)")]
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
                Debug.LogWarning($"Aucun visualTarget assigné sur '{name}'");
            }
        }

        _startSize = visualTarget.localScale;

        SubscribeToBeatEvent();
    }

    private void OnDisable()
    {
        UnsubscribeFromBeatEvent();
    }

    private void OnDestroy()
    {
        UnsubscribeFromBeatEvent();
    }

    private void Update()
    {
        if (visualTarget != null)
        {
            visualTarget.localScale = Vector3.Lerp(visualTarget.localScale, _startSize, Time.deltaTime * _returnSpeed);
        }
    }

    public void pulse()
    {
        if (visualTarget != null)
        {
            visualTarget.localScale = _startSize * pulseSize;
        }
    }

    private void SubscribeToBeatEvent()
    {
        if (BeatManager.Instance == null) return;

        switch (pulseInterval)
        {
            case BeatInterval.Every1Beat: BeatManager.OnBeat1 += pulse; break;
            case BeatInterval.Every2Beats: BeatManager.OnBeat2 += pulse; break;
            case BeatInterval.Every4Beats: BeatManager.OnBeat4 += pulse; break;
            case BeatInterval.Every8Beats: BeatManager.OnBeat8 += pulse; break;
        }
    }

    private void UnsubscribeFromBeatEvent()
    {
        switch (pulseInterval)
        {
            case BeatInterval.Every1Beat: BeatManager.OnBeat1 -= pulse; break;
            case BeatInterval.Every2Beats: BeatManager.OnBeat2 -= pulse; break;
            case BeatInterval.Every4Beats: BeatManager.OnBeat4 -= pulse; break;
            case BeatInterval.Every8Beats: BeatManager.OnBeat8 -= pulse; break;
        }
    }
}