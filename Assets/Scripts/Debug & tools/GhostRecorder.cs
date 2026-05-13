using UnityEngine;
using System.Collections.Generic;

public class GhostRecorder : MonoBehaviour
{
    public GhostData dataToSave;
    public bool isRecording = false;
    private float timer = 0f;
    private Animator anim;

    void Start() => anim = GetComponent<Animator>();

    void Update()
    {
        if (!isRecording || dataToSave == null) return;

        timer += Time.deltaTime;

        GhostFrame frame = new GhostFrame
        {
            time = timer,
            position = transform.position,
            animBoolName = "isSliding",
            animValue = anim.GetBool("isSliding")
        };

        dataToSave.frames.Add(frame);
    }
}