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
    public float scoreRollSpeed = 2500f;

    private string lastFeedbackType = "";
    private int currentStreak = 0;
    private float currentIncrement = 1f;
    private Coroutine popUpCoroutine;
    private Coroutine scorePunchCoroutine;
    private Vector2 nativePopUpPos;
    private float displayedScore = 0f;

    // --- INITIALISATION ---

    /// Initialisation du singleton au chargement de la scène.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Établissement des abonnements aux événements rythmiques.
    void Start()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.OnInputFeedback += ProcessScore;
        }

        if (scorePopUpText != null)
        {
            scorePopUpText.text = "";
            nativePopUpPos = scorePopUpText.rectTransform.anchoredPosition;
        }

        displayedScore = currentScore;
        UpdateUI();
    }

    /// Suppression des abonnements aux événements.
    void OnDestroy()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.OnInputFeedback -= ProcessScore;
        }
    }

    // --- SYSTEME DE SCORE ---

    /// Calcul et distribution des points selon la précision.
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

    /// Application d'une pénalité suite à une collision.
    public void DeductObstacleCollisionScore()
    {
        currentScore = Mathf.Max(0, currentScore - 1000);
        DisplayScorePopUp(-1000);
        PunchInGameScore();
        UpdateUI();
    }

    /// Réinitialisation du multiplicateur de combo.
    private void ResetMultiplier()
    {
        currentStreak = 0;
        currentMultiplier = 1f;
        currentIncrement = 1f;
        lastFeedbackType = "";

        currentScore = Mathf.Max(0, currentScore - 500);

        DisplayScorePopUp(-500);
        PunchInGameScore();
        UpdateUI();

        Debug.Log("[ScoreManager] Note ratee. Multiplicateur reinitialise.");
    }

    /// Détermination du rang numérique de la note.
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

    /// Calcul de l'écart avec la balise la plus proche.
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

    /// Progression fluide du compteur de score en jeu.
    private void Update()
    {
        if (inGameTotalScoreText == null) return;

        displayedScore = Mathf.MoveTowards(displayedScore, currentScore, scoreRollSpeed * Time.deltaTime);
        inGameTotalScoreText.text = Mathf.RoundToInt(displayedScore).ToString();
    }

    /// Actualisation des textes fixes de l'interface.
    public void UpdateUI()
    {
        if (victoryTotalScoreText != null)
        {
            victoryTotalScoreText.text = currentScore.ToString();
        }
    }

    /// Déclenchement de l'apparition dynamique du gain.
    private void DisplayScorePopUp(int points)
    {
        if (scorePopUpText == null) return;

        if (popUpCoroutine != null)
        {
            StopCoroutine(popUpCoroutine);
        }

        scorePopUpText.rectTransform.anchoredPosition = nativePopUpPos;
        popUpCoroutine = StartCoroutine(ClearPopUpRoutine(points));
    }

    /// Animation de mouvement et fondu de la popup.
    private IEnumerator ClearPopUpRoutine(int points)
    {
        string prefix = points >= 0 ? "+" : "";
        scorePopUpText.text = prefix + points.ToString();
        Color startColor = scorePopUpText.color;

        float animDuration = popUpDuration * 0.75f;
        float stayDuration = popUpDuration * 0.25f;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;

            Vector2 upwardPos = nativePopUpPos + new Vector2(0f, t * 40f);
            Vector2 shakeOffset = Random.insideUnitCircle * baseShakeMagnitude * currentMultiplier;
            scorePopUpText.rectTransform.anchoredPosition = upwardPos + shakeOffset;

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            scorePopUpText.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        scorePopUpText.text = "";
        scorePopUpText.rectTransform.anchoredPosition = nativePopUpPos;
        scorePopUpText.color = startColor;
    }

    /// Déclenchement de l'effet d'échelle sur le score global.
    private void PunchInGameScore()
    {
        if (inGameTotalScoreText == null) return;

        if (scorePunchCoroutine != null)
        {
            StopCoroutine(scorePunchCoroutine);
        }

        scorePunchCoroutine = StartCoroutine(PunchScaleTotalScoreRoutine());
    }

    /// Animation élastique de l'échelle du score.
    private IEnumerator PunchScaleTotalScoreRoutine()
    {
        if (inGameTotalScoreText == null) yield break;

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