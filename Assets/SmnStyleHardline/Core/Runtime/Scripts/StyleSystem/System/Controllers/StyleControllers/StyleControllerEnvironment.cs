namespace SmnStyleHardline.Core
{
    /*
 * Style Controller – Environment
 * Owns and synchronizes all environment-related shader globals,
 * including shadows, ambient lighting, and additional light data.
 */

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class StyleControllerEnvironment
{
    // ======================================================================
    // Constants
    // ======================================================================

    public const int MaxAdditionalLights = 8;


    // ======================================================================
    // Data Reference
    // ======================================================================

    SSHSystemAsset asset;


    // ======================================================================
    // Shader Property IDs
    // ======================================================================

    static readonly int SHADOW_COLOR      = Shader.PropertyToID("_SSH_Shadow_Color");
    static readonly int SHADOW_INTENSITY  = Shader.PropertyToID("_SSH_Shadow_Intensity");
    static readonly int SHADOW_CLIP       = Shader.PropertyToID("_SSH_Shadow_Clip");
    static readonly int SHADOW_RANGE_MULT = Shader.PropertyToID("_SSH_Shadow_Add_RangeMult");

    static readonly int ENV_COLOR     = Shader.PropertyToID("_SSH_EnvLight_Color");
    static readonly int ENV_INTENSITY = Shader.PropertyToID("_SSH_EnvLight_Intensity");
    static readonly int ENV_INFLUENCE = Shader.PropertyToID("_SSH_EnvLight_Add_Influence");

    static readonly int SHADOW_ADD_COUNT = Shader.PropertyToID("_SSH_Shadow_Add_Count");


    // ======================================================================
    // Initialization
    // ======================================================================

    /// <summary>
    /// Binds the system asset and applies all environment values to shaders.
    /// </summary>
    public void Init(SSHSystemAsset systemAsset)
    {
        asset = systemAsset;
        ApplyAll();
        ClearAdditionalLights();
    }

    void ApplyAll()
    {
        if (asset == null)
            return;

        var env = asset.Environment;

        Shader.SetGlobalColor(SHADOW_COLOR, env.ShadowColor);
        Shader.SetGlobalFloat(SHADOW_INTENSITY, env.ShadowIntensity);
        Shader.SetGlobalFloat(SHADOW_CLIP, env.ShadowClip);
        Shader.SetGlobalFloat(SHADOW_RANGE_MULT, env.ShadowRangeMult);

        Shader.SetGlobalColor(ENV_COLOR, env.EnvironmentLightColor);
        Shader.SetGlobalFloat(ENV_INTENSITY, env.EnvironmentLightIntensity);
        Shader.SetGlobalFloat(ENV_INFLUENCE, env.EnvironmentLightInfluence);
    }


    // ======================================================================
    // Editor Utilities
    // ======================================================================

#if UNITY_EDITOR
    static void MarkDirty(SSHSystemAsset asset)
    {
        if (asset == null)
            return;

        EditorUtility.SetDirty(asset);
    }
#else
    static void MarkDirty(SSHSystemAsset asset) { }
#endif


    // ======================================================================
    // Shadow Controls
    // ======================================================================

    public void SetShadowColor(Color v)
    {
        asset.Environment.ShadowColor = v;
        Shader.SetGlobalColor(SHADOW_COLOR, v);
        MarkDirty(asset);
    }

    public Color GetShadowColor()
        => asset.Environment.ShadowColor;

    public void SetShadowIntensity(float v)
    {
        asset.Environment.ShadowIntensity = v;
        Shader.SetGlobalFloat(SHADOW_INTENSITY, v);
        MarkDirty(asset);
    }

    public float GetShadowIntensity()
        => asset.Environment.ShadowIntensity;

    public void SetShadowClip(float v)
    {
        asset.Environment.ShadowClip = v;
        Shader.SetGlobalFloat(SHADOW_CLIP, v);
        MarkDirty(asset);
    }

    public float GetShadowClip()
        => asset.Environment.ShadowClip;

    public void SetShadowRangeMult(float v)
    {
        asset.Environment.ShadowRangeMult = v;
        Shader.SetGlobalFloat(SHADOW_RANGE_MULT, v);
        MarkDirty(asset);
    }

    public float GetShadowRangeMult()
        => asset.Environment.ShadowRangeMult;


    // ======================================================================
    // Environment Light Controls
    // ======================================================================

    public void SetEnvironmentLightColor(Color v)
    {
        asset.Environment.EnvironmentLightColor = v;
        Shader.SetGlobalColor(ENV_COLOR, v);
        MarkDirty(asset);
    }

    public Color GetEnvironmentLightColor()
        => asset.Environment.EnvironmentLightColor;

    public void SetEnvironmentLightIntensity(float v)
    {
        asset.Environment.EnvironmentLightIntensity = v;
        Shader.SetGlobalFloat(ENV_INTENSITY, v);
        MarkDirty(asset);
    }

    public float GetEnvironmentLightIntensity()
        => asset.Environment.EnvironmentLightIntensity;

    public void SetEnvironmentLightInfluence(float v)
    {
        asset.Environment.EnvironmentLightInfluence = v;
        Shader.SetGlobalFloat(ENV_INFLUENCE, v);
        MarkDirty(asset);
    }

    public float GetEnvironmentLightInfluence()
        => asset.Environment.EnvironmentLightInfluence;


    // ======================================================================
    // Additional Lights
    // ======================================================================

    public void SetAdditionalLight(int index, Vector4 data)
    {
        if (index < 0 || index >= MaxAdditionalLights)
            return;

        Shader.SetGlobalVector($"_SSH_Shadow_Add_Light{index:00}", data);
    }

    public void SetAdditionalLightCount(int count)
    {
        Shader.SetGlobalFloat(SHADOW_ADD_COUNT, count);
    }

    public void ClearAdditionalLights()
    {
        SetAdditionalLightCount(0);

        for (int i = 0; i < MaxAdditionalLights; i++)
            SetAdditionalLight(i, Vector4.zero);
    }

    public int GetAdditionalLightCount()
        => (int)Shader.GetGlobalFloat(SHADOW_ADD_COUNT);

    public Vector4 GetAdditionalLight(int index)
    {
        if (index < 0 || index >= MaxAdditionalLights)
            return Vector4.zero;

        return Shader.GetGlobalVector($"_SSH_Shadow_Add_Light{index:00}");
    }
}

}