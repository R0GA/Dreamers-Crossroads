namespace SmnStyleHardline.Editor
{
    /*
 * Editor Outline Controller
 * Editor-only controller responsible for drawing color and geometry
 * outline controls for the SSH style system.
 */

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Core;

    // ==========================================================================
    // Outline Editor Controller
    // ==========================================================================

    public sealed class EditorControllerOutline
    {
        // ======================================================================
        // Ranges – Color Outline
        // ======================================================================

        const float COLOR_OUTLINE_STRENGTH_MIN = 0.05f;
        const float COLOR_OUTLINE_STRENGTH_MAX = 3f;

        const float COLOR_OUTLINE_NOISE_SCALE_MIN = 0f;
        const float COLOR_OUTLINE_NOISE_SCALE_MAX = 10f;

        const float COLOR_OUTLINE_NOISE_INTENSITY_MIN = 0f;
        const float COLOR_OUTLINE_NOISE_INTENSITY_MAX = 5f;

        const float COLOR_OUTLINE_NOISE_ADD_MIN = 0f;
        const float COLOR_OUTLINE_NOISE_ADD_MAX = 1f;

        const float COLOR_OUTLINE_CLIP_MIN = 0f;
        const float COLOR_OUTLINE_CLIP_MAX = 1f;


        // ======================================================================
        // Ranges – Geometry Outline
        // ======================================================================

        const float GEO_OUTLINE_NORMAL_STRENGTH_MIN = 0.4f;
        const float GEO_OUTLINE_NORMAL_STRENGTH_MAX = 4f;

        const float GEO_OUTLINE_NORMAL_CUTOFF_STRENGTH_MIN = 0f;
        const float GEO_OUTLINE_NORMAL_CUTOFF_STRENGTH_MAX = 10f;

        const float GEO_OUTLINE_NORMAL_CUTOFF_ADD_MIN = -10f;
        const float GEO_OUTLINE_NORMAL_CUTOFF_ADD_MAX = 10f;

        const float GEO_OUTLINE_DEPTH_STRENGTH_START_MIN = 0.04f;
        const float GEO_OUTLINE_DEPTH_STRENGTH_START_MAX = 10f;

        const float GEO_OUTLINE_DEPTH_STRENGTH_END_MIN = 0f;
        const float GEO_OUTLINE_DEPTH_STRENGTH_END_MAX = 10f;

        const float GEO_OUTLINE_DEPTH_ADD_MIN = 0f;
        const float GEO_OUTLINE_DEPTH_ADD_MAX = 10f;

        const float GEO_OUTLINE_DEPTH_MULT_MIN = 0f;
        const float GEO_OUTLINE_DEPTH_MULT_MAX = 10f;


        // ======================================================================
        // Inspector Entry
        // ======================================================================

        public void Draw(SSHStyleManager manager)
        {
            var api = SSHStyleManager.API;
            if (api == null)
            {
                EditorGUILayout.HelpBox(
                    "SSHStyleAPI not initialized.",
                    MessageType.Warning
                );
                return;
            }

            DrawOutlineConfig(api.Outline);
        }


        // ======================================================================
        // Outline
        // ======================================================================

        void DrawOutlineConfig(OutlineAPI outline)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Outlines", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            DrawColorOutlineBlock(outline);
            EditorGUILayout.Space(8);
            DrawGeoOutlineBlock(outline);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Color Outline
        // ======================================================================

        void DrawColorOutlineBlock(OutlineAPI outline)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Color Outline", EditorStyles.boldLabel);

            DrawColor(
                "Color",
                outline.GetColorOutlineColor(),
                outline.SetColorOutlineColor);

            DrawSlider(
                "Strength",
                outline.GetColorOutlineStrength(),
                COLOR_OUTLINE_STRENGTH_MIN,
                COLOR_OUTLINE_STRENGTH_MAX,
                outline.SetColorOutlineStrength);

            DrawSlider(
                "Noise Scale",
                outline.GetColorOutlineNoiseScale(),
                COLOR_OUTLINE_NOISE_SCALE_MIN,
                COLOR_OUTLINE_NOISE_SCALE_MAX,
                outline.SetColorOutlineNoiseScale);

            DrawSlider(
                "Noise Intensity",
                outline.GetColorOutlineNoiseIntensity(),
                COLOR_OUTLINE_NOISE_INTENSITY_MIN,
                COLOR_OUTLINE_NOISE_INTENSITY_MAX,
                outline.SetColorOutlineNoiseIntensity);

            DrawSlider(
                "Noise Add",
                outline.GetColorOutlineNoiseAdd(),
                COLOR_OUTLINE_NOISE_ADD_MIN,
                COLOR_OUTLINE_NOISE_ADD_MAX,
                outline.SetColorOutlineNoiseAdd);

            DrawSlider(
                "Clip",
                outline.GetColorOutlineClip(),
                COLOR_OUTLINE_CLIP_MIN,
                COLOR_OUTLINE_CLIP_MAX,
                outline.SetColorOutlineClip);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Geometry Outline
        // ======================================================================

        void DrawGeoOutlineBlock(OutlineAPI outline)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Geo Outline", EditorStyles.boldLabel);

            DrawColor(
                "Color",
                outline.GetGeoOutlineColor(),
                outline.SetGeoOutlineColor);

            DrawSlider(
                "Normal Strength",
                outline.GetGeoOutlineNormalStrength(),
                GEO_OUTLINE_NORMAL_STRENGTH_MIN,
                GEO_OUTLINE_NORMAL_STRENGTH_MAX,
                outline.SetGeoOutlineNormalStrength);

            DrawSlider(
                "Normal Cutoff Strength",
                outline.GetGeoOutlineNormalCutoffMaskStrength(),
                GEO_OUTLINE_NORMAL_CUTOFF_STRENGTH_MIN,
                GEO_OUTLINE_NORMAL_CUTOFF_STRENGTH_MAX,
                outline.SetGeoOutlineNormalCutoffMaskStrength);

            DrawSlider(
                "Normal Cutoff Add",
                outline.GetGeoOutlineNormalCutoffMaskAdd(),
                GEO_OUTLINE_NORMAL_CUTOFF_ADD_MIN,
                GEO_OUTLINE_NORMAL_CUTOFF_ADD_MAX,
                outline.SetGeoOutlineNormalCutoffMaskAdd);

            DrawSlider(
                "Depth Strength Start",
                outline.GetGeoOutlineDepthStrengthStart(),
                GEO_OUTLINE_DEPTH_STRENGTH_START_MIN,
                GEO_OUTLINE_DEPTH_STRENGTH_START_MAX,
                outline.SetGeoOutlineDepthStrengthStart);

            DrawSlider(
                "Depth Strength End",
                outline.GetGeoOutlineDepthStrengthEnd(),
                GEO_OUTLINE_DEPTH_STRENGTH_END_MIN,
                GEO_OUTLINE_DEPTH_STRENGTH_END_MAX,
                outline.SetGeoOutlineDepthStrengthEnd);

            DrawSlider(
                "Depth Add",
                outline.GetGeoOutlineDepthAdd(),
                GEO_OUTLINE_DEPTH_ADD_MIN,
                GEO_OUTLINE_DEPTH_ADD_MAX,
                outline.SetGeoOutlineDepthAdd);

            DrawSlider(
                "Depth Mult",
                outline.GetGeoOutlineDepthMult(),
                GEO_OUTLINE_DEPTH_MULT_MIN,
                GEO_OUTLINE_DEPTH_MULT_MAX,
                outline.SetGeoOutlineDepthMult);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Field Helpers
        // ======================================================================

        void DrawSlider(
            string label,
            float value,
            float min,
            float max,
            System.Action<float> setter)
        {
            var asset = SSHStyleManager.API.Core.GetSystemAsset();
            if (asset == null)
                return;

            EditorGUI.BeginChangeCheck();
            float v = EditorGUILayout.Slider(label, value, min, max);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(asset, $"Change {label}");
                setter(v);
                EditorUtility.SetDirty(asset);
            }
        }

        void DrawColor(
            string label,
            Color value,
            System.Action<Color> setter)
        {
            var asset = SSHStyleManager.API.Core.GetSystemAsset();
            if (asset == null)
                return;

            EditorGUI.BeginChangeCheck();
            Color v = EditorGUILayout.ColorField(label, value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(asset, $"Change {label}");
                setter(v);
                EditorUtility.SetDirty(asset);
            }
        }
    }
#endif

}