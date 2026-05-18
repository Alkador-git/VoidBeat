using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TempoIndicator : MonoBehaviour
{
    [Header("BPM Central")]
    public RectTransform indicatorUI;
    public Text bpmText;
    public float pulseScale = 1.3f;

    [Header("Symboles Défilants BPM")]
    public RectTransform[] leftSymbols = new RectTransform[4];
    public RectTransform[] rightSymbols = new RectTransform[4];
    public float distanceBeat = 150f;

    [Header("Paramètres de Fondu (Opacités)")]
    public float fadeInStartDistance = 600f;
    public float fadeInEndDistance = 450f;
    public float fadeOutThresholdX = 10f;

    [Header("Animation de Flash de Précision")]
    public float flashDuration = 0.25f;
    public Color perfectColor = Color.green;
    public Color goodColor = Color.blue;
    public Color fairColor = Color.yellow;
    public Color missColor = Color.red;

    [Header("Outils Éditeur")]
    public Transform player;
    public Color gizmoBeatColor = Color.magenta;
    public float lineLength = 12f;

    [Header("Paramètres d'Animation Rythmique")]
    [Range(0.05f, 0.5f)] public float shrinkDurationPercent = 0.2f;
    [Range(0.05f, 0.5f)] public float anticipationDurationPercent = 0.35f;

    [Header("Couleurs Par Défaut")]
    public Color beatColor = Color.green;
    public Color anticipateColor = Color.cyan;
    public Color normalColor = new Color(1, 1, 1, 0.5f);

    private Image img;
    private Vector3 originalScale;
    private Image[] leftImages;
    private Image[] rightImages;
    private Color currentFlashColor;
    private float flashTimer;

    // --- INITIALISATION ---

    /// Initialise les structures d'images et s'abonne aux événements.
    void Start()
    {
        if (indicatorUI != null)
        {
            img = indicatorUI.GetComponent<Image>();
            originalScale = indicatorUI.localScale;
        }

        leftImages = new Image[leftSymbols.Length];
        for (int i = 0; i < leftSymbols.Length; i++)
        {
            if (leftSymbols[i] != null)
            {
                leftImages[i] = leftSymbols[i].GetComponent<Image>();
            }
        }

        rightImages = new Image[rightSymbols.Length];
        for (int i = 0; i < rightSymbols.Length; i++)
        {
            if (rightSymbols[i] != null)
            {
                rightImages[i] = rightSymbols[i].GetComponent<Image>();
            }
        }

        BeatManager.OnInputFeedback += HandleInputFeedback;
    }

    /// Se désabonne des événements à la destruction de l'objet.
    void OnDestroy()
    {
        BeatManager.OnInputFeedback -= HandleInputFeedback;
    }

    // --- BOUCLE PRINCIPALE ---

    /// Met à jour les positions, les opacités et les animations de couleur de l'interface.
    void Update()
    {
        if (BeatManager.Instance == null || indicatorUI == null || BeatManager.Instance.dataContainer == null) return;

        if (bpmText != null)
        {
            bpmText.text = Mathf.RoundToInt(BeatManager.Instance.currentBPM).ToString() + " BPM";
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
        }

        float flashRatio = flashTimer > 0f ? Mathf.Clamp01(flashTimer / flashDuration) : 0f;
        float musicTimer = BeatManager.Instance.GetMusicTimer();
        List<BeatPoint> beats = BeatManager.Instance.dataContainer.recordedBeats;

        if (beats.Count < 2) return;

        int nextBeatIndex = -1;

        for (int i = 0; i < beats.Count; i++)
        {
            if (beats[i].musicTime > musicTimer)
            {
                nextBeatIndex = i;
                break;
            }
        }

        if (nextBeatIndex < 0)
        {
            ResetVisuals();
            HideAllSymbols();
            return;
        }

        float currentBPM = BeatManager.Instance.currentBPM;
        float currentBeatInterval = currentBPM > 0 ? (60f / currentBPM) : 0.5f;

        for (int k = 0; k < 4; k++)
        {
            int targetBeatDataIdx = nextBeatIndex + k;

            if (targetBeatDataIdx < beats.Count)
            {
                float timeRemaining = beats[targetBeatDataIdx].musicTime - musicTimer;
                float beatOffset = timeRemaining / currentBeatInterval;

                if (beatOffset <= 4f)
                {
                    float posXLeft = -beatOffset * distanceBeat;
                    float posXRight = beatOffset * distanceBeat;

                    if (leftSymbols[k] != null)
                    {
                        leftSymbols[k].gameObject.SetActive(true);
                        leftSymbols[k].anchoredPosition = new Vector2(posXLeft, 0f);

                        if (leftImages[k] != null)
                        {
                            float alpha = 1f;
                            float absX = Mathf.Abs(posXLeft);

                            if (absX > fadeInEndDistance)
                            {
                                alpha = Mathf.InverseLerp(fadeInStartDistance, fadeInEndDistance, absX);
                            }
                            else if (absX < fadeOutThresholdX)
                            {
                                alpha = Mathf.InverseLerp(0f, fadeOutThresholdX, absX);
                            }

                            Color c = Color.white;
                            c.a = alpha;

                            if (flashTimer > 0f && k == 0)
                            {
                                Color flashed = currentFlashColor;
                                flashed.a = alpha;
                                c = Color.Lerp(c, flashed, flashRatio);
                            }

                            leftImages[k].color = c;
                        }
                    }

                    if (rightSymbols[k] != null)
                    {
                        rightSymbols[k].gameObject.SetActive(true);
                        rightSymbols[k].anchoredPosition = new Vector2(posXRight, 0f);

                        if (rightImages[k] != null)
                        {
                            float alpha = 1f;
                            float absX = Mathf.Abs(posXRight);

                            if (absX > fadeInEndDistance)
                            {
                                alpha = Mathf.InverseLerp(fadeInStartDistance, fadeInEndDistance, absX);
                            }
                            else if (absX < fadeOutThresholdX)
                            {
                                alpha = Mathf.InverseLerp(0f, fadeOutThresholdX, absX);
                            }

                            Color c = Color.white;
                            c.a = alpha;

                            if (flashTimer > 0f && k == 0)
                            {
                                Color flashed = currentFlashColor;
                                flashed.a = alpha;
                                c = Color.Lerp(c, flashed, flashRatio);
                            }

                            rightImages[k].color = c;
                        }
                    }
                }
                else
                {
                    DisableSymbolPair(k);
                }
            }
            else
            {
                DisableSymbolPair(k);
            }
        }

        float prevTime = (nextBeatIndex == 0) ? 0f : beats[nextBeatIndex - 1].musicTime;
        float nextTime = beats[nextBeatIndex].musicTime;
        float beatDuration = nextTime - prevTime;

        float beatPhase = 0f;
        if (beatDuration > 0f)
        {
            beatPhase = (musicTimer - prevTime) / beatDuration;
            beatPhase = Mathf.Clamp01(beatPhase);
        }

        float anticipateThreshold = 1f - anticipationDurationPercent;
        Color targetCenterColor = normalColor;
        Vector3 targetScale = originalScale;

        if (beatPhase >= anticipateThreshold)
        {
            float progress = (beatPhase - anticipateThreshold) / anticipationDurationPercent;
            progress = Mathf.Clamp01(progress);

            targetScale = Vector3.Lerp(originalScale, originalScale * pulseScale, progress);
            targetCenterColor = Color.Lerp(normalColor, anticipateColor, progress);
        }
        else if (beatPhase <= shrinkDurationPercent)
        {
            float progress = beatPhase / shrinkDurationPercent;
            progress = Mathf.Clamp01(progress);

            targetScale = Vector3.Lerp(originalScale * pulseScale, originalScale, progress);
            targetCenterColor = Color.Lerp(beatColor, normalColor, progress);
        }

        if (flashTimer > 0f)
        {
            indicatorUI.localScale = originalScale * pulseScale;
            if (img != null) img.color = Color.Lerp(targetCenterColor, currentFlashColor, flashRatio);
        }
        else
        {
            indicatorUI.localScale = targetScale;
            if (img != null) img.color = targetCenterColor;
        }
    }

    // --- GESTION DES VISUELS ---

    /// Intercepte le signal de réussite pour assigner la couleur du flash.
    private void HandleInputFeedback(string feedback)
    {
        flashTimer = flashDuration;
        if (feedback == "parfait") currentFlashColor = perfectColor;
        else if (feedback == "bien") currentFlashColor = goodColor;
        else if (feedback == "juste") currentFlashColor = fairColor;
        else currentFlashColor = missColor;
    }

    /// Remet les éléments graphiques centraux à leur configuration par défaut.
    private void ResetVisuals()
    {
        indicatorUI.localScale = originalScale;
        if (img != null) img.color = normalColor;
    }

    /// Désactive un doublon d'icônes défilantes gauche et droite.
    private void DisableSymbolPair(int index)
    {
        if (leftSymbols[index] != null) leftSymbols[index].gameObject.SetActive(false);
        if (rightSymbols[index] != null) rightSymbols[index].gameObject.SetActive(false);
    }

    /// Masque tous les repères de défilement de l'interface.
    private void HideAllSymbols()
    {
        for (int i = 0; i < 4; i++)
        {
            DisableSymbolPair(i);
        }
    }

    // --- OUTILS EDITEUR ---

    /// Dessine les repères visuels d'emplacement des beats dans la scène.
    private void OnDrawGizmos()
    {
        if (BeatManager.Instance == null || BeatManager.Instance.dataContainer == null) return;

        List<BeatPoint> recordedBeats = BeatManager.Instance.dataContainer.recordedBeats;
        if (recordedBeats == null || recordedBeats.Count == 0) return;

        Gizmos.color = gizmoBeatColor;

        foreach (BeatPoint beat in recordedBeats)
        {
            float baselineY = (player != null) ? player.position.y : transform.position.y;

            Vector3 bottom = new Vector3(beat.xPos, baselineY - (lineLength / 2f), 0f);
            Vector3 top = new Vector3(beat.xPos, baselineY + (lineLength / 2f), 0f);

            Gizmos.DrawLine(bottom, top);
        }
    }

    /// Supprime l'intégralité des battements stockés dans le fichier de données.
    [ContextMenu("Effacer les Données de Beat")]
    public void ClearData()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.dataContainer != null)
        {
            BeatManager.Instance.dataContainer.recordedBeats.Clear();
        }
    }
}