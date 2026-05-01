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

    [Header("Paramètres du Joueur")]
    public float moveSpeed = 10f;

    [Header("Timeline du Niveau")]
    public float totalLevelDuration = 122.5f;
    public ZoneTimelineEntry[] timelineZones;

    [Header("Paramètres Visuels (Gizmos)")]
    public float lineLength = 12f;
    public Color beatColor = Color.green;
    public Color barColor = Color.cyan;
    public int beatsPerBar = 4;

    [Header("Visualisation des Zones")]
    public Color zoneBoundaryColor = Color.yellow;
    public float zoneHeight = 8f;

    private List<ZoneTimelineEntry> sortedZones = new List<ZoneTimelineEntry>();

    private void OnValidate()
    {
        SortAndCalculateSpatialTimeline();
    }

    private void OnDrawGizmos()
    {
        if (moveSpeed <= 0) return;

        Vector3 basePos = transform.position;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(basePos + new Vector3(0, -lineLength / 2, 0), basePos + new Vector3(0, lineLength / 2, 0));

        SortAndCalculateSpatialTimeline();

        DrawZoneGizmos(basePos);

        DrawBeatGizmos(basePos);
    }

    private void SortAndCalculateSpatialTimeline()
    {
        if (timelineZones == null || timelineZones.Length == 0) return;

        sortedZones.Clear();
        sortedZones.AddRange(timelineZones);
        sortedZones.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        float currentX = 0f;
        float currentTime = 0f;
        float timeStep = 0.005f;

        for (int i = 0; i < sortedZones.Count; i++)
        {
            ZoneTimelineEntry entry = sortedZones[i];

            while (currentTime < entry.startTime)
            {
                currentX += moveSpeed * timeStep;
                currentTime += timeStep;
            }

            entry.calculatedStartX = currentX;

            while (currentTime < entry.endTime)
            {
                currentX += moveSpeed * timeStep;
                currentTime += timeStep;
            }

            entry.calculatedEndX = currentX;

            for (int j = 0; j < timelineZones.Length; j++)
            {
                if (timelineZones[j].label == entry.label && timelineZones[j].startTime == entry.startTime)
                {
                    timelineZones[j].calculatedStartX = entry.calculatedStartX;
                    timelineZones[j].calculatedEndX = entry.calculatedEndX;
                }
            }

            sortedZones[i] = entry;
        }
    }

    private void DrawZoneGizmos(Vector3 basePos)
    {
        if (timelineZones == null) return;

        foreach (var zone in timelineZones)
        {
            Gizmos.color = zoneBoundaryColor;

            Vector3 startTop = basePos + new Vector3(zone.calculatedStartX, zoneHeight / 2, 0);
            Vector3 startBottom = basePos + new Vector3(zone.calculatedStartX, -zoneHeight / 2, 0);
            Gizmos.DrawLine(startBottom, startTop);

            Vector3 endTop = basePos + new Vector3(zone.calculatedEndX, zoneHeight / 2, 0);
            Vector3 endBottom = basePos + new Vector3(zone.calculatedEndX, -zoneHeight / 2, 0);
            Gizmos.DrawLine(endBottom, endTop);

            Gizmos.DrawLine(startTop, endTop);
            Gizmos.DrawLine(startBottom, endBottom);

#if UNITY_EDITOR
            Vector3 textPos = basePos + new Vector3((zone.calculatedStartX + zone.calculatedEndX) / 2, zoneHeight / 2 + 1f, 0);
            UnityEditor.Handles.Label(textPos, $"ZONE: {zone.label}\n[{zone.startTime}s - {zone.endTime}s]\nBPM: {zone.bpmStart} -> {zone.bpmEnd}");
#endif
        }
    }

    private void DrawBeatGizmos(Vector3 basePos)
    {
        float currentX = 0f;
        float currentTime = 0f;
        float timeStep = 0.005f;

        float lastBeatTime = 0f;
        int beatCount = 0;

        while (currentTime < totalLevelDuration)
        {
            float currentBPM = GetBPMAtTime(currentTime);
            if (currentBPM <= 0) break;

            float beatInterval = 60f / currentBPM;

            currentX += moveSpeed * timeStep;
            currentTime += timeStep;

            if (currentTime >= lastBeatTime + beatInterval)
            {
                lastBeatTime += beatInterval;
                beatCount++;

                bool isNewBar = (beatCount % beatsPerBar == 1);
                Gizmos.color = isNewBar ? barColor : beatColor;

                Vector3 bottom = basePos + new Vector3(currentX, -lineLength / 2, 0);
                Vector3 top = basePos + new Vector3(currentX, lineLength / 2, 0);
                Gizmos.DrawLine(bottom, top);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(top + Vector3.up * 0.5f, $"B:{beatCount}");
#endif
            }
        }
    }

    private float GetBPMAtTime(float time)
    {
        if (sortedZones == null || sortedZones.Count == 0) return 120f;

        foreach (var zone in sortedZones)
        {
            if (time >= zone.startTime && time <= zone.endTime)
            {
                float progress = Mathf.InverseLerp(zone.startTime, zone.endTime, time);
                return Mathf.Lerp(zone.bpmStart, zone.bpmEnd, progress);
            }
        }

        if (time > sortedZones[sortedZones.Count - 1].endTime)
        {
            return sortedZones[sortedZones.Count - 1].bpmEnd;
        }

        return sortedZones[0].bpmStart;
    }
}