using UnityEngine;

public class VoidDistortionController : MonoBehaviour
{
    [Header("Matériau")]
    public Material distortionMaterial;

    [Header("Seuils d'activation par Distance")]
    public float activationDistance = 15f;

    [Header("Réglages Spaghettification")]
    [Range(1f, 5f)]
    public float stretchCurvature = 2.5f;

    [Header("Réglages Visuels Maximaux")]
    [Range(0f, 1f)]
    public float maxAberration = 0.4f;
    [Range(0f, 1f)]
    public float maxDesaturation = 1.0f;
    [Range(0f, 2f)]
    public float maxFlare = 1.5f;
    public Color flareColor = new Color(0.7f, 0f, 1f);

    // --- BOUCLE PRINCIPALE ---

    /// Met à jour les modificateurs esthétiques du shader selon la distance directe avec le bord droit.
    void Update()
    {
        if (distortionMaterial == null || BlackHoleManager.Instance == null || BlackHoleManager.Instance.player == null) return;

        float blackHoleRadius = BlackHoleManager.Instance.transform.localScale.x * 0.5f;
        float blackHoleRightEdgeX = BlackHoleManager.Instance.transform.position.x + blackHoleRadius;
        float currentDistance = Mathf.Abs(BlackHoleManager.Instance.player.position.x - blackHoleRightEdgeX);

        float currentIntensity = 0f;
        if (currentDistance <= activationDistance)
        {
            float range = activationDistance - BlackHoleManager.Instance.deathDistance;
            float clampedDist = Mathf.Clamp(currentDistance, BlackHoleManager.Instance.deathDistance, activationDistance);
            currentIntensity = 1f - ((clampedDist - BlackHoleManager.Instance.deathDistance) / range);
        }

        distortionMaterial.SetFloat("_StretchCurvature", stretchCurvature);
        distortionMaterial.SetFloat("_AberrationIntensity", currentIntensity * maxAberration);
        distortionMaterial.SetFloat("_DesatIntensity", currentIntensity * maxDesaturation);
        distortionMaterial.SetFloat("_FlareIntensity", currentIntensity * maxFlare);
        distortionMaterial.SetColor("_FlareColor", flareColor);
    }
}