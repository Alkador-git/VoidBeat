using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TempoIndicator : MonoBehaviour
{
    // --- VISUELS ---

    [Header("Réglages Visuels")]
    public RectTransform indicatorUI;
    public Text bpmText;
    public float pulseScale = 1.3f;

    [Header("Timings de l'Animation (En % du beat)")]
    [Range(0.05f, 0.5f)]
    public float shrinkDurationPercent = 0.2f;

    [Range(0.05f, 0.5f)]
    public float anticipationDurationPercent = 0.35f;

    // --- COULEURS ---

    [Header("Couleurs UI")]
    public Color beatColor = Color.green;
    public Color anticipateColor = Color.cyan;
    public Color normalColor = new Color(1, 1, 1, 0.5f);

    // --- GIZMOS ---

    [Header("Réglages Gizmos (Debug Tempo)")]
    public Transform player;
    public Color gizmoBeatColor = Color.magenta;
    public float lineLength = 12f;

    // --- COMPOSANTS INTERNES ---

    private Image img;
    private Vector3 originalScale;
    private float previousLastBeatTime;
    private List<float> recordedBeatPositions = new List<float>();

    void Start()
    {
        if (indicatorUI != null)
        {
            img = indicatorUI.GetComponent<Image>();
            originalScale = indicatorUI.localScale;
        }

        if (BeatManager.Instance != null)
        {
            previousLastBeatTime = BeatManager.Instance.GetLastBeatTime();
        }
    }

    void Update()
    {
        if (BeatManager.Instance == null || indicatorUI == null) return;

        if (bpmText != null)
        {
            bpmText.text = Mathf.RoundToInt(BeatManager.Instance.currentBPM).ToString() + " BPM";
        }

        float musicTimer = BeatManager.Instance.GetMusicTimer();
        float lastBeatTime = BeatManager.Instance.GetLastBeatTime();

        // --- DÉTECTION DU BEAT EXACT ---
        if (lastBeatTime != previousLastBeatTime)
        {
            previousLastBeatTime = lastBeatTime;

            if (player != null && Application.isPlaying)
            {
                recordedBeatPositions.Add(player.position.x);
                Debug.Log($"[Tempo Debug] Beat théorique posé à X: {player.position.x}");
            }
        }

        // --- CALCULS DE L'ANIMATION UI ---
        float currentBPM = BeatManager.Instance.currentBPM;
        float beatInterval = 60f / (currentBPM > 0 ? currentBPM : 120f);

        float beatPhase = (musicTimer - lastBeatTime) / beatInterval;
        beatPhase = Mathf.Clamp01(beatPhase);

        float anticipateThreshold = 1f - anticipationDurationPercent;

        if (beatPhase >= anticipateThreshold)
        {
            float progress = (beatPhase - anticipateThreshold) / anticipationDurationPercent;
            progress = Mathf.Clamp01(progress);

            indicatorUI.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, progress);
            if (img != null) img.color = Color.Lerp(normalColor, anticipateColor, progress);
        }
        else if (beatPhase <= shrinkDurationPercent)
        {
            float progress = beatPhase / shrinkDurationPercent;
            progress = Mathf.Clamp01(progress);

            indicatorUI.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, progress);
            if (img != null) img.color = Color.Lerp(beatColor, normalColor, progress);
        }
        else
        {
            indicatorUI.localScale = originalScale;
            if (img != null) img.color = normalColor;
        }
    }

    // --- DESSIN DES GIZMOS DANS L'ÉDITEUR ---
    private void OnDrawGizmos()
    {
        if (recordedBeatPositions == null || recordedBeatPositions.Count == 0) return;

        Gizmos.color = gizmoBeatColor;

        foreach (float xPos in recordedBeatPositions)
        {
            float baselineY = (player != null) ? player.position.y : transform.position.y;

            Vector3 bottom = new Vector3(xPos, baselineY - (lineLength / 2f), 0f);
            Vector3 top = new Vector3(xPos, baselineY + (lineLength / 2f), 0f);

            Gizmos.DrawLine(bottom, top);
        }
    }

    [ContextMenu("Effacer les Gizmos de Beat")]
    public void ClearGizmos()
    {
        recordedBeatPositions.Clear();
        Debug.Log("[TempoIndicator] Historique des Gizmos effacé");
    }
}