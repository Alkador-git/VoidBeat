using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BeatVisualizerTool : MonoBehaviour
{
    [System.Serializable]
    public struct ZoneTimelineEntry
    {
        public string label;
        public float startTime;
        public float endTime;
        public float bpmStart;
        public float bpmEnd;
        [HideInInspector] public float calculatedStartX;
        [HideInInspector] public float calculatedEndX;
    }

    [Header("Données Enregistrées (Mode Jeu)")]
    public BeatData dataContainer;

    [Header("Paramètres du Joueur")]
    public float moveSpeed = 3.5f;

    [Header("Correction Visuelle")]
    public float spatialOffset = 0f;

    [Header("Timeline du Niveau")]
    public float totalLevelDuration = 122.5f;
    public ZoneTimelineEntry[] timelineZones;

    [Header("Paramètres Visuels (Beats)")]
    public float lineLength = 12f;
    public Color beatColor = Color.green;
    public Color barColor = Color.cyan;
    public int beatsPerBar = 4;

    [Header("Paramètres Visuels (Zones & Fin)")]
    public Color zoneBoundaryColor = Color.yellow;
    public Color levelEndColor = Color.red;
    public float zoneHeight = 8f;

    private void OnValidate()
    {
        CalculateTimelinePositions();
    }

    private void OnDrawGizmos()
    {
        if (moveSpeed <= 0) return;
        Vector3 basePos = transform.position;

        DrawLevelBoundaries(basePos);
        DrawZoneGizmos(basePos);

        DrawRecordedBeats(basePos);
    }

    private void CalculateTimelinePositions()
    {
        if (timelineZones == null) return;
        for (int i = 0; i < timelineZones.Length; i++)
        {
            timelineZones[i].calculatedStartX = (timelineZones[i].startTime * moveSpeed);
            timelineZones[i].calculatedEndX = (timelineZones[i].endTime * moveSpeed);
        }
    }

    private void DrawLevelBoundaries(Vector3 basePos)
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(basePos + new Vector3(spatialOffset, -lineLength, 0), basePos + new Vector3(spatialOffset, lineLength, 0));

        Gizmos.color = levelEndColor;
        float endX = (totalLevelDuration * moveSpeed) + spatialOffset;
        Vector3 endBot = basePos + new Vector3(endX, -lineLength, 0);
        Vector3 endTop = basePos + new Vector3(endX, lineLength, 0);
        Gizmos.DrawLine(endBot, endTop);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(endTop + Vector3.up, $"FIN DU NIVEAU ({totalLevelDuration}s)");
#endif
    }

    private void DrawZoneGizmos(Vector3 basePos)
    {
        if (timelineZones == null) return;

        foreach (var zone in timelineZones)
        {
            Gizmos.color = zoneBoundaryColor;
            float startX = zone.calculatedStartX + spatialOffset;
            float endX = zone.calculatedEndX + spatialOffset;

            Vector3 sBot = basePos + new Vector3(startX, -zoneHeight / 2f, 0);
            Vector3 sTop = basePos + new Vector3(startX, zoneHeight / 2f, 0);
            Vector3 eBot = basePos + new Vector3(endX, -zoneHeight / 2f, 0);
            Vector3 eTop = basePos + new Vector3(endX, zoneHeight / 2f, 0);

            Gizmos.DrawLine(sBot, sTop);
            Gizmos.DrawLine(eBot, eTop);
            Gizmos.DrawLine(sTop, eTop);
            Gizmos.DrawLine(sBot, eBot);

#if UNITY_EDITOR
            Vector3 labelPos = basePos + new Vector3((startX + endX) / 2f, zoneHeight / 2f + 0.5f, 0);
            string info = $"ZONE: {zone.label}\n{zone.bpmStart} -> {zone.bpmEnd} BPM";
            UnityEditor.Handles.Label(labelPos, info);
#endif
        }
    }

    private void DrawRecordedBeats(Vector3 basePos)
    {
        if (dataContainer == null || dataContainer.recordedBeats == null) return;

        for (int i = 0; i < dataContainer.recordedBeats.Count; i++)
        {
            bool isNewBar = (i % beatsPerBar == 0);
            Gizmos.color = isNewBar ? barColor : beatColor;
            float currentLineHeight = isNewBar ? lineLength : lineLength * 0.7f;

            float xPos = dataContainer.recordedBeats[i] + spatialOffset;
            Vector3 bottom = basePos + new Vector3(xPos, -currentLineHeight / 2, 0);
            Vector3 top = basePos + new Vector3(xPos, currentLineHeight / 2, 0);
            Gizmos.DrawLine(bottom, top);
        }
    }

    [ContextMenu("Effacer les données d'enregistrement")]
    public void ClearData()
    {
        if (dataContainer != null) dataContainer.recordedBeats.Clear();
    }
}