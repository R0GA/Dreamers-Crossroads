#ifndef SMN_CUSTOM_STYLIZED_LIGHTING_INCLUDED
#define SMN_CUSTOM_STYLIZED_LIGHTING_INCLUDED

// ============================================================
// COMPILER / PIPELINE INCLUDES
// ============================================================
// (None required — math-only utilities)

// ============================================================
// SHADER GRAPH PREVIEW SAFEGUARDS
// ============================================================
// No SHADERGRAPH_PREVIEW guards required (pure math / lighting)

// ============================================================
// INTERNAL / HELPER FUNCTIONS
// ============================================================

float SMN_Internal_Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float SMN_Internal_Noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = SMN_Internal_Hash21(i);
    float b = SMN_Internal_Hash21(i + float2(1, 0));
    float c = SMN_Internal_Hash21(i + float2(0, 1));
    float d = SMN_Internal_Hash21(i + float2(1, 1));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(a, b, u.x),
                lerp(c, d, u.x), u.y);
}

// ============================================================
// EXPOSED SHADER GRAPH FUNCTIONS
// ============================================================

void SMN_SG_StylizedSpec_float(
    float3 NormalWS,
    float3 ViewDirWS,
    float3 LightDirWS,
    float  Push,
    float  ViewSuppress,
    float  Intensity,
    float  Threshold,
    out float SpecMask)
{
    float3 N = normalize(NormalWS);
    float3 V = normalize(ViewDirWS);
    float3 L = normalize(LightDirWS);

    // ---- Rim Amount ----
    float rim = 1.0 - saturate(dot(N, V));

    // ---- Warp Normal Toward Silhouette ----
    float3 warpedN = normalize(lerp(N, V, Push * rim));

    // ---- Reflect Light Around Warped Normal ----
    float3 R = reflect(-L, warpedN);

    float spec = abs(dot(R, V));

    // ---- Suppress When Facing Camera Directly ----
    float facing   = saturate(dot(N, V));
    float viewMask = pow(1.0 - facing, ViewSuppress);

    spec *= viewMask;
    spec *= Intensity;

    SpecMask = step(Threshold, spec);
}

void SMN_SG_PlanarHighlightStreak_float(
    float3 WorldPos,
    float3 Normal,
    float3 LightDir,
    float3 CameraPos,
    float3 ViewDir,
    float  StreakScale,
    float  BaseWidth,
    float  Softness,
    float  VariationAmount,
    float  NoiseScale,
    float  CameraOffsetScale,
    float  CameraLineMoveScale,
    float  TimeValue,
    float  Seed,
    out float StreakMask)
{
    float3 L = normalize(LightDir);
    float3 N = normalize(Normal);

    // ---- Base Projection ----
    float projected = dot(WorldPos, L);

    // Seed shifts projection phase (keeps lines parallel)
    float streakCoord = projected * StreakScale + Seed * 17.13;

    // ---- Camera-Based Parallel Shift (No Shear) ----
    float cameraShift = dot(CameraPos, L) * CameraOffsetScale;
    streakCoord += cameraShift;

    float cameraLineMove = dot(CameraPos * 0.1, L) * CameraLineMoveScale;
    streakCoord += cameraLineMove;

    // ---- Variation / Wobble ----
    float2 noiseDomain = WorldPos.xz * NoiseScale + Seed * 23.7;
    float wobble = SMN_Internal_Noise2D(noiseDomain + TimeValue * 0.1);
    streakCoord += (wobble - 0.5) * VariationAmount;

    float bands    = frac(streakCoord);
    float centered = abs(bands - 0.5) * 2.0;

    // ---- Width Variation Per Streak (Seeded) ----
    float widthVariation =
        SMN_Internal_Noise2D(float2(floor(streakCoord) + Seed * 11.0, 0.0));

    float finalWidth = BaseWidth * lerp(0.6, 1.4, widthVariation);

    float streak =
        1.0 - smoothstep(finalWidth, finalWidth + Softness, centered);

    // ---- Brightness Breakup (Seeded) ----
    float brightnessNoise =
        SMN_Internal_Noise2D(noiseDomain * 0.5 + Seed * 5.3);

    streak *= lerp(0.7, 1.2, brightnessNoise);

    StreakMask = saturate(streak);
}

#endif // SMN_CUSTOM_STYLIZED_LIGHTING_INCLUDED