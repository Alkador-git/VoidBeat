using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct GhostFrame
{
    public float time;
    public Vector3 position;
    public string animBoolName;
    public bool animValue;
}

[CreateAssetMenu(fileName = "NewGhostData", menuName = "VoidBeat/GhostData")]
public class GhostData : ScriptableObject
{
    public List<GhostFrame> frames = new List<GhostFrame>();
}