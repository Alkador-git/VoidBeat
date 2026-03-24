Shader "URP/VoidBorderLensing"
{
    Properties
    {
        _CenterY("Center Y (Height)", Range(0, 1)) = 0.5
        _Intensity("Global Danger", Range(0, 1)) = 0
        _BlackHoleRadius("Radius", Range(0, 0.5)) = 0.15
        _LensingThickness("Lensing Border Thickness", Range(0, 0.2)) = 0.05
        _LensingStrength("Lensing Strength", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _CenterY, _Intensity, _BlackHoleRadius, _LensingThickness, _LensingStrength;

            // Calcule l'effet de lentille gravitationnelle sur les bords du trou noir
            float4 Frag(Varyings input) : SV_Target
            {
                float2 center = float2(0.0, _CenterY);
                float2 uv = input.texcoord;
                float2 dir = uv - center;
                float dist = length(dir);

                float currentRadius = _BlackHoleRadius * _Intensity; 
                float lensingOuterLimit = currentRadius + _LensingThickness;

                // Applique la distorsion sur la bordure de lentille
                if (dist > currentRadius && dist < lensingOuterLimit) 
                {
                    float borderWeight = 1.0 - ((dist - currentRadius) / _LensingThickness);
                    float offset = borderWeight * _LensingStrength * _Intensity;
                    uv -= normalize(dir) * offset;
                }

                float3 screenCol = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                // Noircit le centre du trou noir
                if (dist < currentRadius) screenCol = float3(0, 0, 0); 

                return float4(screenCol, 1.0);
            }
            ENDHLSL
        }
    }
}