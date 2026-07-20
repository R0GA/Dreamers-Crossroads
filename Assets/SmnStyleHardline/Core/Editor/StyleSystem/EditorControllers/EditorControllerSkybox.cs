namespace SmnStyleHardline.Editor
{
    /*
 * Editor Skybox Controller
 * Editor-only controller responsible for drawing skybox color,
 * band, and starmap controls for the SSH style system.
 */

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Core;

    // ==========================================================================
    // Skybox Editor Controller
    // ==========================================================================

    public sealed class EditorControllerSkybox
    {
        // ======================================================================
        // Ranges – Bands
        // ======================================================================

        const float SKY_BAND_INTENSITY_MIN = 0f;
        const float SKY_BAND_INTENSITY_MAX = 1f;

        const float SKY_BAND_ADD_MIN = -1f;
        const float SKY_BAND_ADD_MAX = 1f;

        const float SKY_BAND_HORIZONTAL_WIDTH_MIN = 0f;
        const float SKY_BAND_HORIZONTAL_WIDTH_MAX = 1f;

        const float SKY_BAND_CONTRAST_MIN = 0f;
        const float SKY_BAND_CONTRAST_MAX = 1f;


        // ======================================================================
        // Ranges – Starmap
        // ======================================================================

        const float SKY_STARMAP_DENSITY_MIN = 100f;
        const float SKY_STARMAP_DENSITY_MAX = 1000f;

        const float SKY_STARMAP_SCALE_MIN = 0.05f;
        const float SKY_STARMAP_SCALE_MAX = 0.5f;

        const float SKY_STARMAP_SEED_MIN = 0f;
        const float SKY_STARMAP_SEED_MAX = 0.5f;

        const float SKY_STARMAP_TILT_MIN = 20f;
        const float SKY_STARMAP_TILT_MAX = 100f;


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

            DrawSkyboxConfig(api.Skybox);
        }


        // ======================================================================
        // Skybox
        // ======================================================================

        void DrawSkyboxConfig(SkyboxAPI sky)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Skybox", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            DrawColorBlock(sky);
            EditorGUILayout.Space(8);
            DrawBandBlock(sky);
            EditorGUILayout.Space(8);
            DrawStarmapBlock(sky);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Colors
        // ======================================================================

        void DrawColorBlock(SkyboxAPI sky)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);

            DrawColor("Base", sky.GetBaseColor(), sky.SetBaseColor);
            DrawColor("Accent", sky.GetAccentColor(), sky.SetAccentColor);
            DrawColor("Stars", sky.GetStarsColor(), sky.SetStarsColor);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Bands
        // ======================================================================

        void DrawBandBlock(SkyboxAPI sky)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Bands", EditorStyles.boldLabel);

            DrawSlider(
                "Intensity",
                sky.GetBandIntensity(),
                SKY_BAND_INTENSITY_MIN,
                SKY_BAND_INTENSITY_MAX,
                sky.SetBandIntensity);

            DrawSlider(
                "Add",
                sky.GetBandAdd(),
                SKY_BAND_ADD_MIN,
                SKY_BAND_ADD_MAX,
                sky.SetBandAdd);

            DrawSlider(
                "Horizontal Width",
                sky.GetBandHorizontalWidth(),
                SKY_BAND_HORIZONTAL_WIDTH_MIN,
                SKY_BAND_HORIZONTAL_WIDTH_MAX,
                sky.SetBandHorizontalWidth);

            DrawSlider(
                "Contrast",
                sky.GetBandContrast(),
                SKY_BAND_CONTRAST_MIN,
                SKY_BAND_CONTRAST_MAX,
                sky.SetBandContrast);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Starmap
        // ======================================================================

        void DrawStarmapBlock(SkyboxAPI sky)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Starmap", EditorStyles.boldLabel);

            DrawSlider(
                "Density",
                sky.GetStarmapDensity(),
                SKY_STARMAP_DENSITY_MIN,
                SKY_STARMAP_DENSITY_MAX,
                sky.SetStarmapDensity);

            DrawSlider(
                "Scale",
                sky.GetStarmapScale(),
                SKY_STARMAP_SCALE_MIN,
                SKY_STARMAP_SCALE_MAX,
                sky.SetStarmapScale);

            DrawSlider(
                "Seed",
                sky.GetStarmapSeed(),
                SKY_STARMAP_SEED_MIN,
                SKY_STARMAP_SEED_MAX,
                sky.SetStarmapSeed);

            DrawSlider(
                "Tilt",
                sky.GetStarmapTilt(),
                SKY_STARMAP_TILT_MIN,
                SKY_STARMAP_TILT_MAX,
                sky.SetStarmapTilt);

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