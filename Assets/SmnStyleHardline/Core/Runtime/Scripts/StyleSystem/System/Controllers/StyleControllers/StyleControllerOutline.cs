namespace SmnStyleHardline.Core
{
    /*
 * Style Controller – Outline
 * Synchronizes outline-related style data with global shader properties,
 * covering both color-based and geometry-based outline effects.
 */

    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public sealed class StyleControllerOutline
    {
        // ======================================================================
        // Data Reference
        // ======================================================================

        SSHSystemAsset asset;


        // ======================================================================
        // Shader Property Names
        // ======================================================================

        const string COLOR_OUTLINE_COLOR = "_SSH_PP_ColorOutline_Color";
        const string COLOR_OUTLINE_STRENGTH = "_SSH_PP_ColorOutline_Strength";
        const string COLOR_OUTLINE_NOISE_SCALE = "_SSH_PP_ColorOutline_Noise_Scale";
        const string COLOR_OUTLINE_NOISE_INTENSITY = "_SSH_PP_ColorOutline_Noise_Intensity";
        const string COLOR_OUTLINE_NOISE_ADD = "_SSH_PP_ColorOutline_Noise_Add";
        const string COLOR_OUTLINE_CLIP = "_SSH_PP_ColorOutline_Noise_Clip";

        const string GEO_OUTLINE_COLOR = "_SSH_PP_GeoOutline_Color";
        const string GEO_OUTLINE_NORMAL_STRENGTH = "_SSH_PP_GeoOutline_Normal_Strength";
        const string GEO_OUTLINE_NORMAL_CUTOFF_MASK_STRENGTH = "_SSH_PP_GeoOutline_Normal_CutoffMaskStrength";
        const string GEO_OUTLINE_NORMAL_CUTOFF_MASK_ADD = "_SSH_PP_GeoOutline_Normal_CutoffMaskAdd";

        const string GEO_OUTLINE_DEPTH_STRENGTH_START = "_SSH_PP_GeoOutline_Depth_StrengthStart";
        const string GEO_OUTLINE_DEPTH_STRENGTH_END = "_SSH_PP_GeoOutline_Depth_StrengthEnd";
        const string GEO_OUTLINE_DEPTH_ADD = "_SSH_PP_GeoOutline_Depth_Add";
        const string GEO_OUTLINE_DEPTH_MULT = "_SSH_PP_GeoOutline_Depth_Mult";


        // ======================================================================
        // Initialization
        // ======================================================================

        /// <summary>
        /// Binds the system asset and applies all outline values to shaders.
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

            var o = asset.Outline;

            Shader.SetGlobalColor(COLOR_OUTLINE_COLOR, o.ColorOutlineColor);
            Shader.SetGlobalFloat(COLOR_OUTLINE_STRENGTH, o.ColorOutlineStrength);
            Shader.SetGlobalFloat(COLOR_OUTLINE_NOISE_SCALE, o.ColorOutlineNoiseScale);
            Shader.SetGlobalFloat(COLOR_OUTLINE_NOISE_INTENSITY, o.ColorOutlineNoiseIntensity);
            Shader.SetGlobalFloat(COLOR_OUTLINE_NOISE_ADD, o.ColorOutlineNoiseAdd);
            Shader.SetGlobalFloat(COLOR_OUTLINE_CLIP, o.ColorOutlineClip);

            Shader.SetGlobalColor(GEO_OUTLINE_COLOR, o.GeoOutlineColor);
            Shader.SetGlobalFloat(GEO_OUTLINE_NORMAL_STRENGTH, o.GeoOutlineNormalStrength);
            Shader.SetGlobalFloat(
                GEO_OUTLINE_NORMAL_CUTOFF_MASK_STRENGTH,
                o.GeoOutlineNormalCutoffMaskStrength);
            Shader.SetGlobalFloat(
                GEO_OUTLINE_NORMAL_CUTOFF_MASK_ADD,
                o.GeoOutlineNormalCutoffMaskAdd);

            Shader.SetGlobalFloat(
                GEO_OUTLINE_DEPTH_STRENGTH_START,
                o.GeoOutlineDepthStrengthStart);
            Shader.SetGlobalFloat(
                GEO_OUTLINE_DEPTH_STRENGTH_END,
                o.GeoOutlineDepthStrengthEnd);
            Shader.SetGlobalFloat(GEO_OUTLINE_DEPTH_ADD, o.GeoOutlineDepthAdd);
            Shader.SetGlobalFloat(GEO_OUTLINE_DEPTH_MULT, o.GeoOutlineDepthMult);
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
        // Color Outline Controls
        // ======================================================================

        public void SetColorOutlineColor(Color v)
        {
            asset.Outline.ColorOutlineColor = v;
            Shader.SetGlobalColor(COLOR_OUTLINE_COLOR, v);
            MarkDirty(asset);
        }

        public Color GetColorOutlineColor()
            => asset.Outline.ColorOutlineColor;

        public void SetColorOutlineStrength(float v)
        {
            asset.Outline.ColorOutlineStrength = v;
            Shader.SetGlobalFloat(COLOR_OUTLINE_STRENGTH, v);
            MarkDirty(asset);
        }

        public float GetColorOutlineStrength()
            => asset.Outline.ColorOutlineStrength;

        public void SetColorOutlineNoiseScale(float v)
        {
            asset.Outline.ColorOutlineNoiseScale = v;
            Shader.SetGlobalFloat(COLOR_OUTLINE_NOISE_SCALE, v);
            MarkDirty(asset);
        }

        public float GetColorOutlineNoiseScale()
            => asset.Outline.ColorOutlineNoiseScale;

        public void SetColorOutlineNoiseIntensity(float v)
        {
            asset.Outline.ColorOutlineNoiseIntensity = v;
            Shader.SetGlobalFloat(COLOR_OUTLINE_NOISE_INTENSITY, v);
            MarkDirty(asset);
        }

        public float GetColorOutlineNoiseIntensity()
            => asset.Outline.ColorOutlineNoiseIntensity;

        public void SetColorOutlineNoiseAdd(float v)
        {
            asset.Outline.ColorOutlineNoiseAdd = v;
            Shader.SetGlobalFloat(COLOR_OUTLINE_NOISE_ADD, v);
            MarkDirty(asset);
        }

        public float GetColorOutlineNoiseAdd()
            => asset.Outline.ColorOutlineNoiseAdd;

        public void SetColorOutlineClip(float v)
        {
            asset.Outline.ColorOutlineClip = v;
            Shader.SetGlobalFloat(COLOR_OUTLINE_CLIP, v);
            MarkDirty(asset);
        }

        public float GetColorOutlineClip()
            => asset.Outline.ColorOutlineClip;


        // ======================================================================
        // Geometry Outline Controls
        // ======================================================================

        public void SetGeoOutlineColor(Color v)
        {
            asset.Outline.GeoOutlineColor = v;
            Shader.SetGlobalColor(GEO_OUTLINE_COLOR, v);
            MarkDirty(asset);
        }

        public Color GetGeoOutlineColor()
            => asset.Outline.GeoOutlineColor;

        public void SetGeoOutlineNormalStrength(float v)
        {
            asset.Outline.GeoOutlineNormalStrength = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_NORMAL_STRENGTH, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineNormalStrength()
            => asset.Outline.GeoOutlineNormalStrength;

        public void SetGeoOutlineNormalCutoffMaskStrength(float v)
        {
            asset.Outline.GeoOutlineNormalCutoffMaskStrength = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_NORMAL_CUTOFF_MASK_STRENGTH, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineNormalCutoffMaskStrength()
            => asset.Outline.GeoOutlineNormalCutoffMaskStrength;

        public void SetGeoOutlineNormalCutoffMaskAdd(float v)
        {
            asset.Outline.GeoOutlineNormalCutoffMaskAdd = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_NORMAL_CUTOFF_MASK_ADD, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineNormalCutoffMaskAdd()
            => asset.Outline.GeoOutlineNormalCutoffMaskAdd;

        public void SetGeoOutlineDepthStrengthStart(float v)
        {
            asset.Outline.GeoOutlineDepthStrengthStart = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_DEPTH_STRENGTH_START, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineDepthStrengthStart()
            => asset.Outline.GeoOutlineDepthStrengthStart;

        public void SetGeoOutlineDepthStrengthEnd(float v)
        {
            asset.Outline.GeoOutlineDepthStrengthEnd = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_DEPTH_STRENGTH_END, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineDepthStrengthEnd()
            => asset.Outline.GeoOutlineDepthStrengthEnd;

        public void SetGeoOutlineDepthAdd(float v)
        {
            asset.Outline.GeoOutlineDepthAdd = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_DEPTH_ADD, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineDepthAdd()
            => asset.Outline.GeoOutlineDepthAdd;

        public void SetGeoOutlineDepthMult(float v)
        {
            asset.Outline.GeoOutlineDepthMult = v;
            Shader.SetGlobalFloat(GEO_OUTLINE_DEPTH_MULT, v);
            MarkDirty(asset);
        }

        public float GetGeoOutlineDepthMult()
            => asset.Outline.GeoOutlineDepthMult;
    }

}