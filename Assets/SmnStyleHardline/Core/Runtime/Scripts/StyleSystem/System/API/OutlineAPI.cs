namespace SmnStyleHardline.Core
{
    /*
 * Outline API
 * Exposes runtime control over all outline rendering parameters,
 * including color and geometry-based outlines.
 */

using UnityEngine;


// ==========================================================================
// Outline API
// ==========================================================================

public sealed class OutlineAPI
{
    readonly StyleControllerOutline style;

    internal OutlineAPI(StyleControllerOutline styleOutline)
    {
        style = styleOutline;
    }


    // ======================================================================
    // Color Outline
    // ======================================================================

    /// <summary>
    /// Sets the color used for color-based outlines.
    /// </summary>
    public void SetColorOutlineColor(Color v)
        => style.SetColorOutlineColor(v);

    /// <summary>
    /// Returns the color used for color-based outlines.
    /// </summary>
    public Color GetColorOutlineColor()
        => style.GetColorOutlineColor();

    /// <summary>
    /// Sets the strength of the color-based outline effect.
    /// <para>Valid range: <c>0.05 – 3.0</c></para>
    /// </summary>
    public void SetColorOutlineStrength(float v)
        => style.SetColorOutlineStrength(v);

    /// <summary>
    /// Returns the strength of the color-based outline effect.
    /// <para>Range: <c>0.05 – 3.0</c></para>
    /// </summary>
    public float GetColorOutlineStrength()
        => style.GetColorOutlineStrength();

    /// <summary>
    /// Sets the noise scale applied to color outlines.
    /// <para>Valid range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public void SetColorOutlineNoiseScale(float v)
        => style.SetColorOutlineNoiseScale(v);

    /// <summary>
    /// Returns the noise scale applied to color outlines.
    /// <para>Range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public float GetColorOutlineNoiseScale()
        => style.GetColorOutlineNoiseScale();

    /// <summary>
    /// Sets the noise intensity applied to color outlines.
    /// <para>Valid range: <c>0.0 – 5.0</c></para>
    /// </summary>
    public void SetColorOutlineNoiseIntensity(float v)
        => style.SetColorOutlineNoiseIntensity(v);

    /// <summary>
    /// Returns the noise intensity applied to color outlines.
    /// <para>Range: <c>0.0 – 5.0</c></para>
    /// </summary>
    public float GetColorOutlineNoiseIntensity()
        => style.GetColorOutlineNoiseIntensity();

    /// <summary>
    /// Sets the additive noise contribution for color outlines.
    /// <para>Valid range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public void SetColorOutlineNoiseAdd(float v)
        => style.SetColorOutlineNoiseAdd(v);

    /// <summary>
    /// Returns the additive noise contribution for color outlines.
    /// <para>Range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public float GetColorOutlineNoiseAdd()
        => style.GetColorOutlineNoiseAdd();

    /// <summary>
    /// Sets the clipping threshold for color outlines.
    /// <para>Valid range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public void SetColorOutlineClip(float v)
        => style.SetColorOutlineClip(v);

    /// <summary>
    /// Returns the clipping threshold for color outlines.
    /// <para>Range: <c>0.0 – 1.0</c></para>
    /// </summary>
    public float GetColorOutlineClip()
        => style.GetColorOutlineClip();


    // ======================================================================
    // Geometry Outline
    // ======================================================================

    /// <summary>
    /// Sets the color used for geometry-based outlines.
    /// </summary>
    public void SetGeoOutlineColor(Color v)
        => style.SetGeoOutlineColor(v);

    /// <summary>
    /// Returns the color used for geometry-based outlines.
    /// </summary>
    public Color GetGeoOutlineColor()
        => style.GetGeoOutlineColor();

    /// <summary>
    /// Sets the normal-based outline strength.
    /// <para>Valid range: <c>0.4 – 4.0</c></para>
    /// </summary>
    public void SetGeoOutlineNormalStrength(float v)
        => style.SetGeoOutlineNormalStrength(v);

    /// <summary>
    /// Returns the normal-based outline strength.
    /// <para>Range: <c>0.4 – 4.0</c></para>
    /// </summary>
    public float GetGeoOutlineNormalStrength()
        => style.GetGeoOutlineNormalStrength();

    /// <summary>
    /// Sets the cutoff mask strength for geometry outlines.
    /// <para>Valid range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public void SetGeoOutlineNormalCutoffMaskStrength(float v)
        => style.SetGeoOutlineNormalCutoffMaskStrength(v);

    /// <summary>
    /// Returns the cutoff mask strength for geometry outlines.
    /// <para>Range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public float GetGeoOutlineNormalCutoffMaskStrength()
        => style.GetGeoOutlineNormalCutoffMaskStrength();

    /// <summary>
    /// Sets the cutoff mask additive offset for geometry outlines.
    /// <para>Valid range: <c>-10.0 – 10.0</c></para>
    /// </summary>
    public void SetGeoOutlineNormalCutoffMaskAdd(float v)
        => style.SetGeoOutlineNormalCutoffMaskAdd(v);

    /// <summary>
    /// Returns the cutoff mask additive offset for geometry outlines.
    /// <para>Range: <c>-10.0 – 10.0</c></para>
    /// </summary>
    public float GetGeoOutlineNormalCutoffMaskAdd()
        => style.GetGeoOutlineNormalCutoffMaskAdd();

    /// <summary>
    /// Sets the starting depth strength for geometry outlines.
    /// <para>Valid range: <c>0.04 – 10.0</c></para>
    /// </summary>
    public void SetGeoOutlineDepthStrengthStart(float v)
        => style.SetGeoOutlineDepthStrengthStart(v);

    /// <summary>
    /// Returns the starting depth strength for geometry outlines.
    /// <para>Range: <c>0.04 – 10.0</c></para>
    /// </summary>
    public float GetGeoOutlineDepthStrengthStart()
        => style.GetGeoOutlineDepthStrengthStart();

    /// <summary>
    /// Sets the ending depth strength for geometry outlines.
    /// <para>Valid range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public void SetGeoOutlineDepthStrengthEnd(float v)
        => style.SetGeoOutlineDepthStrengthEnd(v);

    /// <summary>
    /// Returns the ending depth strength for geometry outlines.
    /// <para>Range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public float GetGeoOutlineDepthStrengthEnd()
        => style.GetGeoOutlineDepthStrengthEnd();

    /// <summary>
    /// Sets the additive depth contribution for geometry outlines.
    /// <para>Valid range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public void SetGeoOutlineDepthAdd(float v)
        => style.SetGeoOutlineDepthAdd(v);

    /// <summary>
    /// Returns the additive depth contribution for geometry outlines.
    /// <para>Range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public float GetGeoOutlineDepthAdd()
        => style.GetGeoOutlineDepthAdd();

    /// <summary>
    /// Sets the depth multiplier for geometry outlines.
    /// <para>Valid range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public void SetGeoOutlineDepthMult(float v)
        => style.SetGeoOutlineDepthMult(v);

    /// <summary>
    /// Returns the depth multiplier for geometry outlines.
    /// <para>Range: <c>0.0 – 10.0</c></para>
    /// </summary>
    public float GetGeoOutlineDepthMult()
        => style.GetGeoOutlineDepthMult();
}

}