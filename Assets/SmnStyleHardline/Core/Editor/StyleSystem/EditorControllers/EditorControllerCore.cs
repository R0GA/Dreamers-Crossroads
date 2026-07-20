namespace SmnStyleHardline.Editor
{
    /*
     * Editor Core Controller
     * Editor-only controller responsible for drawing core system controls,
     * preset management, and tick configuration for the SSH style system.
     */

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Core;


    // ==========================================================================
    // Core Editor Controller
    // ==========================================================================

    public sealed class EditorControllerCore
    {
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

            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField("Core", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            DrawPresetPanel(manager, api);
            EditorGUILayout.Space(8);
            DrawTickPanel(manager, api);

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Presets
        // ======================================================================

        void DrawPresetPanel(SSHStyleManager manager, SSHStyleAPI api)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Preset"))
                PresetDetacher.RequestSave(manager, api);

            if (GUILayout.Button("Load Preset"))
                PresetDetacher.RequestLoad(manager, api);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Tick Controls
        // ======================================================================

        void DrawTickPanel(SSHStyleManager manager, SSHStyleAPI api)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("Tick Controls", EditorStyles.boldLabel);

            var systemAsset = api.Core.GetSystemAsset();
            if (systemAsset == null)
            {
                EditorGUILayout.HelpBox(
                    "System Asset required for tick configuration.",
                    MessageType.Warning
                );
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUI.BeginChangeCheck();
            int tick = EditorGUILayout.IntField(
                "Light Sync Tick",
                api.Core.GetLightSyncTick());

            if (EditorGUI.EndChangeCheck())
            {
                tick = Mathf.Max(1, tick);
                Undo.RecordObject(systemAsset, "Change Light Sync Tick");
                api.Core.SetLightSyncTick(tick);
                EditorUtility.SetDirty(systemAsset);
            }

            if (GUILayout.Button("Force Environment Tick"))
            {
                api.Core.ForceEnvironmentTick();
            }

            EditorGUILayout.EndVertical();
        }


        // ======================================================================
        // Preset Save / Load
        // ======================================================================

        static class PresetDetacher
        {
            enum Action
            {
                None,
                Save,
                Load
            }

            static Action action;
            static SSHStyleManager manager;
            static SSHStyleAPI api;

            public static void RequestSave(
                SSHStyleManager manager,
                SSHStyleAPI api)
            {
                Queue(Action.Save, manager, api);
            }

            public static void RequestLoad(
                SSHStyleManager manager,
                SSHStyleAPI api)
            {
                Queue(Action.Load, manager, api);
            }

            static void Queue(
                Action nextAction,
                SSHStyleManager nextManager,
                SSHStyleAPI nextApi)
            {
                action = nextAction;
                manager = nextManager;
                api = nextApi;

                EditorApplication.delayCall -= Execute;
                EditorApplication.delayCall += Execute;
            }

            static void Execute()
            {
                EditorApplication.delayCall -= Execute;

                if (action == Action.None ||
                    manager == null ||
                    api == null)
                    return;

                switch (action)
                {
                    case Action.Save:
                        SaveInternal(api);
                        break;

                    case Action.Load:
                        LoadInternal(manager, api);
                        break;
                }

                action = Action.None;
                manager = null;
                api = null;
            }


            // ==================================================================
            // Preset I/O
            // ==================================================================

            static void SaveInternal(SSHStyleAPI api)
            {
                string path =
                    EditorUtility.SaveFilePanelInProject(
                        "Save Style Preset",
                        "SSHStyleAsset",
                        "asset",
                        "Save style preset");

                if (string.IsNullOrEmpty(path))
                    return;

                var asset =
                    ScriptableObject.CreateInstance<SSHStyleAsset>();

                asset.Environment = CaptureEnvironment(api.Environment);
                asset.Skybox = CaptureSkybox(api.Skybox);
                asset.Outline = CaptureOutline(api.Outline);

                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            static void LoadInternal(
                SSHStyleManager manager,
                SSHStyleAPI api)
            {
                string path =
                    EditorUtility.OpenFilePanel(
                        "Load Style Preset",
                        "Assets",
                        "asset");

                if (string.IsNullOrEmpty(path))
                    return;

                path = FileUtil.GetProjectRelativePath(path);

                var asset =
                    AssetDatabase.LoadAssetAtPath<SSHStyleAsset>(path);

                if (asset == null)
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Preset",
                        "Selected file is not a SSHStyleAsset:\n" + path,
                        "OK");
                    return;
                }

                var systemAsset = api.Core.GetSystemAsset();
                if (systemAsset == null)
                    return;

                Undo.RecordObject(systemAsset, "Load Style Preset");
                api.Preset.Apply(asset);
                EditorUtility.SetDirty(systemAsset);
            }
        }


        // ======================================================================
        // Preset Capture
        // ======================================================================

        static SSHStyleEnvironmentData CaptureEnvironment(
            EnvironmentAPI env)
        {
            return new SSHStyleEnvironmentData
            {
                ShadowColor = env.GetShadowColor(),
                ShadowIntensity = env.GetShadowIntensity(),
                ShadowClip = env.GetShadowClip(),
                ShadowRangeMult = env.GetShadowRangeMult(),

                EnvironmentLightColor =
                    env.GetEnvironmentLightColor(),
                EnvironmentLightIntensity =
                    env.GetEnvironmentLightIntensity(),
                EnvironmentLightInfluence =
                    env.GetEnvironmentLightInfluence()
            };
        }

        static SSHStyleSkyboxData CaptureSkybox(
            SkyboxAPI sky)
        {
            return new SSHStyleSkyboxData
            {
                BaseColor = sky.GetBaseColor(),
                AccentColor = sky.GetAccentColor(),
                StarsColor = sky.GetStarsColor(),

                BandIntensity = sky.GetBandIntensity(),
                BandAdd = sky.GetBandAdd(),
                BandHorizontalWidth =
                    sky.GetBandHorizontalWidth(),
                BandContrast = sky.GetBandContrast(),

                StarmapDensity = sky.GetStarmapDensity(),
                StarmapScale = sky.GetStarmapScale(),
                StarmapSeed = sky.GetStarmapSeed(),
                StarmapTilt =
                    sky.GetStarmapTilt()
            };
        }

        static SSHStyleOutlineData CaptureOutline(
            OutlineAPI outline)
        {
            return new SSHStyleOutlineData
            {
                ColorOutlineColor =
                    outline.GetColorOutlineColor(),
                ColorOutlineStrength =
                    outline.GetColorOutlineStrength(),
                ColorOutlineNoiseScale =
                    outline.GetColorOutlineNoiseScale(),
                ColorOutlineNoiseIntensity =
                    outline.GetColorOutlineNoiseIntensity(),
                ColorOutlineNoiseAdd =
                    outline.GetColorOutlineNoiseAdd(),
                ColorOutlineClip =
                    outline.GetColorOutlineClip(),

                GeoOutlineColor =
                    outline.GetGeoOutlineColor(),
                GeoOutlineNormalStrength =
                    outline.GetGeoOutlineNormalStrength(),
                GeoOutlineNormalCutoffMaskStrength =
                    outline.GetGeoOutlineNormalCutoffMaskStrength(),
                GeoOutlineNormalCutoffMaskAdd =
                    outline.GetGeoOutlineNormalCutoffMaskAdd(),

                GeoOutlineDepthStrengthStart =
                    outline.GetGeoOutlineDepthStrengthStart(),
                GeoOutlineDepthStrengthEnd =
                    outline.GetGeoOutlineDepthStrengthEnd(),
                GeoOutlineDepthAdd =
                    outline.GetGeoOutlineDepthAdd(),
                GeoOutlineDepthMult =
                    outline.GetGeoOutlineDepthMult()
            };
        }
    }
#endif

}