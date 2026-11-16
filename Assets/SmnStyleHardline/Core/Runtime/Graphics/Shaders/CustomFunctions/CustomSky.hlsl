#ifndef SMN_CUSTOM_SKY_INCLUDED
#define SMN_CUSTOM_SKY_INCLUDED

// ============================================================
// COMPILER / PIPELINE INCLUDES
// ============================================================
// (Unity globals provided automatically)

// ============================================================
// SHADER GRAPH PREVIEW SAFEGUARDS
// ============================================================
// Safe in preview; uses depth params only

// ============================================================
// INTERNAL / HELPER FUNCTIONS
// ============================================================
// (None)

// ============================================================
// EXPOSED SHADER GRAPH FUNCTIONS
// ============================================================

void SMN_SG_SkyDepthMask_float(
    float DepthSample,
    float DepthThreshold,
    out float OutMask)
{
    float rawDepth = DepthSample;

    #if defined(UNITY_REVERSED_Z)
        rawDepth = 1.0 - rawDepth;
    #endif

    float linearDepth = 1.0 / (_ZBufferParams.x * rawDepth + _ZBufferParams.y);
    OutMask = step(DepthThreshold, linearDepth);
}

#endif // SMN_CUSTOM_SKY_INCLUDED
