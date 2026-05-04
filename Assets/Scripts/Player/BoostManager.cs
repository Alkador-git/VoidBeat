using System;
using UnityEngine;
using UnityEngine.UI;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance;

    // --- PARAMÈTRES ---

    [Header("Paramètres du Boost")]
    public float currentBoost = 50f;
    public float maxBoost = 100f;
    public float decayRate = 5f;
    public float boostGain = 10f;

    // --- INTERFACE UTILISATEUR ---

    [Header("UI")]
    public Slider boostSlider;
    public Image fillImage;
    public Color normalColor = Color.cyan;
    public Color lowColor = Color.magenta;

    // --- INITIALISATION ---

    void Awake() => Instance = this;

    // --- MISE À JOUR ---

    void Update()
    {
        currentBoost -= decayRate * Time.deltaTime;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);

        if (boostSlider) boostSlider.value = currentBoost / maxBoost;

        if (fillImage) fillImage.color = Color.Lerp(lowColor, normalColor, currentBoost / maxBoost);
    }

    // --- GESTION DU BOOST ---

    public void AddBoost()
    {
        AddBoost(boostGain);
    }

    public void AddBoost(float boostReward)
    {
        currentBoost += boostReward;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }

    public void RemoveBoost(float amount)
    {
        currentBoost -= amount;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }
}