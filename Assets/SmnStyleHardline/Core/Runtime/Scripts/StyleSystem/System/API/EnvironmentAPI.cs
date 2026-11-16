namespace SmnStyleHardline.Core
{
    /*
 * Environment API
 * Provides runtime access to environment lighting, shadows,
 * and additional light synchronization.
 */

using UnityEngine;


// ==========================================================================
// Environment API
// ==========================================================================

public sealed class EnvironmentAPI
{
    readonly StyleControllerEnvironment style;
    readonly LogicControllerEnvironment logic;

    internal EnvironmentAPI(
        StyleControllerEnvironment styleEnvironment,
        LogicControllerEnvironment logicEnvironment)
    {
        style = styleEnvironment;
        logic = logicEnvironment;
    }


    // ======================================================================
    // Shadows
    // ======================================================================

    /// <summary>
    /// Sets the global shadow color applied by the environment.
    /// </summary>
    public void SetShadowColor(Color v)
        => style.SetShadowColor(v);

    /// <summary>
    /// Returns the current global shadow color.
    /// </summary>
    public Color GetShadowColor()
        => style.GetShadowColor();

    /// <summary>
    /// Sets the intensity multiplier for environment shadows.
    /// <para>Valid range: <c>0.0 – 2.0</c></para>
    /// </summary>
    public void SetShadowIntensity(float v)
        => style.SetShadowIntensity(v);

    /// <summary>
    /// Returns the current shadow intensity multiplier.
    /// <para>Range: <c>0.0 – 2.0</c></para>
    /// </summary>
    public float GetShadowIntensity()
        => style.GetShadowIntensity();

    /// <summary>
    /// Sets the shadow clipping threshold.
    /// <para>Valid range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public void SetShadowClip(float v)
        => style.SetShadowClip(v);

    /// <summary>
    /// Returns the shadow clipping threshold.
    /// <para>Range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public float GetShadowClip()
        => style.GetShadowClip();

    /// <summary>
    /// Sets the distance multiplier used when evaluating shadow range.
    /// <para>Valid range: <c>0.0 – 4.0</c></para>
    /// </summary>
    public void SetShadowRangeMult(float v)
        => style.SetShadowRangeMult(v);

    /// <summary>
    /// Returns the shadow range distance multiplier.
    /// <para>Range: <c>0.0 – 4.0</c></para>
    /// </summary>
    public float GetShadowRangeMult()
        => style.GetShadowRangeMult();


    // ======================================================================
    // Additional Lights
    // ======================================================================

    /// <summary>
    /// Returns the number of additional lights currently uploaded to shaders.
    /// </summary>
    public int GetAdditionalLightCount()
        => style.GetAdditionalLightCount();

    /// <summary>
    /// Returns additional light data at the specified index.
    /// </summary>
    /// <param name="index">Zero-based light slot index.</param>
    /// <returns>
    /// A Vector4 containing position (xyz) and range (w),
    /// or Vector4.zero if the index is invalid.
    /// </returns>
    public Vector4 GetAdditionalLight(int index)
        => style.GetAdditionalLight(index);


    // ======================================================================
    // Environment Light
    // ======================================================================

    /// <summary>
    /// Sets the color of the ambient environment light.
    /// </summary>
    public void SetEnvironmentLightColor(Color v)
        => style.SetEnvironmentLightColor(v);

    /// <summary>
    /// Returns the ambient environment light color.
    /// </summary>
    public Color GetEnvironmentLightColor()
        => style.GetEnvironmentLightColor();

    /// <summary>
    /// Sets the intensity of the ambient environment light.
    /// <para>Valid range: <c>0.0 – 2.0</c></para>
    /// </summary>
    public void SetEnvironmentLightIntensity(float v)
        => style.SetEnvironmentLightIntensity(v);

    /// <summary>
    /// Returns the ambient environment light intensity.
    /// <para>Range: <c>0.0 – 2.0</c></para>
    /// </summary>
    public float GetEnvironmentLightIntensity()
        => style.GetEnvironmentLightIntensity();

    /// <summary>
    /// Sets how strongly environment lighting influences shading.
    /// <para>Valid range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public void SetEnvironmentLightInfluence(float v)
        => style.SetEnvironmentLightInfluence(v);

    /// <summary>
    /// Returns the environment light influence factor.
    /// <para>Range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public float GetEnvironmentLightInfluence()
        => style.GetEnvironmentLightInfluence();


    // ======================================================================
    // Synchronization
    // ======================================================================

    /// <summary>
    /// Forces an immediate rescan and upload of additional light data.
    /// </summary>
    public void RequestImmediateLightSync()
    {
        logic.RequestImmediateLightSync();
    }
}

}