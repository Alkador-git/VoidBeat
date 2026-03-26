using UnityEngine;
using UnityEngine.UI;

public class TempoIndicator : MonoBehaviour
{
    [Header("Réglages Visuels")]
    public RectTransform indicatorUI;
    public float pulseScale = 1.2f;
    public float lerpSpeed = 10f;

    [Header("Couleurs")]
    public Color beatColor = Color.cyan;
    public Color normalColor = new Color(1, 1, 1, 0.5f);

    private Image img;
    private Vector3 originalScale;

    void Start()
    {
        if (indicatorUI != null)
        {
            img = indicatorUI.GetComponent<Image>();
            originalScale = indicatorUI.localScale;
        }
    }

    void Update()
    {
        if (BeatManager.Instance == null || indicatorUI == null) return;

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