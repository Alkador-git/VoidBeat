using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBeatData", menuName = "VoidBeat/BeatData")]
public class BeatData : ScriptableObject
{
    public List<float> recordedBeats = new List<float>();
}