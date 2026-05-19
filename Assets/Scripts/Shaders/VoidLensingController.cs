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

    /// Met à jour l'activation des passes URP selon la proximité du trou noir.
    void Update()
    {
        if (BlackHoleManager.Instance == null || BlackHoleManager.Instance.player == null) return;

        float currentDistance = Mathf.Abs(BlackHoleManager.Instance.player.position.x - BlackHoleManager.Instance.transform.position.x);
        bool isDangerous = currentDistance <= activationDistance;

        if (featureManager != null)
        {
            featureManager.SetFeatureActive(lensingFeatureName, isDangerous);
            featureManager.SetFeatureActive(spaghettiFeatureName, isDangerous);
        }

        if (isDangerous)
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