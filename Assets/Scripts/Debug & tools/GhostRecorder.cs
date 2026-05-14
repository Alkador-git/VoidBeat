using UnityEngine;
using System.Collections.Generic;

public class GhostRecorder : MonoBehaviour
{
    public GhostData dataToSave;
    public bool isRecording = false;
    private float timer = 0f;
    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
        {
            Debug.LogError("GhostRecorder : Aucun Animator trouvé dans les objets enfants.");
        }
    }

    void Update()
    {
        if (!isRecording || dataToSave == null || anim == null) return;

        timer += Time.deltaTime;

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