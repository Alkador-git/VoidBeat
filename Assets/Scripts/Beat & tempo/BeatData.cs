using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct BeatPoint
{
    public float xPos;
    public float musicTime;
}

[CreateAssetMenu(fileName = "NewBeatData", menuName = "VoidBeat/BeatData")]
public class BeatData : ScriptableObject
{
    // --- BEAT RECORDING ---

    public List<BeatPoint> recordedBeats = new List<BeatPoint>();

    public void ClearData()
    {
        recordedBeats.Clear();
    }
}