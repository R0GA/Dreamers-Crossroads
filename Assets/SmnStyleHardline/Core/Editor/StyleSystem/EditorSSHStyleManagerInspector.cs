namespace SmnStyleHardline.Editor
{
    /*
 * SSH Style Manager Inspector
 * Custom editor providing tabbed access to system, environment,
 * skybox, and outline controls for the SSH style system.
 */

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Core;


    // ==========================================================================
    // SSH Style Manager Inspector
    // ==========================================================================

    [CustomEditor(typeof(SSHStyleManager))]
    public class EditorSSHStyleManagerInspector : Editor
    {
        // ======================================================================
        // Tabs
        // ======================================================================

        enum Tab
        {
            Core,
            Environment,
            Skybox,
            Outlines
        }

        Tab currentTab;


        // ======================================================================
        // Editor Controllers
        // ======================================================================

        EditorControllerCore editorCore;
        EditorControllerEnvironment editorEnvironment;
        EditorControllerSkybox editorSkybox;
        EditorControllerOutline editorOutline;


        // ======================================================================
        // Unity Hooks
        // ======================================================================

        void OnEnable()
        {
            editorCore = new EditorControllerCore();
            editorEnvironment = new EditorControllerEnvironment();
            editorSkybox = new EditorControllerSkybox();
            editorOutline = new EditorControllerOutline();
        }

        public override void OnInspectorGUI()
        {
            SSHStyleManager manager = (SSHStyleManager)target;
            SSHStyleAPI api = SSHStyleManager.API;

            if (api == null)
            {
                EditorGUILayout.HelpBox(
                    "SSHStyleAPI not initialized.",
                    MessageType.Warning
                );
                return;
            }

            DrawSystemAssetBlock(manager, api);

            if (!api.Core.HasSystemAsset())
                return;

            EditorGUILayout.Space();
            DrawTabBar();
            EditorGUILayout.Space();
            DrawCurrentTab(manager);
        }


        // ======================================================================
        // System Asset
        // ======================================================================

        void DrawSystemAssetBlock(
            SSHStyleManager manager,
            SSHStyleAPI api)
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);
            EditorGUILayout.LabelField("System", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            SSHSystemAsset asset =
                (SSHSystemAsset)EditorGUILayout.ObjectField(
                    "System Asset",
                    api.Core.GetSystemAsset(),
                    typeof(SSHSystemAsset),
                    false
                );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager, "Assign System Asset");
                api.Core.SetSystemAsset(asset);
                EditorUtility.SetDirty(manager);
            }

            if (!api.Core.HasSystemAsset())
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.HelpBox(
                    "A SSH System Asset must be assigned before the Style Manager can be used.",
                    MessageType.Warning
                );

                if (GUILayout.Button("Auto Assign"))
                {
                    AutoAssignSystemAsset(manager, api);
                }
            }

            EditorGUILayout.EndVertical();
        }

        void AutoAssignSystemAsset(
            SSHStyleManager manager,
            SSHStyleAPI api)
        {
            string[] guids = AssetDatabase.FindAssets("t:SSHSystemAsset");
            if (guids.Length == 0)
                return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            SSHSystemAsset asset =
                AssetDatabase.LoadAssetAtPath<SSHSystemAsset>(path);

            if (asset == null)
                return;

            Undo.RecordObject(manager, "Auto Assign System Asset");
            api.Core.SetSystemAsset(asset);
            EditorUtility.SetDirty(manager);
        }


        // ======================================================================
        // Tabs
        // ======================================================================

        void DrawTabBar()
        {
            currentTab = (Tab)GUILayout.Toolbar(
                (int)currentTab,
                new[] { "Core", "Environment", "Skybox", "Outlines" }
            );
        }

        void DrawCurrentTab(SSHStyleManager manager)
        {
            switch (currentTab)
            {
                case Tab.Core:
                    editorCore.Draw(manager);
                    break;

                case Tab.Environment:
                    editorEnvironment.Draw(manager);
                    break;

                case Tab.Skybox:
                    editorSkybox.Draw(manager);
                    break;

                case Tab.Outlines:
                    editorOutline.Draw(manager);
                    break;
            }
        }
    }
#endif

}