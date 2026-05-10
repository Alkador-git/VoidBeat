using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class URPFeatureManager : MonoBehaviour
{
    // --- RENDERER DATA ---

    public Renderer2DData rendererData;

    // --- FEATURE CONTROL ---

    /// Activates or deactivates a renderer feature by name.
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

    // --- EDITOR MODE HANDLING ---

    /// Disables features when entering edit mode.
    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DisableAllFeatures();
#endif
    }

    /// Disables features during editor validation.
    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DisableAllFeatures();
#endif
    }

    // --- FEATURE MANAGEMENT ---

    /// Disables all renderer features.
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