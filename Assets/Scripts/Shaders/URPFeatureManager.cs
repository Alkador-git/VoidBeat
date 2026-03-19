using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class URPFeatureManager : MonoBehaviour
{
    [Header("Référence")]
    public UniversalRendererData rendererData;

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

    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DisableAllFeatures();
#endif
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DisableAllFeatures();
#endif
    }

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