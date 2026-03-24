using System;
using UnityEngine;
using UnityEngine.UI;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance;

    [Header("Paramètres du Boost")]
    public float currentBoost = 50f;
    public float maxBoost = 100f;
    public float decayRate = 5f;
    public float boostGain = 10f;

    [Header("UI")]
    public Slider boostSlider;
    public Image fillImage;
    public Color normalColor = Color.cyan; 
    public Color lowColor = Color.magenta;

    // Initialise l'instance singleton
    void Awake() => Instance = this;

    // Met à jour le boost et l'interface utilisateur
    void Update()
    {
        currentBoost -= decayRate * Time.deltaTime;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);

        if (boostSlider) boostSlider.value = currentBoost / maxBoost;

        if (fillImage) fillImage.color = Color.Lerp(lowColor, normalColor, currentBoost / maxBoost);
    }
    // Ajout de boost par défaut
    public void AddBoost()
    {
        AddBoost(boostGain);
    }

    // Ajout d'une quantité de boost spécifique
    public void AddBoost(float boostReward)
    {
        currentBoost += boostGain;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }

    // Réduit le boost d'une quantité spécifique
    public void RemoveBoost(float amount)
    {
        currentBoost -= amount;

        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }

}