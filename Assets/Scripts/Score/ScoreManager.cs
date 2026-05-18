using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int currentScore = 0;
    public float currentMultiplier = 1f;

    [Header("Composants UI")]
    public TextMeshProUGUI victoryTotalScoreText;
    public TextMeshProUGUI inGameTotalScoreText;
    public TextMeshProUGUI scorePopUpText;
    public float popUpDuration = 0.5f;

    [Header("Réglages des Animations Visuelles")]
    public float baseShakeMagnitude = 3f;
    public float scorePunchScale = 1.25f;
    public float scorePunchDuration = 0.15f;

    private string lastFeedbackType = "";
    private int currentStreak = 0;
    private float currentIncrement = 1f;
    private Coroutine popUpCoroutine;
    private Coroutine scorePunchCoroutine;

    // --- INITIALISATION ---

    /// Initialisation du singleton au chargement de la scène.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Abonnement aux événements du gestionnaire au lancement.
    void Start()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.OnInputFeedback += ProcessScore;
        }

        if (scorePopUpText != null)
        {
            scorePopUpText.text = "";
        }

        UpdateUI();
    }

    /// Désabonnement des événements à la destruction de l'objet.
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

        DisplayScorePopUp(scoreGained);
        PunchInGameScore();
        UpdateUI();

        Debug.Log("[ScoreManager] Precision: " + feedback + " | Ecart: " + deltaMs.ToString("F1") + "ms | Points gagnes: " + scoreGained + " | Multiplicateur: " + currentMultiplier.ToString("F4") + "x | Score total: " + currentScore);
    }

    /// Réinitialisation complète du multiplicateur lors d'un raté.
    private void ResetMultiplier()
    {
        currentStreak = 0;
        currentMultiplier = 1f;
        currentIncrement = 1f;
        lastFeedbackType = "";
        DisplayScorePopUp(0);
        Debug.Log("[ScoreManager] Note ratee. Multiplicateur reinitialise.");
    }

    /// Conversion de la chaîne de caractères de précision en valeur numérique.
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

    // --- GESTION DE L'INTERFACE ---

    /// Mise à jour des affichages textuels fixes de fin de niveau.
    public void UpdateUI()
    {
        if (victoryTotalScoreText != null)
        {
            victoryTotalScoreText.text = currentScore.ToString();
        }
    }

    /// Déclenche l'affichage temporaire et l'animation du gain de points sous l'indicateur.
    private void DisplayScorePopUp(int points)
    {
        if (scorePopUpText == null) return;

        if (popUpCoroutine != null)
        {
            StopCoroutine(popUpCoroutine);
        }

        popUpCoroutine = StartCoroutine(ClearPopUpRoutine(points));
    }

    /// Coroutine appliquant une translation verticale, un fondu et un tremblement indexé sur le multiplicateur.
    private IEnumerator ClearPopUpRoutine(int points)
    {
        scorePopUpText.text = "+" + points.ToString();
        Vector2 startPos = scorePopUpText.rectTransform.anchoredPosition;
        Color startColor = scorePopUpText.color;

        float animDuration = popUpDuration * 0.75f;
        float stayDuration = popUpDuration * 0.25f;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;

            Vector2 upwardPos = startPos + new Vector2(0f, t * 40f);
            Vector2 shakeOffset = Random.insideUnitCircle * baseShakeMagnitude * currentMultiplier;
            scorePopUpText.rectTransform.anchoredPosition = upwardPos + shakeOffset;

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            scorePopUpText.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        scorePopUpText.text = "";
        scorePopUpText.rectTransform.anchoredPosition = startPos;
        scorePopUpText.color = startColor;
    }

    /// Déclenche l'animation d'impulsion de taille sur le texte du score total en jeu.
    private void PunchInGameScore()
    {
        if (inGameTotalScoreText == null) return;

        if (scorePunchCoroutine != null)
        {
            StopCoroutine(scorePunchCoroutine);
        }

        scorePunchCoroutine = StartCoroutine(PunchScaleTotalScoreRoutine());
    }

    /// Coroutine simulant un effet élastique d'échelle sur le score total in game.
    private IEnumerator PunchScaleTotalScoreRoutine()
    {
        if (inGameTotalScoreText == null) yield break;

        inGameTotalScoreText.text = currentScore.ToString();
        Vector3 originalScale = Vector3.one;
        float elapsed = 0f;

        while (elapsed < scorePunchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scorePunchDuration;
            float currentScaleFactor = Mathf.Sin(t * Mathf.PI);
            inGameTotalScoreText.rectTransform.localScale = Vector3.Lerp(originalScale, originalScale * scorePunchScale, currentScaleFactor);
            yield return null;
        }

        inGameTotalScoreText.rectTransform.localScale = originalScale;
    }
}