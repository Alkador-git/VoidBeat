using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    // --- CONFIGURATION ET ETAT ---

    public static ScoreManager Instance;
    public int currentScore = 0;
    public float currentMultiplier = 1f;
    private string lastFeedbackType = "";
    private int currentStreak = 0;
    private float currentIncrement = 1f;

    // --- INITIALISATION ---

    /// Initialisation du singleton au chargement.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Abonnement aux evenements du gestionnaire.
    void Start()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.OnInputFeedback += ProcessScore;
        }
    }

    /// Desabonnement des evenements.
    void OnDestroy()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.OnInputFeedback -= ProcessScore;
        }
    }

    // --- SYSTEME DE SCORE ---

    /// Calcul des points et de la progression du multiplicateur.
    private void ProcessScore(string feedback)
    {
        if (feedback == "raté")
        {
            ResetMultiplier();
            return;
        }

        float deltaMs = CalculateClosestBeatDeltaMs();
        if (deltaMs > 125f) return;

        int currentRank = GetFeedbackRank(feedback);
        int lastRank = GetFeedbackRank(lastFeedbackType);

        if (currentRank >= lastRank && lastFeedbackType != "")
        {
            currentStreak++;
            currentMultiplier += currentIncrement;
            currentIncrement *= 0.5f;
        }
        else
        {
            currentStreak = 1;
            currentMultiplier = 1f;
            currentIncrement = 1f;
        }

        lastFeedbackType = feedback;

        float baseScore = Mathf.Max(0f, 1000f * (1f - (deltaMs / 125f)));
        int scoreGained = Mathf.RoundToInt(baseScore * currentMultiplier);
        currentScore += scoreGained;

        Debug.Log("[ScoreManager] Precision: " + feedback + " | Ecart: " + deltaMs.ToString("F1") + "ms | Points gagnes: " + scoreGained + " | Multiplicateur: " + currentMultiplier.ToString("F4") + "x | Score total: " + currentScore);
    }

    /// Reinitialisation complete du multiplicateur.
    private void ResetMultiplier()
    {
        currentStreak = 0;
        currentMultiplier = 1f;
        currentIncrement = 1f;
        lastFeedbackType = "";
    }

    /// Conversion de la chaine de caracteres en valeur numerique.
    private int GetFeedbackRank(string feedback)
    {
        switch (feedback)
        {
            case "juste": return 1;
            case "bien": return 2;
            case "parfait": return 3;
            default: return 0;
        }
    }

    /// Calcul de la distance temporelle du battement le plus proche.
    private float CalculateClosestBeatDeltaMs()
    {
        if (BeatManager.Instance == null || BeatManager.Instance.dataContainer == null) return float.MaxValue;

        float currentMusicTime = BeatManager.Instance.GetMusicTimer();
        float minDelta = float.MaxValue;

        foreach (var beat in BeatManager.Instance.dataContainer.recordedBeats)
        {
            float delta = Mathf.Abs(currentMusicTime - beat.musicTime);
            if (delta < minDelta)
            {
                minDelta = delta;
            }
        }

        return minDelta * 1000f;
    }
}