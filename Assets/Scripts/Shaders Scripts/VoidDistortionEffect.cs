using UnityEngine;

public class VoidDistortionController : MonoBehaviour
{
    public Material distortionMaterial;

    [Header("Seuil d'activation")]
    [Range(0f, 1f)] public float activationThreshold = 0.6f;

    [Header("Réglages Spaghettification")]
    [Range(0f, 1f)] public float maxStretch = 1.0f;
    [Range(1f, 5f)] public float stretchCurvature = 2.5f;

    [Header("Réglages Visuels")]
    [Range(0f, 1f)] public float maxAberration = 0.4f;
    [Range(0f, 1f)] public float maxDesaturation = 1.0f;
    [Range(0f, 2f)] public float maxFlare = 1.5f;
    public Color flareColor = new Color(0.7f, 0f, 1f);

    // Met à jour les effets de distorsion en fonction du boost
    void Update()
    {
        if (distortionMaterial == null || BoostManager.Instance == null) return;

        float boostFactor = BoostManager.Instance.currentBoost / BoostManager.Instance.maxBoost;
        float dangerLevel = 1.0f - boostFactor;
        float dangerThreshold = 1.0f - activationThreshold;

        float effectIntensity = 0f;
        if (dangerLevel > dangerThreshold)
        {
            effectIntensity = (dangerLevel - dangerThreshold) / (1.0f - dangerThreshold);
        }

        distortionMaterial.SetFloat("_StretchIntensity", effectIntensity * maxStretch);
        distortionMaterial.SetFloat("_StretchCurvature", stretchCurvature);
        distortionMaterial.SetFloat("_AberrationIntensity", effectIntensity * maxAberration);
        distortionMaterial.SetFloat("_DesatIntensity", effectIntensity * maxDesaturation);
        distortionMaterial.SetFloat("_FlareIntensity", effectIntensity * maxFlare);
        distortionMaterial.SetColor("_FlareColor", flareColor);
    }
}