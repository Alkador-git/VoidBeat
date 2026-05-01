using UnityEngine;

[ExecuteInEditMode]
public class BeatVisualizerTool : MonoBehaviour
{
    [System.Serializable]
    public struct ZoneTimelineEntry
    {
        public string label;
        public ZoneSettings settings;
        public float startTime;
        public float endTime;

        [HideInInspector] public float calculatedStartX;
        [HideInInspector] public float calculatedEndX;
    }

    [Header("Paramètres du Joueur")]
    public float moveSpeed = 10f;
    public float originalMusicBPM = 90f;

    [Header("Timeline du Niveau")]
    public float totalLevelDuration = 60f;
    public ZoneTimelineEntry[] timelineZones;

    [Header("Paramètres Visuels (Gizmos)")]
    public float lineLength = 12f;
    public Color beatColor = Color.green;
    public Color barColor = Color.cyan;
    public int beatsPerBar = 4;

    [Header("Visualisation des Zones")]
    public Color zoneBoundaryColor = Color.yellow;
    public float zoneHeight = 8f;

    private void OnValidate()
    {
        CalculateSpatialTimeline();
    }

    private void OnDrawGizmos()
    {
        if (moveSpeed <= 0 || originalMusicBPM <= 0) return;

        Vector3 basePos = transform.position;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(basePos + new Vector3(0, -lineLength / 2, 0), basePos + new Vector3(0, lineLength / 2, 0));

        CalculateSpatialTimeline();

        DrawZoneGizmos(basePos);

        DrawBeatGizmos(basePos);
    }

    private void CalculateSpatialTimeline()
    {
        if (timelineZones == null || timelineZones.Length == 0) return;

        float currentX = 0f;
        float currentTime = 0f;
        float timeStep = 0.01f;

        for (int i = 0; i < timelineZones.Length; i++)
        {
            while (currentTime < timelineZones[i].startTime)
            {
                float bpm = originalMusicBPM;
                float speed = moveSpeed * (bpm / originalMusicBPM);
                currentX += speed * timeStep;
                currentTime += timeStep;
            }

            timelineZones[i].calculatedStartX = currentX;

            while (currentTime < timelineZones[i].endTime)
            {
                float progress = Mathf.InverseLerp(timelineZones[i].startTime, timelineZones[i].endTime, currentTime);

                float bpm = (timelineZones[i].settings != null)
                    ? Mathf.Lerp(timelineZones[i].settings.bpmStart, timelineZones[i].settings.bpmEnd, progress)
                    : originalMusicBPM;

                float speed = moveSpeed * (bpm / originalMusicBPM);
                currentX += speed * timeStep;
                currentTime += timeStep;
            }

            timelineZones[i].calculatedEndX = currentX;
        }
    }

    private void DrawZoneGizmos(Vector3 basePos)
    {
        if (timelineZones == null) return;

        foreach (var zone in timelineZones)
        {
            if (zone.settings == null) continue;

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
            UnityEditor.Handles.Label(textPos, $"ZONE: {zone.settings.name}\n[{zone.startTime}s - {zone.endTime}s]");
#endif
        }
    }

    private void DrawBeatGizmos(Vector3 basePos)
    {
        float currentX = 0f;
        float currentTime = 0f;
        float timeStep = 0.01f;
        float beatIntervalCounter = 0f;
        int beatCount = 0;

        while (currentTime < totalLevelDuration)
        {
            float currentBPM = GetBPMAtTime(currentTime);
            float speed = moveSpeed * (currentBPM / originalMusicBPM);

            currentX += speed * timeStep;
            currentTime += timeStep;
            beatIntervalCounter += timeStep;

            float beatInterval = 60f / currentBPM;

            if (beatIntervalCounter >= beatInterval)
            {
                beatIntervalCounter -= beatInterval;
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
        if (timelineZones == null || timelineZones.Length == 0) return originalMusicBPM;

        foreach (var zone in timelineZones)
        {
            if (time >= zone.startTime && time <= zone.endTime)
            {
                if (zone.settings == null) return originalMusicBPM;

                float progress = Mathf.InverseLerp(zone.startTime, zone.endTime, time);
                return Mathf.Lerp(zone.settings.bpmStart, zone.settings.bpmEnd, progress);
            }
        }

        return originalMusicBPM;
    }
}