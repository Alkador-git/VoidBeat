Shader "URP/VoidBorderLensing"
{
    Properties
    {
        [Header(Base)]
        _Intensity("Global Danger", Range(0, 1)) = 0
        _BlackHoleRadius("Radius", Range(0, 0.5)) = 0.15
        
        [Header(Lensing Settings)]
        _LensingThickness("Lensing Border Thickness", Range(0, 0.2)) = 0.05
        _LensingStrength("Lensing Strength", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "VoidBorderLensingPass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float _BlackHoleRadius;
            float _LensingThickness;
            float _LensingStrength;

            float4 Frag(Varyings input) : SV_Target
            {
                // Position du trou noir sur le bord gauche, représentant la mort
                float2 center = float2(0.0, 0.5); 
                float2 uv = input.texcoord;
                float2 dir = uv - center;
                float dist = length(dir);

                // DÉFINITION DE L'HORIZON ET DE LA BORDURE
                float currentRadius = _BlackHoleRadius * _Intensity; 
                float lensingOuterLimit = currentRadius + _LensingThickness;

                // LOGIQUE DE LENSING ISOLÉE À LA BORDURE
                if (dist > currentRadius && dist < lensingOuterLimit) 
                {
                    // Calcul du poids de la distorsion (maximale à l'horizon)
                    float borderWeight = 1.0 - ((dist - currentRadius) / _LensingThickness);
                    
                    // Déviation des UV pour simuler la gravité du trou noir conscient
                    float offset = borderWeight * _LensingStrength * _Intensity;
                    uv -= normalize(dir) * offset;
                }

                // ÉCHANTILLONNAGE DE L'IMAGE
                float3 screenCol = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // RENDU DE L'INTÉRIEUR DU TROU NOIR (Néant absolu) 
                if (dist < currentRadius) 
                {
                    screenCol = float3(0, 0, 0); 
                }

                return float4(screenCol, 1.0);
            }
            ENDHLSL
        }
    }
}