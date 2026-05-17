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
    // --- DATA CONTAINER ---
    public List<GhostFrame> frames = new List<GhostFrame>();

    public void ClearData()
    {
        frames.Clear();
#if UNITY_EDITOR
        SaveData();
#endif
    }

    public void SaveData()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);

        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log("Trajectoire '{name}' sauvegardée");
#endif
    }
}