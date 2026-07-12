Shader "Custom/URPPainterly"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Painterly Lighting)]
        _Bands ("Light Bands (Quantize Steps)", Range(2,12)) = 4
        _BandSoftness ("Band Edge Softness", Range(0.001,0.5)) = 0.08
        _ShadowColor ("Shadow Tint", Color) = (0.6,0.55,0.7,1)

        [Header(Brush Strokes)]
        _BrushTex ("Brush Normal/Detail", 2D) = "bump" {}
        _BrushScale ("Brush Tiling", Float) = 8
        _BrushStrength ("Brush Normal Strength", Range(0,2)) = 0.6

        [Header(Canvas Rim)]
        _RimColor ("Rim Color", Color) = (1,0.95,0.85,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
        _RimStrength ("Rim Strength", Range(0,2)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 tangentWS  : TEXCOORD2;
                float3 bitangentWS: TEXCOORD3;
                float3 viewDirWS  : TEXCOORD4;
                float3 positionWS : TEXCOORD5;
            };

            TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
            TEXTURE2D(_BrushTex);  SAMPLER(sampler_BrushTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float4 _RimColor;
                float _Bands;
                float _BandSoftness;
                float _BrushScale;
                float _BrushStrength;
                float _RimPower;
                float _RimStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                OUT.normalWS = normInputs.normalWS;
                OUT.tangentWS = normInputs.tangentWS;
                OUT.bitangentWS = normInputs.bitangentWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Albedo: sampled normally, untouched by stylization ---
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                // --- Brush stroke normal perturbation ---
                half3 brushMap = UnpackNormal(SAMPLE_TEXTURE2D(_BrushTex, sampler_BrushTex, IN.uv * _BrushScale));
                brushMap.xy *= _BrushStrength;

                float3x3 TBN = float3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalize(IN.normalWS));
                float3 normalWS = normalize(mul(brushMap, TBN));

                float3 viewDirWS = normalize(IN.viewDirWS);

                // --- Main light + banding ---
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                half NdotL = dot(normalWS, mainLight.direction) * 0.5 + 0.5;

                half stepped = floor(NdotL * _Bands) / _Bands;
                half frac = NdotL * _Bands - floor(NdotL * _Bands);
                half eased = stepped + smoothstep(0.0, _BandSoftness, frac) / _Bands;

                half3 shadowTint = lerp(_ShadowColor.rgb, half3(1,1,1), eased);
                half3 litColor = albedo.rgb * mainLight.color * mainLight.shadowAttenuation * shadowTint * eased * 2.0;

                // --- Additional lights (simple, non-banded, for fill) ---
                #ifdef _ADDITIONAL_LIGHTS
                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    Light addLight = GetAdditionalLight(i, IN.positionWS);
                    half addNdotL = saturate(dot(normalWS, addLight.direction));
                    litColor += albedo.rgb * addLight.color * addLight.distanceAttenuation * addLight.shadowAttenuation * addNdotL;
                }
                #endif

                // --- Rim / canvas edge glow ---
                half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
                litColor += _RimColor.rgb * pow(rim, _RimPower) * _RimStrength * albedo.rgb;

                return half4(litColor, albedo.a);
            }
            ENDHLSL
        }

        // Needed so the object still casts shadows correctly
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
