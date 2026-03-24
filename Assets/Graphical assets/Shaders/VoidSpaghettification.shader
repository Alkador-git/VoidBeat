Shader "URP/VoidSpaghettificationPro"
{
    Properties
    {
        _CenterY("Center Y (Height)", Range(0, 1)) = 0.5
        _StretchIntensity("Stretch Intensity", Range(0, 1)) = 0
        _StretchCurvature("Stretch Curvature", Range(0, 5)) = 2.0
        _AberrationIntensity("Aberration Intensity", Range(0, 1)) = 0
        _DesatIntensity("Desaturation Intensity", Range(0, 1)) = 0
        _BlackHoleRadius("Event Horizon Size", Range(0, 0.5)) = 0.1
        _FlareIntensity("Flare Intensity", Range(0, 2)) = 0
        _FlareColor("Flare Color", Color) = (1, 0, 1, 1)
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

            float _CenterY;
            float _StretchIntensity, _StretchCurvature, _AberrationIntensity;
            float _DesatIntensity, _BlackHoleRadius, _FlareIntensity;
            float4 _FlareColor;

            // Calcule l'effet de spaghettification : étirement, aberration chromatique et décoloration
            float4 Frag(Varyings input) : SV_Target
            {
                float2 uvOrig = input.texcoord;
                float2 center = float2(0.0, _CenterY);
                float2 dir = uvOrig - center;
                float dist = length(dir);

                // Étire les pixels vers le trou noir
                float2 distortedUV = uvOrig;
                float curve = pow(saturate(1.0 - dist), _StretchCurvature);
                distortedUV -= dir * curve * _StretchIntensity * 0.5;

                // Applique l'aberration chromatique
                float shift = _AberrationIntensity * 0.05;
                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV + float2(shift, 0)).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedUV - float2(shift, 0)).b;
                float3 col = float3(r, g, b);

                // Désature les couleurs
                float3 grayscale = Luminance(col).xxx;
                float3 finalColor = lerp(col, grayscale, _DesatIntensity);

                // Ajoute un éclat lumineux
                float flare = smoothstep(0.8, 0.0, dist) * _FlareIntensity;
                finalColor += _FlareColor.rgb * flare;

                // Rend l'horizon des événements complètement noir
                if (dist < _BlackHoleRadius * _StretchIntensity)
                {
                    finalColor = float3(0, 0, 0);
                }
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}