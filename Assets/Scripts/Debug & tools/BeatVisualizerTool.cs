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

    [Header("Correction de Dérive")]
    public float speedMultiplier = 1.0f;
    public float spatialOffset = 0f;

    [Header("Timeline du Niveau")]
    public float totalLevelDuration = 122.5f;
    public ZoneTimelineEntry[] timelineZones;

    [Header("Paramètres Visuels")]
    public float lineLength = 12f;
    public Color beatColor = Color.green;
    public Color barColor = Color.cyan;
    public int beatsPerBar = 4;

    private List<ZoneTimelineEntry> sortedZones = new List<ZoneTimelineEntry>();
    private void OnValidate() { SortAndCalculateSpatialTimeline(); }

    private void OnDrawGizmos()
    {
        if (moveSpeed <= 0) return;
        Vector3 basePos = transform.position;

        SortAndCalculateSpatialTimeline();
        DrawZoneGizmos(basePos);
        DrawBeatGizmos(basePos);
    }

    public float GetXAtTime(float time)
    {
        return time * (moveSpeed * speedMultiplier) + spatialOffset;
    }

    private void SortAndCalculateSpatialTimeline()
    {
        if (timelineZones == null || timelineZones.Length == 0) return;
        sortedZones.Clear();
        sortedZones.AddRange(timelineZones);
        sortedZones.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        for (int i = 0; i < sortedZones.Count; i++)
        {
            ZoneTimelineEntry entry = sortedZones[i];
            entry.calculatedStartX = GetXAtTime(entry.startTime);
            entry.calculatedEndX = GetXAtTime(entry.endTime);

            for (int j = 0; j < timelineZones.Length; j++)
            {
                if (timelineZones[j].label == entry.label)
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
        foreach (var zone in timelineZones)
        {
            Gizmos.color = Color.yellow;
            Vector3 sBot = basePos + new Vector3(zone.calculatedStartX, -4f, 0);
            Vector3 sTop = basePos + new Vector3(zone.calculatedStartX, 4f, 0);
            Gizmos.DrawLine(sBot, sTop);
            Gizmos.DrawLine(basePos + new Vector3(zone.calculatedEndX, -4f, 0), basePos + new Vector3(zone.calculatedEndX, 4f, 0));
        }
    }

    private void DrawBeatGizmos(Vector3 basePos)
    {
        float currentTime = 0f;
        int beatCount = 0;

        while (currentTime < totalLevelDuration)
        {
            float currentBPM = GetBPMAtTime(currentTime);
            if (currentBPM <= 0) break;

            float beatDuration = 60f / currentBPM;

            float finalX = GetXAtTime(currentTime);

            beatCount++;
            bool isNewBar = (beatCount % beatsPerBar == 1);
            Gizmos.color = isNewBar ? barColor : beatColor;

            Vector3 bottom = basePos + new Vector3(finalX, -lineLength / 2, 0);
            Vector3 top = basePos + new Vector3(finalX, lineLength / 2, 0);
            Gizmos.DrawLine(bottom, top);
            currentTime += beatDuration;
        }
    }

    private float GetBPMAtTime(float time)
    {
        foreach (var zone in sortedZones)
        {
            if (time >= zone.startTime && time <= zone.endTime)
            {
                float progress = Mathf.InverseLerp(zone.startTime, zone.endTime, time);
                return Mathf.Lerp(zone.bpmStart, zone.bpmEnd, progress);
            }
        }
        return (sortedZones.Count > 0) ? sortedZones[0].bpmStart : 120f;
    }
}