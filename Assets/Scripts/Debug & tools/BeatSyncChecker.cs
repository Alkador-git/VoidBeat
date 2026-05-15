using UnityEngine;

public class BeatSyncChecker : MonoBehaviour
{
    // --- CONFIGURATION ---

    [Header("Paramètres")]
    public KeyCode recordKey = KeyCode.E;

    // --- UPDATE LOOP ---

    /// Gère l'entrée manuelle d'enregistrement de beat pendant le jeu.
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

    /// Déclenche l'enregistrement manuel dans le BeatManager.
    private void TriggerManualRecord()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.RecordManualBeat();
        }
    }
}