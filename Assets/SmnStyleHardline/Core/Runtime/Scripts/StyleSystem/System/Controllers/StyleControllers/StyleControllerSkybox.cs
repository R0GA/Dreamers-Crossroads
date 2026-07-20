namespace SmnStyleHardline.Core
{
    /*
 * Style Controller – Skybox
 * Applies skybox-related style data to global shader parameters,
 * controlling colors, bands, and procedural starmap settings.
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class StyleControllerSkybox
{
    // ======================================================================
    // Data Reference
    // ======================================================================

    SSHSystemAsset asset;


    // ======================================================================
    // Shader Property Names
    // ======================================================================

    const string SKY_COLOR_BASE   = "_SSH_Sky_Color_Base";
    const string SKY_COLOR_ACCENT = "_SSH_Sky_Color_Accent";
    const string SKY_COLOR_STARS  = "_SSH_Sky_Color_Stars";

    const string SKY_BAND_INTENSITY = "_SSH_Sky_Band_Intensity";
    const string SKY_BAND_ADD       = "_SSH_Sky_Band_Add";
    const string SKY_BAND_HOR_WIDTH = "_SSH_Sky_Band_HorWidth";
    const string SKY_BAND_CONTRAST  = "_SSH_Sky_Band_Contrast";

    const string SKY_STARMAP_DENSITY  = "_SSH_Sky_StarmapDensity";
    const string SKY_STARMAP_SCALE    = "_SSH_Sky_StarmapScale";
    const string SKY_STARMAP_SEED     = "_SSH_Sky_StarmapSeed";
    const string SKY_STARMAP_TILT = "_SSH_Sky_StarmapTilt";


    // ======================================================================
    // Initialization
    // ======================================================================

    /// <summary>
    /// Binds the system asset and applies all skybox values to shaders.
    /// </summary>
    public void Init(SSHSystemAsset systemAsset)
    {
        asset = systemAsset;
        ApplyAll();
    }

    void ApplyAll()
    {
        if (asset == null)
            return;

        var s = asset.Skybox;

        Shader.SetGlobalColor(SKY_COLOR_BASE, s.BaseColor);
        Shader.SetGlobalColor(SKY_COLOR_ACCENT, s.AccentColor);
        Shader.SetGlobalColor(SKY_COLOR_STARS, s.StarsColor);

        Shader.SetGlobalFloat(SKY_BAND_INTENSITY, s.BandIntensity);
        Shader.SetGlobalFloat(SKY_BAND_ADD, s.BandAdd);
        Shader.SetGlobalFloat(SKY_BAND_HOR_WIDTH, s.BandHorizontalWidth);
        Shader.SetGlobalFloat(SKY_BAND_CONTRAST, s.BandContrast);

        Shader.SetGlobalFloat(SKY_STARMAP_DENSITY, s.StarmapDensity);
        Shader.SetGlobalFloat(SKY_STARMAP_SCALE, s.StarmapScale);
        Shader.SetGlobalFloat(SKY_STARMAP_SEED, s.StarmapSeed);
        Shader.SetGlobalFloat(SKY_STARMAP_TILT, s.StarmapTilt);
    }


    // ======================================================================
    // Editor Utilities
    // ======================================================================

#if UNITY_EDITOR
    static void MarkDirty(SSHSystemAsset asset)
    {
        if (asset != null)
            EditorUtility.SetDirty(asset);
    }
#else
    static void MarkDirty(SSHSystemAsset asset) { }
#endif


    // ======================================================================
    // Color Controls
    // ======================================================================

    public void SetBaseColor(Color v)
    {
        asset.Skybox.BaseColor = v;
        Shader.SetGlobalColor(SKY_COLOR_BASE, v);
        MarkDirty(asset);
    }

    public Color GetBaseColor()
        => asset.Skybox.BaseColor;

    public void SetAccentColor(Color v)
    {
        asset.Skybox.AccentColor = v;
        Shader.SetGlobalColor(SKY_COLOR_ACCENT, v);
        MarkDirty(asset);
    }

    public Color GetAccentColor()
        => asset.Skybox.AccentColor;

    public void SetStarsColor(Color v)
    {
        asset.Skybox.StarsColor = v;
        Shader.SetGlobalColor(SKY_COLOR_STARS, v);
        MarkDirty(asset);
    }

    public Color GetStarsColor()
        => asset.Skybox.StarsColor;


    // ======================================================================
    // Band Controls
    // ======================================================================

    public void SetBandIntensity(float v)
    {
        asset.Skybox.BandIntensity = v;
        Shader.SetGlobalFloat(SKY_BAND_INTENSITY, v);
        MarkDirty(asset);
    }

    public float GetBandIntensity()
        => asset.Skybox.BandIntensity;

    public void SetBandAdd(float v)
    {
        asset.Skybox.BandAdd = v;
        Shader.SetGlobalFloat(SKY_BAND_ADD, v);
        MarkDirty(asset);
    }

    public float GetBandAdd()
        => asset.Skybox.BandAdd;

    public void SetBandHorizontalWidth(float v)
    {
        asset.Skybox.BandHorizontalWidth = v;
        Shader.SetGlobalFloat(SKY_BAND_HOR_WIDTH, v);
        MarkDirty(asset);
    }

    public float GetBandHorizontalWidth()
        => asset.Skybox.BandHorizontalWidth;

    public void SetBandContrast(float v)
    {
        asset.Skybox.BandContrast = v;
        Shader.SetGlobalFloat(SKY_BAND_CONTRAST, v);
        MarkDirty(asset);
    }

    public float GetBandContrast()
        => asset.Skybox.BandContrast;


    // ======================================================================
    // Starmap Controls
    // ======================================================================

    public void SetStarmapDensity(float v)
    {
        asset.Skybox.StarmapDensity = v;
        Shader.SetGlobalFloat(SKY_STARMAP_DENSITY, v);
        MarkDirty(asset);
    }

    public float GetStarmapDensity()
        => asset.Skybox.StarmapDensity;

    public void SetStarmapScale(float v)
    {
        asset.Skybox.StarmapScale = v;
        Shader.SetGlobalFloat(SKY_STARMAP_SCALE, v);
        MarkDirty(asset);
    }

    public float GetStarmapScale()
        => asset.Skybox.StarmapScale;

    public void SetStarmapSeed(float v)
    {
        asset.Skybox.StarmapSeed = v;
        Shader.SetGlobalFloat(SKY_STARMAP_SEED, v);
        MarkDirty(asset);
    }

    public float GetStarmapSeed()
        => asset.Skybox.StarmapSeed;

    public void SetStarmapTilt(float v)
    {
        asset.Skybox.StarmapTilt = v;
        Shader.SetGlobalFloat(SKY_STARMAP_TILT, v);
        MarkDirty(asset);
    }

    public float GetStarmapTilt()
        => asset.Skybox.StarmapTilt;
}

}