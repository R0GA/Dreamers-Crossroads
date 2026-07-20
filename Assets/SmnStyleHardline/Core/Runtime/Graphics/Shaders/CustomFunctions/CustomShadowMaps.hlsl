#ifndef SMN_CUSTOM_SHADOW_MAPS_INCLUDED
#define SMN_CUSTOM_SHADOW_MAPS_INCLUDED

// ============================================================
// COMPILER / PIPELINE INCLUDES
// ============================================================

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#else
TEXTURE2D(_ScreenSpaceShadowmapTexture);
SAMPLER(sampler_ScreenSpaceShadowmapTexture);
#endif

// ============================================================
// SHADER GRAPH PREVIEW SAFEGUARDS
// ============================================================
// Required: screen-space shadows and additional light data unavailable in preview

// ============================================================
// INTERNAL / HELPER FUNCTIONS
// ============================================================

inline float SMN_Internal_DirectionalSSShadowSample(float4 screenPosRaw, float3 positionWS, float clipValue)
{
    #if SHADERGRAPH_PREVIEW
    return 1.0;
    #else
    float2 uv = screenPosRaw.xy / screenPosRaw.w;

    #if defined(UNITY_SINGLE_PASS_STEREO)
    uv = UnityStereoTransformScreenSpaceTex(uv);
    #endif

    if (any(uv < 0.0) || any(uv > 1.0))
        return 1.0;

    float ssShadow = SAMPLE_TEXTURE2D(
    _ScreenSpaceShadowmapTexture,
    sampler_ScreenSpaceShadowmapTexture,
    uv
    ).r;

    float shadowFade = GetMainLightShadowFade(positionWS);
    ssShadow = lerp(ssShadow, 1.0, shadowFade);

    return step(clipValue, ssShadow);
    #endif
}


inline float SMN_Internal_AddLightRangeMask(float3 worldPos, float4 light, float rangeMult)
{
    float3 d = worldPos - light.xyz;
    float range = light.w * rangeMult;
    return step(dot(d, d), range * range);
}

inline float SMN_Internal_AddLightMaskComposite(
float3 worldPos,
float addLightCount,
float addLightRangeMult,
float4 l0, float4 l1, float4 l2, float4 l3,
float4 l4, float4 l5, float4 l6, float4 l7)
{
    #if SHADERGRAPH_PREVIEW
    return 0.0;
    #else
    float mask = 0.0;
    if (addLightCount > 0) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l0, addLightRangeMult));
        if (addLightCount > 1) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l1, addLightRangeMult));
        if (addLightCount > 2) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l2, addLightRangeMult));
        if (addLightCount > 3) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l3, addLightRangeMult));
        if (addLightCount > 4) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l4, addLightRangeMult));
        if (addLightCount > 5) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l5, addLightRangeMult));
        if (addLightCount > 6) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l6, addLightRangeMult));
        if (addLightCount > 7) mask = max(mask, SMN_Internal_AddLightRangeMask(worldPos, l7, addLightRangeMult));
        return mask;
    #endif
}

// ============================================================
// EXPOSED SHADER GRAPH FUNCTIONS
// ============================================================

void SMN_SG_DirectionalSSShadow_float(
float4 PositionCS,
float3 PositionWS,
float ClipValue,
out float Shadow)
{
    Shadow = SMN_Internal_DirectionalSSShadowSample(PositionCS, PositionWS, ClipValue);
}

void SMN_SG_DirectionalSSShadow_half(
half4 PositionCS,
half3 PositionWS,
half ClipValue,
out half Shadow)
{
    Shadow = (half)SMN_Internal_DirectionalSSShadowSample(
    (float4)PositionCS,
    (float3)PositionWS,
    (float)ClipValue
    );
}

void SMN_SG_AddLightMask_float(
float3 PositionWS, float AddLightCount, float AddLightRangeMult,
float4 L0, float4 L1, float4 L2, float4 L3,
float4 L4, float4 L5, float4 L6, float4 L7,
out float Mask)
{
    Mask = SMN_Internal_AddLightMaskComposite(
    PositionWS, AddLightCount, AddLightRangeMult,
    L0, L1, L2, L3, L4, L5, L6, L7);
}

void SMN_SG_AddLightMask_half(
half3 PositionWS, half AddLightCount, half AddLightRangeMult,
half4 L0, half4 L1, half4 L2, half4 L3,
half4 L4, half4 L5, half4 L6, half4 L7,
out half Mask)
{
    Mask = (half)SMN_Internal_AddLightMaskComposite(
    (float3)PositionWS, (float)AddLightCount, (float)AddLightRangeMult,
    (float4)L0, (float4)L1, (float4)L2, (float4)L3,
    (float4)L4, (float4)L5, (float4)L6, (float4)L7);
}

#endif // SMN_CUSTOM_SHADOW_MAPS_INCLUDED
