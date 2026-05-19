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

    [Header("Vitesse de lissage de l'effet")]
    public float smoothSpeed = 2f;

    private float currentIntensity = 0f;

    // --- BOUCLE PRINCIPALE ---

    /// Met à jour l'activation et les valeurs progressives des passes URP selon la proximité.
    void Update()
    {
        if (BlackHoleManager.Instance == null || BlackHoleManager.Instance.player == null) return;

        float currentDistance = Mathf.Abs(BlackHoleManager.Instance.player.position.x - BlackHoleManager.Instance.transform.position.x);

        float targetIntensity = 0f;
        if (currentDistance <= activationDistance)
        {
            float range = activationDistance - BlackHoleManager.Instance.deathDistance;
            float clampedDist = Mathf.Clamp(currentDistance, BlackHoleManager.Instance.deathDistance, activationDistance);
            targetIntensity = 1f - ((clampedDist - BlackHoleManager.Instance.deathDistance) / range);
        }

        currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, smoothSpeed * Time.deltaTime);

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