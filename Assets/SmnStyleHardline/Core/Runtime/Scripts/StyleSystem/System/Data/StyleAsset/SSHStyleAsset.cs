namespace SmnStyleHardline.Core
{
    /*
 * SSH Style Asset
 * Serializable preset container defining a complete visual style
 * that can be applied wholesale to all style controllers.
 */

    using System;
    using UnityEngine;


    // ==========================================================================
    // Style Preset Asset
    // ==========================================================================

    [CreateAssetMenu(
        fileName = "SSHStyleAsset",
        menuName = "SSH/Style Asset"
    )]
    public sealed class SSHStyleAsset : ScriptableObject
    {
        // ----- Style Blocks ----- //

        [Header("Style Blocks")]
        public SSHStyleEnvironmentData Environment;
        public SSHStyleSkyboxData Skybox;
        public SSHStyleOutlineData Outline;
    }


    // ==========================================================================
    // Environment Style Data (Preset)
    // ==========================================================================

    /// <summary>
    /// Snapshot-style environment configuration used when applying presets.
    /// </summary>
    [Serializable]
    public sealed class SSHStyleEnvironmentData
    {
        // ----- Shadows ----- //

        [Header("Shadows")]
        public Color ShadowColor = Color.black;
        public float ShadowIntensity = 1f;
        public float ShadowClip = 0f;
        public float ShadowRangeMult = 1f;

        // ----- Environment Light ----- //

        [Header("Environment Light")]
        public Color EnvironmentLightColor = Color.white;
        public float EnvironmentLightIntensity = 1f;
        public float EnvironmentLightInfluence = 1f;
    }


    // ==========================================================================
    // Outline Style Data (Preset)
    // ==========================================================================

    /// <summary>
    /// Preset outline configuration for both color-based and geometry-based outlines.
    /// </summary>
    [Serializable]
    public sealed class SSHStyleOutlineData
    {
        // ----- Color Outline ----- //

        [Header("Color Outline")]
        public Color ColorOutlineColor = Color.black;
        public float ColorOutlineStrength = 1f;
        public float ColorOutlineNoiseScale = 1f;
        public float ColorOutlineNoiseIntensity = 1f;
        public float ColorOutlineNoiseAdd = 0f;
        public float ColorOutlineClip = 0f;

        // ----- Geo Outline ----- //

        [Header("Geo Outline")]
        public Color GeoOutlineColor = Color.black;
        public float GeoOutlineNormalStrength = 1f;
        public float GeoOutlineNormalCutoffMaskStrength = 1f;
        public float GeoOutlineNormalCutoffMaskAdd = 0f;

        public float GeoOutlineDepthStrengthStart = 0f;
        public float GeoOutlineDepthStrengthEnd = 1f;
        public float GeoOutlineDepthAdd = 0f;
        public float GeoOutlineDepthMult = 1f;
    }


    // ==========================================================================
    // Skybox Style Data (Preset)
    // ==========================================================================

    /// <summary>
    /// Preset skybox configuration defining colors, bands, and starmap parameters.
    /// </summary>
    [Serializable]
    public sealed class SSHStyleSkyboxData
    {
        // ----- Colors ----- //

        [Header("Colors")]
        public Color BaseColor = Color.black;
        public Color AccentColor = Color.white;
        public Color StarsColor = Color.white;

        // ----- Bands ----- //

        [Header("Bands")]
        public float BandIntensity = 1f;
        public float BandAdd = 0f;
        public float BandHorizontalWidth = 1f;
        public float BandContrast = 1f;

        // ----- Starmap ----- //

        [Header("Starmap")]
        public float StarmapDensity = 1f;
        public float StarmapScale = 1f;
        public float StarmapSeed = 0f;
        public float StarmapTilt = 1f;
    }

}