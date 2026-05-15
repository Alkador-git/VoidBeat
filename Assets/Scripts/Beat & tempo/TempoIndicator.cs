using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TempoIndicator : MonoBehaviour
{
    // --- VISUALS ---

    public RectTransform indicatorUI;
    public Text bpmText;
    public float pulseScale = 1.3f;

    // --- ANIMATION TIMING ---

    [Range(0.05f, 0.5f)]
    public float shrinkDurationPercent = 0.2f;

    [Range(0.05f, 0.5f)]
    public float anticipationDurationPercent = 0.35f;

    // --- COLORS ---

    public Color beatColor = Color.green;
    public Color anticipateColor = Color.cyan;
    public Color normalColor = new Color(1, 1, 1, 0.5f);

    // --- GIZMOS ---

    public Transform player;
    public Color gizmoBeatColor = Color.magenta;
    public float lineLength = 12f;

    // --- INTERNAL COMPONENTS ---

    private Image img;
    private Vector3 originalScale;

    // --- INITIALIZATION ---

    void Start()
    {
        if (indicatorUI != null)
        {
            img = indicatorUI.GetComponent<Image>();
            originalScale = indicatorUI.localScale;
        }
    }

    // --- UPDATE LOOP ---

    void Update()
    {
        if (BeatManager.Instance == null || indicatorUI == null || BeatManager.Instance.dataContainer == null) return;

        if (bpmText != null)
        {
            bpmText.text = Mathf.RoundToInt(BeatManager.Instance.currentBPM).ToString() + " BPM";
        }

        float musicTimer = BeatManager.Instance.GetMusicTimer();
        List<BeatPoint> beats = BeatManager.Instance.dataContainer.recordedBeats;

        if (beats.Count < 2) return;

        // --- RECHERCHE DU BEAT ACTUEL ET SUIVANT ---

        int nextBeatIndex = -1;

        for (int i = 0; i < beats.Count; i++)
        {
            if (beats[i].musicTime > musicTimer)
            {
                nextBeatIndex = i;
                break;
            }
        }

        if (nextBeatIndex <= 0)
        {
            ResetVisuals();
            return;
        }

        BeatPoint prevBeat = beats[nextBeatIndex - 1];
        BeatPoint nextBeat = beats[nextBeatIndex];

        // --- CALCUL DE LA PHASE ---

        float beatDuration = nextBeat.musicTime - prevBeat.musicTime;
        float beatPhase = (musicTimer - prevBeat.musicTime) / beatDuration;
        beatPhase = Mathf.Clamp01(beatPhase);

        // --- LOGIQUE D'ANIMATION ---

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
            ResetVisuals();
        }
    }

    private void ResetVisuals()
    {
        indicatorUI.localScale = originalScale;
        if (img != null) img.color = normalColor;
    }

    // --- GIZMO VISUALIZATION ---

    private void OnDrawGizmos()
    {
        if (BeatManager.Instance == null || BeatManager.Instance.dataContainer == null) return;

        List<BeatPoint> recordedBeats = BeatManager.Instance.dataContainer.recordedBeats;
        if (recordedBeats == null || recordedBeats.Count == 0) return;

        Gizmos.color = gizmoBeatColor;

        foreach (BeatPoint beat in recordedBeats)
        {
            float baselineY = (player != null) ? player.position.y : transform.position.y;

            Vector3 bottom = new Vector3(beat.xPos, baselineY - (lineLength / 2f), 0f);
            Vector3 top = new Vector3(beat.xPos, baselineY + (lineLength / 2f), 0f);

            Gizmos.DrawLine(bottom, top);
        }
    }

    [ContextMenu("Effacer les Données de Beat")]
    public void ClearData()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.dataContainer != null)
        {
            BeatManager.Instance.dataContainer.recordedBeats.Clear();
        }
    }
}