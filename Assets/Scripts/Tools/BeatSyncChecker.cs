using UnityEngine;
using System.Collections.Generic;

public class BeatSyncChecker : MonoBehaviour
{
    [Header("Références")]
    public Transform player;

    [Header("Paramètres Visuels")]
    public float lineLength = 12f;
    public Color recordedBeatColor = Color.red;

    private List<float> recordedXPositions = new List<float>();

    void Update()
    {
        if (Application.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                RecordCurrentPosition();
            }
        }
    }

    private void RecordCurrentPosition()
    {
        if (player != null)
        {
            recordedXPositions.Add(player.position.x);
            Debug.Log($"<color=red>[Sync Debug] Beat enregistré à X : {player.position.x}</color>");
        }
        else
        {
            recordedXPositions.Add(transform.position.x);
            Debug.Log($"<color=red>[Sync Debug] Beat enregistré à X : {transform.position.x}</color>");
        }
    }

    private void OnDrawGizmos()
    {
        if (recordedXPositions == null || recordedXPositions.Count == 0) return;

        Gizmos.color = recordedBeatColor;

        foreach (float xPos in recordedXPositions)
        {
            float baselineY = (player != null) ? player.position.y : transform.position.y;

            Vector3 bottom = new Vector3(xPos, baselineY - (lineLength / 2f), 0f);
            Vector3 top = new Vector3(xPos, baselineY + (lineLength / 2f), 0f);

            Gizmos.DrawLine(bottom, top);
        }
    }

    [ContextMenu("Effacer les Beats Enregistrés")]
    public void ClearRecordedBeats()
    {
        recordedXPositions.Clear();
        Debug.Log("<color=orange>[Sync Debug] Toutes les lignes de debug ont été effacées.</color>");
    }
}