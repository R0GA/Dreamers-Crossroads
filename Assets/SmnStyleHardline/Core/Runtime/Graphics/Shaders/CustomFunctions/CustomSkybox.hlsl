#ifndef SMN_CUSTOM_SKYBOX_INCLUDED
#define SMN_CUSTOM_SKYBOX_INCLUDED

// ============================================================
// COMPILER / PIPELINE INCLUDES
// ============================================================

#ifndef SHADERGRAPH_PREVIEW
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

// ============================================================
// SHADER GRAPH PREVIEW SAFEGUARDS
// ============================================================
// None required (math-only view direction operations)

// ============================================================
// INTERNAL / HELPER FUNCTIONS
// ============================================================
// (None)

// ============================================================
// EXPOSED SHADER GRAPH FUNCTIONS
// ============================================================

void SMN_SG_SkyboxHorizonBands_float(
    float3 ViewDirWS,
    float Intensity,
    float Add,
    float HorizonWidth,
    float Contrast,
    out float Mask)
{
    float3 dir = normalize(ViewDirWS);
    float w = 1.0 - smoothstep(0.0, max(HorizonWidth, 1e-5), abs(dir.y));
    w = pow(saturate(w), max(Contrast, 1e-5));

    float h = dir.y * 0.5 + 0.5 + Add;
    float x = frac(h * max(Intensity, 1e-4));
    Mask = (1.0 - abs(x * 2.0 - 1.0)) * w;
}

void SMN_SG_SkyboxStarmap_float(
    float3 ViewDirWS,
    float Density,
    float Scale,
    float Intensity,
    float Seed,
    out float Mask)
{
    float3 dir = normalize(ViewDirWS);
    float2 uv = float2(
        atan2(dir.z, dir.x) * (1.0 / (2.0 * PI)) + 0.5,
        acos(dir.y) * (1.0 / PI)
    );

    float2 p = uv * max(Density, 1e-4) + Seed;
    float rnd = frac(sin(dot(floor(p), float2(127.1, 311.7))) * 43758.5453);

    float star = smoothstep(Scale, 0.0, length(frac(p) - 0.5));
    Mask = step(1.0 - saturate(Intensity), rnd) * step(0.3, star);
}

#endif // SMN_CUSTOM_SKYBOX_INCLUDED
