using UnityEngine;
using System.Collections;

public class GhostReplayer : MonoBehaviour
{
    public GhostData dataToPlay;
    private Animator anim;
    private SpriteRenderer[] childRenderers;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        childRenderers = GetComponentsInChildren<SpriteRenderer>();
        SetGhostVisuals(false);
    }

    public void StartDemo()
    {
        SetGhostVisuals(true);
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

            while (frameIndex < dataToPlay.frames.Count && elapsed >= dataToPlay.frames[frameIndex].time)
            {
                GhostFrame currentFrame = dataToPlay.frames[frameIndex];

                transform.position = currentFrame.position;

                if (anim != null)
                {
                    anim.Play(currentFrame.animatorStateHash, 0, currentFrame.normalizedTime);
                }

                frameIndex++;
            }

            yield return null;
        }

        SetGhostVisuals(false);
    }

    private void SetGhostVisuals(bool show)
    {
        if (childRenderers == null) return;
        foreach (SpriteRenderer sr in childRenderers)
        {
            if (sr != null)
            {
                sr.enabled = show;
            }
        }
    }
}