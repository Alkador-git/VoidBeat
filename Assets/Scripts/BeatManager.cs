using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;

    [Header("Configuration Rythme")]
    public float bpm = 120f;
    public AudioSource musicSource;
    public float beatWindow = 0.25f;

    private float beatInterval;
    private float nextBeatTime;

    void Awake() => Instance = this;

    void Start()
    {
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval;
        if (musicSource) musicSource.Play();
    }

    void Update()
    {
        if (Time.time >= nextBeatTime)
        {
            nextBeatTime += beatInterval;
        }
    }

    // Vérifie si l'action du joueur tombe dans la fenêtre du tempo
    public bool IsActionOnBeat()
    {
        float timeToBeat = Mathf.Abs(Time.time - (nextBeatTime - beatInterval));
        float timeToNextBeat = Mathf.Abs(Time.time - nextBeatTime);

        return (timeToBeat <= beatWindow || timeToNextBeat <= beatWindow);
    }
}