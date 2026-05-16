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
#if UNITY_EDITOR
        SaveData();
#endif
    }

    // --- NOUVEAU : FORCER LA SAUVEGARDE SUR LE DISQUE ---
    [ContextMenu("Sauvegarder les Données sur le Disque")]
    public void SaveData()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);

        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log($"Sauvegarde forcée réussie ! {recordedBeats.Count} beats ont été écrits dans le fichier {name}.asset");
#endif
    }
}