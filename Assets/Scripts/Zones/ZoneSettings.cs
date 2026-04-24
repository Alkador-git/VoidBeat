using UnityEngine;

[CreateAssetMenu(fileName = "NewZone", menuName = "VoidBeat/Zone Settings")]
public class ZoneSettings : ScriptableObject
{
    // --- TYPE ---

    public BeatManager.ZoneType zoneType;

    // --- RYTHME ---

    [Header("Rythme")]
    public float bpmStart = 90f;
    public float bpmEnd = 120f;

    // --- PARAMÈTRES SPÉCIAUX ---

    [Header("Paramètres Spéciaux")]
    public bool useLowPass = false;
    public float timeScale = 1.0f;
}