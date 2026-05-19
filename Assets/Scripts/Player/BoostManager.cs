using UnityEngine;
using UnityEngine.UI;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance;

    public float currentBoost = 50f;
    public float maxBoost = 100f;
    public float decayRate = 5f;
    public float boostGain = 10f;

    public Slider boostSlider;
    public Image fillImage;
    public Color normalColor = Color.cyan;
    public Color lowColor = Color.magenta;

    // --- INITIALISATION ---

    /// Initialisation du singleton au chargement.
    void Awake() => Instance = this;

    // --- BOUCLE PRINCIPALE ---

    /// Application du déclin de boost modulé par la zone et mise à jour de l'UI.
    void Update()
    {
        float zoneMultiplier = 1f;
        bool shouldDecay = false;

        if (BeatManager.Instance != null)
        {
            shouldDecay = BeatManager.Instance.IsMusicActive();

            if (BeatManager.Instance.currentZone != null)
            {
                switch (BeatManager.Instance.currentZone.zoneType)
                {
                    case BeatManager.ZoneType.DemoObstacle:
                        zoneMultiplier = 0.4f;
                        break;
                    case BeatManager.ZoneType.Narratif:
                        zoneMultiplier = 0.5f;
                        break;
                    case BeatManager.ZoneType.Checkpoint:
                        zoneMultiplier = 0.35f;
                        break;
                    case BeatManager.ZoneType.Action:
                        zoneMultiplier = 1.00f;
                        break;
                    case BeatManager.ZoneType.Transition:
                        zoneMultiplier = 0.25f;
                        break;
                }
            }
        }

        if (shouldDecay)
        {
            currentBoost -= decayRate * zoneMultiplier * Time.deltaTime;
            currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
        }

        if (boostSlider) boostSlider.value = currentBoost / maxBoost;

        if (fillImage) fillImage.color = Color.Lerp(lowColor, normalColor, currentBoost / maxBoost);
    }

    // --- GESTION DU BOOST ---

    /// Ajout de la valeur de boost par défaut.
    public void AddBoost()
    {
        AddBoost(boostGain);
    }

    /// Ajout d'une valeur de boost spécifique avec bridage au maximum.
    public void AddBoost(float boostReward)
    {
        currentBoost += boostReward;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }

    /// Retrait d'une valeur de boost spécifique avec bridage à zéro.
    public void RemoveBoost(float amount)
    {
        currentBoost -= amount;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }
}