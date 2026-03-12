Shader "URP/VoidSpaghettificationPro"
{
    Properties
    {
        [Header(Distortion)]
        _StretchIntensity("Stretch Intensity", Range(0, 1)) = 0
        _StretchCurvature("Stretch Curvature", Range(0, 5)) = 2.0
        
        [Header(Visuals)]
        _AberrationIntensity("Aberration Intensity", Range(0, 1)) = 0
        _DesatIntensity("Desaturation Intensity", Range(0, 1)) = 0
        
        [Header(Neant-X Core)]
        _BlackHoleRadius("Event Horizon Size", Range(0, 0.5)) = 0.1
        _FlareIntensity("Flare Intensity", Range(0, 2)) = 0
        _FlareColor("Flare Color", Color) = (1, 0, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "VoidPassPro"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _StretchIntensity;
            float _StretchCurvature;
            float _AberrationIntensity;
            float _DesatIntensity;
            float _BlackHoleRadius;
            float _FlareIntensity;
            float4 _FlareColor;

            float4 Frag(Varyings input) : SV_Target
            {
                // Coordonnées de base pour le calcul de distance
                float2 uvOrig = input.texcoord;
                float2 center = float2(0.0, 0.5);
                float2 dir = uvOrig - center;
                float dist = length(dir);

                // STRETCH ARRONDI (Appliqué uniquement aux UV de lecture)
                float2 distortedUV = uvOrig;
                float curve = pow(saturate(1.0 - dist), _StretchCurvature);
                distortedUV -= dir * curve * _StretchIntensity * 0.5;

                // ÉCHANTILLONNAGE ET EFFETS (Sur les UV tordus)
                float shift = _AberrationIntensity * 0.05;
                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV + float2(shift, 0)).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV - float2(shift, 0)).b;
                float3 col = float3(r, g, b);

                // Désaturation vers le gris (Neon-Gloom)
                float lum = Luminance(col);
                float3 grayscale = float3(lum, lum, lum);
                float3 finalColor = lerp(col, grayscale, _DesatIntensity);

                // Flare (Halo de la Singularité)
                float flare = smoothstep(0.8, 0.0, dist) * _FlareIntensity;
                finalColor += _FlareColor.rgb * flare;

                // MASQUE DU TROU NOIR (Appliqué à la toute fin)
                if (dist < _BlackHoleRadius * _StretchIntensity)
                {
                    finalColor = float3(45, 0, 45);
                }

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}