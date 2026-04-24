using UnityEngine;
using UnityEngine.UI;

public class TempoIndicator : MonoBehaviour
{
    // --- VISUELS ---

    [Header("Réglages Visuels")]
    public RectTransform indicatorUI;
    public Text bpmText;
    public float pulseScale = 1.2f;
    public float lerpSpeed = 10f;

    // --- COULEURS ---

    [Header("Couleurs")]
    public Color beatColor = Color.cyan;
    public Color normalColor = new Color(1, 1, 1, 0.5f);

    // --- COMPOSANTS ---

    private Image img;
    private Vector3 originalScale;

    // --- INITIALISATION ---

    /// Récupère les composants et l'échelle originale
    void Start()
    {
        if (indicatorUI != null)
        {
            img = indicatorUI.GetComponent<Image>();
            originalScale = indicatorUI.localScale;
        }
    }

    // --- MISE À JOUR ---

    /// Met à jour l'indicateur visuel en fonction de la détection du beat
    void Update()
    {
        if (BeatManager.Instance == null || indicatorUI == null) return;

        // Affiche le BPM actuel
        if (bpmText != null)
        {
            bpmText.text = Mathf.RoundToInt(BeatManager.Instance.currentBPM).ToString() + " BPM";
        }

        if (BeatManager.Instance.IsActionOnBeat())
        {
            indicatorUI.localScale = originalScale * pulseScale;
            if (img != null) img.color = beatColor;
        }
        else
        {
            indicatorUI.localScale = Vector3.Lerp(indicatorUI.localScale, originalScale, Time.deltaTime * lerpSpeed);
            if (img != null) img.color = Color.Lerp(img.color, normalColor, Time.deltaTime * lerpSpeed);
        }
    }
}