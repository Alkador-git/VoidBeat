using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TempoIndicator : MonoBehaviour
{
    // --- PARAMETRES ET VISUELS ---

    [Header("BPM Central")]
    public RectTransform indicatorUI;
    public Text bpmText;
    public float pulseScale = 1.3f;

    [Header("Symboles Défilants BPM")]
    public RectTransform[] leftSymbols = new RectTransform[4];
    public RectTransform[] rightSymbols = new RectTransform[4];
    public float distanceBeat = 150f;

    [Header("Paramètres d'Animation")]
    [Range(0.05f, 0.5f)] public float shrinkDurationPercent = 0.2f;
    [Range(0.05f, 0.5f)] public float anticipationDurationPercent = 0.35f;

    [Header("Couleurs")]
    public Color beatColor = Color.green;
    public Color anticipateColor = Color.cyan;
    public Color normalColor = new Color(1, 1, 1, 0.5f);

    [Header("Outils Éditeur")]
    public Transform player;
    public Color gizmoBeatColor = Color.magenta;
    public float lineLength = 12f;

    private Image img;
    private Vector3 originalScale;

    // --- INITIALISATION ---

    /// Initialisation des références et stockage de la taille d'origine du widget.
    void Start()
    {
        if (indicatorUI != null)
        {
            img = indicatorUI.GetComponent<Image>();
            originalScale = indicatorUI.localScale;
        }
    }

    // --- BOUCLE PRINCIPALE ---

    /// Gestion des calculs de défilement des demi-cercles et mise à jour des pulsations de l'UI.
    void Update()
    {
        if (BeatManager.Instance == null || indicatorUI == null || BeatManager.Instance.dataContainer == null) return;

        if (bpmText != null)
        {
            bpmText.text = Mathf.RoundToInt(BeatManager.Instance.currentBPM).ToString() + " BPM";
        }

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

        // Déplacement linéaire des 4 paires de symboles d'inputs futurs
        for (int k = 0; k < 4; k++)
        {
            int targetBeatDataIdx = nextBeatIndex + k;

            if (targetBeatDataIdx < beats.Count)
            {
                float timeRemaining = beats[targetBeatDataIdx].musicTime - musicTimer;
                float beatOffset = timeRemaining / currentBeatInterval;

                if (beatOffset <= 4f)
                {
                    if (leftSymbols[k] != null)
                    {
                        leftSymbols[k].gameObject.SetActive(true);
                        leftSymbols[k].anchoredPosition = new Vector2(-beatOffset * distanceBeat, 0f);
                    }

                    if (rightSymbols[k] != null)
                    {
                        rightSymbols[k].gameObject.SetActive(true);
                        rightSymbols[k].anchoredPosition = new Vector2(beatOffset * distanceBeat, 0f);
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

        // Calcul et exécution des pulsations d'anticipation sur le réceptacle central
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

        if (beatPhase >= anticipateThreshold)
        {
            float progress = (beatPhase - anticipateThreshold) / anticipationDurationPercent;
            progress = Mathf.Clamp01(progress);

            indicatorUI.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, progress);
            if (img != null) img.color = Color.Lerp(normalColor, anticipateColor, progress);
        }
        else if (beatPhase <= shrinkDurationPercent)
        {
            float progress = beatPhase / shrinkDurationPercent;
            progress = Mathf.Clamp01(progress);

            indicatorUI.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, progress);
            if (img != null) img.color = Color.Lerp(beatColor, normalColor, progress);
        }
        else
        {
            ResetVisuals();
        }
    }

    // --- GESTION DES VISUELS ---

    /// Remet les éléments graphiques centraux à leur taille et teinte par défaut.
    private void ResetVisuals()
    {
        indicatorUI.localScale = originalScale;
        if (img != null) img.color = normalColor;
    }

    /// Désactive les objets d'une paire de symboles défilants.
    private void DisableSymbolPair(int index)
    {
        if (leftSymbols[index] != null) leftSymbols[index].gameObject.SetActive(false);
        if (rightSymbols[index] != null) rightSymbols[index].gameObject.SetActive(false);
    }

    /// Cache l'ensemble des indicateurs défilants présents sur l'interface.
    private void HideAllSymbols()
    {
        for (int i = 0; i < 4; i++)
        {
            DisableSymbolPair(i);
        }
    }

    /// Dessine les marques temporelles des beats enregistrés dans l'éditeur de niveau Unity.
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

    /// Permet de vider le conteneur de données depuis le menu contextuel de l'inspecteur.
    [ContextMenu("Effacer les Données de Beat")]
    public void ClearData()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.dataContainer != null)
        {
            BeatManager.Instance.dataContainer.recordedBeats.Clear();
        }
    }
}