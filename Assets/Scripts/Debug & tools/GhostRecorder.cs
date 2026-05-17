using UnityEngine;
using System.Collections.Generic;

public class GhostRecorder : MonoBehaviour
{
    public GhostData dataToSave;
    public bool isRecording = false;

    [Header("Configuration des Triggers")]
    public string startTriggerTag = "RecordingTrigger";

    public string stopTriggerTag = "RecordingEndTrigger";

    private float timer = 0f;
    private float recordTimer = 0f;
    private float recordInterval = 1f / 90f;
    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!isRecording || dataToSave == null || anim == null) return;

        timer += Time.deltaTime;
        recordTimer += Time.deltaTime;

        if (recordTimer >= recordInterval)
        {
            recordTimer -= recordInterval;

            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            GhostFrame frame = new GhostFrame
            {
                time = timer,
                position = transform.position,
                animatorStateHash = stateInfo.shortNameHash,
                normalizedTime = stateInfo.normalizedTime
            };

            dataToSave.frames.Add(frame);
        }
    }

    // --- LOGIQUE DE DÉCLENCHEMENT ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(startTriggerTag))
        {
            if (dataToSave != null)
            {
                dataToSave.ClearData();
            }

            timer = 0f;
            recordTimer = 0f;
            isRecording = true;
        }
        else if (other.CompareTag(stopTriggerTag))
        {
            if (isRecording)
            {
                isRecording = false;

#if UNITY_EDITOR
                if (dataToSave != null)
                {
                    dataToSave.SaveData();
                }
#endif
            }
        }
    }
}