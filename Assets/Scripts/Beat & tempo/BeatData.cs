using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBeatData", menuName = "VoidBeat/BeatData")]
public class BeatData : ScriptableObject
{
    // --- BEAT RECORDING ---

    /// List of recorded beat positions during gameplay.
    public List<float> recordedBeats = new List<float>();
}