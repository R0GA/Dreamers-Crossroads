namespace SmnStyleHardline.Core
{
    /*
 * Logic Controller – Presets
 * Applies style preset data to all active style controllers
 * in a single, coordinated pass.
 */

public class LogicControllerPreset
{
    // ======================================================================
    // Controller References
    // ======================================================================

    StyleControllerEnvironment env;
    StyleControllerSkybox sky;
    StyleControllerOutline outline;


    // ======================================================================
    // Initialization
    // ======================================================================

    /// <summary>
    /// Binds style controllers used when applying preset data.
    /// </summary>
    public void Init(
        StyleControllerEnvironment environment,
        StyleControllerSkybox skybox,
        StyleControllerOutline outlineController)
    {
        env = environment;
        sky = skybox;
        outline = outlineController;
    }


    // ======================================================================
    // Preset Application
    // ======================================================================

    /// <summary>
    /// Applies a complete style asset across environment, skybox, and outlines.
    /// </summary>
    public void Apply(SSHStyleAsset asset)
    {
        if (asset == null)
            return;

        ApplyEnvironment(asset.Environment);
        ApplySkybox(asset.Skybox);
        ApplyOutline(asset.Outline);
    }


    // ======================================================================
    // Environment
    // ======================================================================

    void ApplyEnvironment(SSHStyleEnvironmentData data)
    {
        if (env == null || data == null)
            return;

        env.SetShadowColor(data.ShadowColor);
        env.SetShadowIntensity(data.ShadowIntensity);
        env.SetShadowClip(data.ShadowClip);
        env.SetShadowRangeMult(data.ShadowRangeMult);

        env.SetEnvironmentLightColor(data.EnvironmentLightColor);
        env.SetEnvironmentLightIntensity(data.EnvironmentLightIntensity);
        env.SetEnvironmentLightInfluence(data.EnvironmentLightInfluence);
    }


    // ======================================================================
    // Skybox
    // ======================================================================

    void ApplySkybox(SSHStyleSkyboxData data)
    {
        if (sky == null || data == null)
            return;

        sky.SetBaseColor(data.BaseColor);
        sky.SetAccentColor(data.AccentColor);
        sky.SetStarsColor(data.StarsColor);

        sky.SetBandIntensity(data.BandIntensity);
        sky.SetBandAdd(data.BandAdd);
        sky.SetBandHorizontalWidth(data.BandHorizontalWidth);
        sky.SetBandContrast(data.BandContrast);

        sky.SetStarmapDensity(data.StarmapDensity);
        sky.SetStarmapScale(data.StarmapScale);
        sky.SetStarmapSeed(data.StarmapSeed);
        sky.SetStarmapTilt(data.StarmapTilt);
    }


    // ======================================================================
    // Outline
    // ======================================================================

    void ApplyOutline(SSHStyleOutlineData data)
    {
        if (outline == null || data == null)
            return;

        outline.SetColorOutlineColor(data.ColorOutlineColor);
        outline.SetColorOutlineStrength(data.ColorOutlineStrength);
        outline.SetColorOutlineNoiseScale(data.ColorOutlineNoiseScale);
        outline.SetColorOutlineNoiseIntensity(data.ColorOutlineNoiseIntensity);
        outline.SetColorOutlineNoiseAdd(data.ColorOutlineNoiseAdd);
        outline.SetColorOutlineClip(data.ColorOutlineClip);

        outline.SetGeoOutlineColor(data.GeoOutlineColor);
        outline.SetGeoOutlineNormalStrength(data.GeoOutlineNormalStrength);
        outline.SetGeoOutlineNormalCutoffMaskStrength(
            data.GeoOutlineNormalCutoffMaskStrength);
        outline.SetGeoOutlineNormalCutoffMaskAdd(
            data.GeoOutlineNormalCutoffMaskAdd);

        outline.SetGeoOutlineDepthStrengthStart(
            data.GeoOutlineDepthStrengthStart);
        outline.SetGeoOutlineDepthStrengthEnd(
            data.GeoOutlineDepthStrengthEnd);
        outline.SetGeoOutlineDepthAdd(data.GeoOutlineDepthAdd);
        outline.SetGeoOutlineDepthMult(data.GeoOutlineDepthMult);
    }
}

}