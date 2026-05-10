using System;
using UnityEngine;
using UnityEngine.UI;

public class BoostManager : MonoBehaviour
{
    public static BoostManager Instance;

    // --- BOOST PARAMETERS ---

    public float currentBoost = 50f;

    public float maxBoost = 100f;

    public float decayRate = 5f;

    public float boostGain = 10f;

    // --- USER INTERFACE ---

    public Slider boostSlider;

    public Image fillImage;

    public Color normalColor = Color.cyan;

    public Color lowColor = Color.magenta;

    // --- INITIALIZATION ---

    void Awake() => Instance = this;

    // --- UPDATE LOOP ---

    /// Applies boost decay and updates UI display.
    void Update()
    {
        currentBoost -= decayRate * Time.deltaTime;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);

        if (boostSlider) boostSlider.value = currentBoost / maxBoost;

        if (fillImage) fillImage.color = Color.Lerp(lowColor, normalColor, currentBoost / maxBoost);
    }

    // --- BOOST MANAGEMENT ---

    /// Adds default boost amount.
    public void AddBoost()
    {
        AddBoost(boostGain);
    }

    /// Adds specified boost amount clamped to max.
    public void AddBoost(float boostReward)
    {
        currentBoost += boostReward;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }

    /// Removes specified boost amount clamped to zero.
    public void RemoveBoost(float amount)
    {
        currentBoost -= amount;
        currentBoost = Mathf.Clamp(currentBoost, 0, maxBoost);
    }
}