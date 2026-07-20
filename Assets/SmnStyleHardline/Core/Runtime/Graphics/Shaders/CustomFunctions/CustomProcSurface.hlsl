#ifndef SMN_CUSTOM_PROC_SURFACE_INCLUDED
#define SMN_CUSTOM_PROC_SURFACE_INCLUDED

// ============================================================
// COMPILER / PIPELINE INCLUDES
// ============================================================
// (None required — math-only utilities)

// ============================================================
// SHADER GRAPH PREVIEW SAFEGUARDS
// ============================================================
// No SHADERGRAPH_PREVIEW guards required (pure math / noise)

// ============================================================
// INTERNAL / HELPER FUNCTIONS
// ============================================================

float SMN_Internal_Hash2D(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float SMN_Internal_ValueNoise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = SMN_Internal_Hash2D(i);
    float b = SMN_Internal_Hash2D(i + float2(1, 0));
    float c = SMN_Internal_Hash2D(i + float2(0, 1));
    float d = SMN_Internal_Hash2D(i + float2(1, 1));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(a, b, u.x),
                lerp(c, d, u.x), u.y);
}

float SMN_Internal_BandMask1D(float coord, float density, float thickness)
{
    float b = frac(coord * density);
    return step(b, thickness);
}

// ============================================================
// EXPOSED SHADER GRAPH FUNCTIONS
// ============================================================

void SMN_SG_CliffStriationHorizontal_float(
    float3 WorldPos,
    float  LineDensity,
    float  BandThickness,
    float  JitterStrength,
    float  JitterScale,
    float  BreakScale,
    float  BreakThreshold,
    out float OutMask)
{
    float v = WorldPos.y;
    float jitter = SMN_Internal_ValueNoise2D(WorldPos.xz * JitterScale) * JitterStrength;
    v += jitter;

    float bands = frac(v * LineDensity);
    float mask = step(bands, BandThickness);

    float breakA = SMN_Internal_ValueNoise2D(WorldPos.xz * BreakScale);
    float breakB = SMN_Internal_ValueNoise2D(WorldPos.xy * BreakScale * 0.75);

    mask *= step(BreakThreshold, breakA * breakB);
    OutMask = mask;
}

void SMN_SG_CliffStriationVertical_float(
    float3 WorldPos,
    float3 WorldNormal,
    float  LineDensity,
    float  BandThickness,
    float  JitterStrength,
    float  JitterScale,
    float  BreakScale,
    float  BreakThreshold,
    out float OutMask)
{
    float jitter = SMN_Internal_ValueNoise2D(WorldPos.xz * JitterScale) * JitterStrength;

    float maskX = step(frac((WorldPos.x + jitter) * LineDensity), BandThickness);
    float maskZ = step(frac((WorldPos.z + jitter) * LineDensity), BandThickness);

    float breakup =
        SMN_Internal_ValueNoise2D(WorldPos.xz * BreakScale) *
        SMN_Internal_ValueNoise2D(WorldPos.xy * BreakScale) *
        SMN_Internal_ValueNoise2D(WorldPos.yz * BreakScale);

    float breakMask = step(BreakThreshold, breakup);
    maskX *= breakMask;
    maskZ *= breakMask;

    OutMask = (abs(WorldNormal.z) > abs(WorldNormal.x)) ? maskX : maskZ;
}

#endif // SMN_CUSTOM_PROC_SURFACE_INCLUDED
