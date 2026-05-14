using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct GhostFrame
{
    public float time;
    public Vector3 position;
    public int animatorStateHash;
    public float normalizedTime;
}

[CreateAssetMenu(fileName = "NewGhostData", menuName = "VoidBeat/GhostData")]
public class GhostData : ScriptableObject
{
    public List<GhostFrame> frames = new List<GhostFrame>();
}