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
    private Vector2 nativePopUpPos;

    // --- INITIALISATION ---

    /// Met en mémoire l'instance unique globale du système de points.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// Établit la liaison d'écoute auprès de l'horloge centrale du rythme.
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

        UpdateUI();
    }

    /// Supprime la liaison d'écoute à la mise hors tension de l'instance.
    void OnDestroy()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.OnInputFeedback -= ProcessScore;
        }
    }

    // --- SYSTEME DE SCORE ---

    /// Calcule le gain de points pondéré par la précision temporelle et le combo.
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

    /// Soustrait une pénalité fixe de points lors de l'impact matériel avec un obstacle.
    public void DeductObstacleCollisionScore()
    {
        currentScore = Mathf.Max(0, currentScore - 1000);
        DisplayScorePopUp(-1000);
        PunchInGameScore();
        UpdateUI();
    }

    /// Restitue les multiplicateurs par défaut et ampute le score suite à un échec.
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

    /// Associe une valeur de priorité structurelle selon la notation reçue.
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

    /// Extrait la distance en millisecondes séparant l'input de la balise la plus proche.
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

    /// Actualise les conteneurs textuels fixes présents sur les interfaces.
    public void UpdateUI()
    {
        if (victoryTotalScoreText != null)
        {
            victoryTotalScoreText.text = currentScore.ToString();
        }

        if (inGameTotalScoreText != null)
        {
            inGameTotalScoreText.text = currentScore.ToString();
        }
    }

    /// Gère la mise en file d'attente et l'affichage du texte contextuel de score.
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

    /// Anime l'élévation, les secousses de combo et la disparition du texte contextuel.
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

    /// Interrompt et réinitialise l'animation élastique du score total de l'UI.
    private void PunchInGameScore()
    {
        if (inGameTotalScoreText == null) return;

        if (scorePunchCoroutine != null)
        {
            StopCoroutine(scorePunchCoroutine);
        }

        scorePunchCoroutine = StartCoroutine(PunchScaleTotalScoreRoutine());
    }

    /// Opère une déformation sinusoïdale de l'échelle du texte de score en jeu.
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