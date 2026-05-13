using UnityEngine;
using System.Collections;

public class GhostReplayer : MonoBehaviour
{
    public GhostData dataToPlay;
    private SpriteRenderer sr;
    private Animator anim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        sr.enabled = false;
    }

    public void StartDemo()
    {
        sr.enabled = true;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        if (dataToPlay == null || dataToPlay.frames.Count == 0) yield break;

        float startTime = Time.time;
        int frameIndex = 0;

        while (frameIndex < dataToPlay.frames.Count)
        {
            float elapsed = Time.time - startTime;
            GhostFrame currentFrame = dataToPlay.frames[frameIndex];

            if (elapsed >= currentFrame.time)
            {
                transform.position = currentFrame.position;
                if (!string.IsNullOrEmpty(currentFrame.animBoolName))
                    anim.SetBool(currentFrame.animBoolName, currentFrame.animValue);

                frameIndex++;
            }
            yield return null;
        }
        sr.enabled = false;
    }
}