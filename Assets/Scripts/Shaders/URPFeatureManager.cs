using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class URPFeatureManager : MonoBehaviour
{
    [Header("Référence")]
    public Renderer2DData rendererData;

    // Controle les renderer features
    public void SetFeatureActive(string featureName, bool active)
    {
        if (rendererData == null) return;

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature.name == featureName)
            {
                feature.SetActive(active);
            }
        }
    }

    // Désactive les features lors de l'activation en mode édition
    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DisableAllFeatures();
#endif
    }

    // Désactive les features lors de la validation en mode édition
    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DisableAllFeatures();
#endif
    }

    // Désactive toutes les renderer features
    private void DisableAllFeatures()
    {
        if (rendererData == null) return;

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature == null) continue;
            feature.SetActive(false);
        }
    }
}