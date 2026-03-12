using UnityEngine;
using UnityEngine.Rendering.Universal;

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
}