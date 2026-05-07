using UnityEngine;

public class BeatSyncChecker : MonoBehaviour
{
    [Header("Paramètres")]
    public KeyCode recordKey = KeyCode.E;

    void Update()
    {
        if (Application.isPlaying)
        {
            if (Input.GetKeyDown(recordKey))
            {
                TriggerManualRecord();
            }
        }
    }

    private void TriggerManualRecord()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.RecordManualBeat();
        }
    }
}