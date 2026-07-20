#ifndef SMN_CUSTOM_SPLASH_RING_INCLUDED
#define SMN_CUSTOM_SPLASH_RING_INCLUDED

// ============================================================
// COMPILER / PIPELINE INCLUDES
// ============================================================

#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

// ============================================================
// SHADER GRAPH PREVIEW SAFEGUARDS
// ============================================================
// Required: time-based animation and noise warping

// ============================================================
// INTERNAL / HELPER FUNCTIONS
// ============================================================

float SMN_Internal_Hash2D_21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float SMN_Internal_GradientNoise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(
        lerp(SMN_Internal_Hash2D_21(i), SMN_Internal_Hash2D_21(i + float2(1, 0)), u.x),
        lerp(SMN_Internal_Hash2D_21(i + float2(0, 1)), SMN_Internal_Hash2D_21(i + float2(1, 1)), u.x),
        u.y
    );
}

// ============================================================
// EXPOSED SHADER GRAPH FUNCTIONS
// ============================================================

void SMN_SG_SplashRing_float(
    float time,
    float2 uv,
    float2 rectSize,
    float cornerRadius,
    float ringSpacing,
    float ringThickness,
    float speed,
    float warpStrength,
    float warpFrequency,
    float warpSpeed,
    float fadeStart,
    float fadeEnd,
    out float mask)
{
#if SHADERGRAPH_PREVIEW
    mask = 1.0;
#else
    float2 p = uv * 2.0 - 1.0;
    float2 flowUV = p * warpFrequency + time * warpSpeed;

    float2 flow = float2(
        SMN_Internal_GradientNoise2D(flowUV),
        SMN_Internal_GradientNoise2D(flowUV + 17.13)
    ) - 0.5;

    float2 wp = p + flow * warpStrength;

    float r = min(cornerRadius, min(rectSize.x, rectSize.y));
    float2 q = abs(wp) - (rectSize - r);

    float sdf = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
    float outwardDist = max(sdf, 0.0);

    float travel = outwardDist - time * speed;
    float ringCoord = abs(frac(travel / ringSpacing) - 0.5);

    float rings = smoothstep(ringThickness, ringThickness * 0.5, ringCoord);
    rings *= 1.0 - smoothstep(fadeStart, fadeEnd, outwardDist);

    mask = saturate((sdf < 0.0) ? 0.0 : rings);
#endif
}

#endif // SMN_CUSTOM_SPLASH_RING_INCLUDED
