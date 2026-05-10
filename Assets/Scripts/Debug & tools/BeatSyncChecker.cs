using UnityEngine;

public class BeatSyncChecker : MonoBehaviour
{
    // --- CONFIGURATION ---

    [Header("Paramètres")]
    public KeyCode recordKey = KeyCode.E;

    // --- UPDATE LOOP ---

    /// Handles manual beat recording input during gameplay.
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

    // --- RECORDING ---

    /// Triggers manual beat recording in BeatManager.
    private void TriggerManualRecord()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.RecordManualBeat();
        }
    }
}