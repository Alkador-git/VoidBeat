using UnityEngine;

public class VoidLensingController : MonoBehaviour
{
    [Header("Références")]
    public Material lensingMaterial;
    public Material spaghettiMaterial;
    public URPFeatureManager featureManager;

    [Header("Noms des Renderer Features")]
    public string lensingFeatureName = "VoidLensingPass";
    public string spaghettiFeatureName = "VoidSpaghettificationPass";

    [Header("Position & Taille")]
    [Range(0f, 1f)] public float blackHoleHeight = 0.5f;
    [Range(0f, 2f)] public float maxRadius = 0.65f;

    [Header("Seuils de Danger")]
    [Range(0f, 1f)] public float activationThreshold = 0.6f;

    void Update()
    {
        if (BoostManager.Instance == null) return;

        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float dangerLevel = 1.0f - boostFactor;
        float dangerThreshold = 1.0f - activationThreshold;

        float intensity = 0f;
        bool isDangerous = dangerLevel > dangerThreshold;

        if (isDangerous)
        {
            intensity = (dangerLevel - dangerThreshold) / (1.0f - dangerThreshold);
        }

        // Activer/Désactiver les features pour économiser les performances
        if (featureManager != null)
        {
            featureManager.SetFeatureActive(lensingFeatureName, isDangerous);
            featureManager.SetFeatureActive(spaghettiFeatureName, isDangerous);
        }

        // Mettre à jour les shaders seulement si nécessaire
        if (isDangerous)
        {
            UpdateShader(lensingMaterial, intensity);
            UpdateShader(spaghettiMaterial, intensity);
        }
    }

    void UpdateShader(Material mat, float intensity)
    {
        if (mat == null) return;
        mat.SetFloat("_CenterY", blackHoleHeight);
        mat.SetFloat("_Intensity", intensity);
        mat.SetFloat("_BlackHoleRadius", maxRadius * intensity);

    }
}