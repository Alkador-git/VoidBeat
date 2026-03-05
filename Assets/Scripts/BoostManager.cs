using UnityEngine;
using UnityEngine.UI;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance;

    [Header("Paramètres du Boost")]
    public float currentBoost = 50f;
    public float maxBoost = 100f;
    public float decayRate = 5f; // La barre se vide naturellement
    public float boostGain = 10f;

    [Header("UI")]
    public Slider boostSlider; // Glissez votre Slider UI ici
    public Image fillImage;
    public Color normalColor = Color.cyan; // Couleur de K-Z0 
    public Color lowColor = Color.magenta; // Couleur du Néant-X

    void Awake() => Instance = this;

    void Update()
    {
        // Perte d'élan constante
        currentBoost -= decayRate * Time.deltaTime;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);

        // Mise à jour de l'UI
        if (boostSlider) boostSlider.value = currentBoost / maxBoost;

        // Changement de couleur selon l'urgence
        if (fillImage) fillImage.color = Color.Lerp(lowColor, normalColor, currentBoost / maxBoost);

        if (currentBoost <= 0)
        {
            // Logique de défaite : aspiré par le trou noir 
            Debug.Log("K-Z0 a été aspiré par le Néant-X !");
        }
    }

    public void AddBoost()
    {
        currentBoost += boostGain;
        // Effets visuels ici (particules, tremblement d'écran)
    }
}