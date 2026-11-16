namespace SmnStyleHardline.Core
{
    /*
 * SSH System Asset
 * Authoritative system-state container holding live, mutable values
 * that drive all style controllers at runtime and in edit mode
 */

    using UnityEngine;


    // ==========================================================================
    // System Asset
    // ==========================================================================

    [CreateAssetMenu(
        fileName = "SSHSystemAsset",
        menuName = "SSH/System Asset",
        order = 10)]
    public sealed class SSHSystemAsset : ScriptableObject
    {
        // ----- Core ----- //

        [Header("Core")]
        public CoreState Core = new CoreState();

        // ----- Environment ----- //

        [Header("Environment")]
        public EnvironmentState Environment = new EnvironmentState();

        // ----- Skybox ----- //

        [Header("Skybox")]
        public SkyboxState Skybox = new SkyboxState();

        // ----- Outline ----- //

        [Header("Outline")]
        public OutlineState Outline = new OutlineState();


        // ======================================================================
        // Core State (System)
        // ======================================================================

        [System.Serializable]
        public sealed class CoreState
        {
            public int LightSyncTick = 1;
        }


        // ======================================================================
        // Environment State (System)
        // ======================================================================

        [System.Serializable]
        public sealed class EnvironmentState
        {
            // ----- Shadows ----- //

            public Color ShadowColor = Color.black;
            public float ShadowIntensity = 1f;
            public float ShadowClip = 0f;
            public float ShadowRangeMult = 1f;

            // ----- Environment Light ----- //

            public Color EnvironmentLightColor = Color.white;
            public float EnvironmentLightIntensity = 1f;
            public float EnvironmentLightInfluence = 1f;
        }


        // ======================================================================
        // Skybox State (System)
        // ======================================================================

        [System.Serializable]
        public sealed class SkyboxState
        {
            // ----- Colors ----- //

            public Color BaseColor = Color.black;
            public Color AccentColor = Color.white;
            public Color StarsColor = Color.white;

            // ----- Bands ----- //

            public float BandIntensity = 1f;
            public float BandAdd = 0f;
            public float BandHorizontalWidth = 1f;
            public float BandContrast = 1f;

            // ----- Starmap ----- //

            public float StarmapDensity = 1f;
            public float StarmapScale = 1f;
            public float StarmapSeed = 0f;
            public float StarmapTilt = 1f;
        }


        // ======================================================================
        // Outline State (System)
        // ======================================================================

        [System.Serializable]
        public sealed class OutlineState
        {
            // ----- Color Outline ----- //

            public Color ColorOutlineColor = Color.black;
            public float ColorOutlineStrength = 1f;
            public float ColorOutlineNoiseScale = 1f;
            public float ColorOutlineNoiseIntensity = 0f;
            public float ColorOutlineNoiseAdd = 0f;
            public float ColorOutlineClip = 0f;

            // ----- Geo Outline ----- //

            public Color GeoOutlineColor = Color.black;
            public float GeoOutlineNormalStrength = 1f;
            public float GeoOutlineNormalCutoffMaskStrength = 0f;
            public float GeoOutlineNormalCutoffMaskAdd = 0f;

            public float GeoOutlineDepthStrengthStart = 0f;
            public float GeoOutlineDepthStrengthEnd = 1f;
            public float GeoOutlineDepthAdd = 0f;
            public float GeoOutlineDepthMult = 1f;
        }
    }

}