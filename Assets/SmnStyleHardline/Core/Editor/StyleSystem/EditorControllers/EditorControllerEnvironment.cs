namespace SmnStyleHardline.Editor
{
    /*
 * Editor Environment Controller
 * Editor-only controller responsible for drawing environment,
 * shadow, and ambient lighting controls for the SSH style system.
 */

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Core;

    // ==========================================================================
    // Environment Editor Controller
    // ==========================================================================

    public sealed class EditorControllerEnvironment
    {
        // ======================================================================
        // Ranges
        // ======================================================================

        const float SHADOW_INTENSITY_MIN = 0f;
        const float SHADOW_INTENSITY_MAX = 2f;

        const float SHADOW_CLIP_MIN = 0f;
        const float SHADOW_CLIP_MAX = 1f;

        const float SHADOW_RANGE_MULT_MIN = 0f;
        const float SHADOW_RANGE_MULT_MAX = 4f;

        const float ENV_LIGHT_INTENSITY_MIN = 0f;
        const float ENV_LIGHT_INTENSITY_MAX = 2f;

        const float ENV_LIGHT_INFLUENCE_MIN = 0f;
        const float ENV_LIGHT_INFLUENCE_MAX = 1f;


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

            DrawEnvironmentConfig(api.Environment);
        }


        // ======================================================================
        // Environment
        // ======================================================================

        void DrawEnvironmentConfig(EnvironmentAPI env)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Environment", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            DrawShadowBlock(env);
            EditorGUILayout.Space(8);
            DrawEnvironmentLightBlock(env);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Shadows
        // ======================================================================

        void DrawShadowBlock(EnvironmentAPI env)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Shadows", EditorStyles.boldLabel);

            DrawColor(
                "Shadow Color",
                env.GetShadowColor(),
                env.SetShadowColor);

            DrawSlider(
                "Shadow Intensity",
                env.GetShadowIntensity(),
                SHADOW_INTENSITY_MIN,
                SHADOW_INTENSITY_MAX,
                env.SetShadowIntensity);

            DrawSlider(
                "Shadow Clip",
                env.GetShadowClip(),
                SHADOW_CLIP_MIN,
                SHADOW_CLIP_MAX,
                env.SetShadowClip);

            DrawSlider(
                "Shadow Range Mult",
                env.GetShadowRangeMult(),
                SHADOW_RANGE_MULT_MIN,
                SHADOW_RANGE_MULT_MAX,
                env.SetShadowRangeMult);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Environment Light
        // ======================================================================

        void DrawEnvironmentLightBlock(EnvironmentAPI env)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Environment Light", EditorStyles.boldLabel);

            DrawColor(
                "Light Color",
                env.GetEnvironmentLightColor(),
                env.SetEnvironmentLightColor);

            DrawSlider(
                "Light Intensity",
                env.GetEnvironmentLightIntensity(),
                ENV_LIGHT_INTENSITY_MIN,
                ENV_LIGHT_INTENSITY_MAX,
                env.SetEnvironmentLightIntensity);

            DrawSlider(
                "Light Influence",
                env.GetEnvironmentLightInfluence(),
                ENV_LIGHT_INFLUENCE_MIN,
                ENV_LIGHT_INFLUENCE_MAX,
                env.SetEnvironmentLightInfluence);

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