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

    [Header("Seuils de Danger par Distance")]
    public float activationDistance = 15f;

    // --- BOUCLE PRINCIPALE ---

    /// Met à jour l'activation et les valeurs des passes URP selon la proximité directe avec le bord droit.
    void Update()
    {
        if (BlackHoleManager.Instance == null || BlackHoleManager.Instance.player == null) return;

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

        bool shouldBeActive = currentIntensity > 0f;

        if (featureManager != null)
        {
            featureManager.SetFeatureActive(lensingFeatureName, shouldBeActive);
            featureManager.SetFeatureActive(spaghettiFeatureName, shouldBeActive);
        }

        if (shouldBeActive)
        {
            UpdateShader(lensingMaterial);
            UpdateShader(spaghettiMaterial);
        }
    }

    // --- AJUSTEMENT DES SHADERS ---

    /// Actualise les paramètres locaux des matériaux de distorsion si nécessaire.
    void UpdateShader(Material mat)
    {
        if (mat == null) return;
    }
}