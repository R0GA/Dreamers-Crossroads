namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Editor;

    public class SSHWizardUITabDocs
    {
        // ------------------------------------------------------------
        // Config
        // ------------------------------------------------------------
        private const string DocsRootUrl =
            "https://summonboxstudio.com/summonstylehardline/docs";

        SSHWizardLogic logic = new SSHWizardLogic();

        // ------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------
        public void Draw()
        {
            EditorGUILayout.LabelField("Documentation", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var state = logic.GetWizardSystemState();

            // Only show local docs if the module exists
            if (state.ModulePresentDocumentation)
            {
                DrawLocalDocsPanel();
                EditorGUILayout.Space();
            }

            DrawSystemDocsPanel();
            EditorGUILayout.Space();

            DrawStyleGuidePanel();
        }

        // ------------------------------------------------------------
        // Local Docs
        // ------------------------------------------------------------
        private void DrawLocalDocsPanel()
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "Local Documentation",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "A local copy of the documentation is included with this package. "
                + "This is useful for offline access or long-term reference.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("Select Local README"))
            {
                logic.SelectLocalReadme();
            }

            EditorGUILayout.EndVertical();
        }

        // ------------------------------------------------------------
        // System Docs
        // ------------------------------------------------------------
        private void DrawSystemDocsPanel()
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "System Documentation",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "These documents explain how SummonStyle: Hardline is structured, "
                + "how its core systems work, and how to author content correctly.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space();

            DrawDocLink(
                "System Architecture",
                $"{DocsRootUrl}/system/architecture"
            );

            DrawDocLink(
                "Shader Overview",
                $"{DocsRootUrl}/system/shaders"
            );

            DrawDocLink(
                "Content Authoring",
                $"{DocsRootUrl}/system/authoring"
            );

            EditorGUILayout.EndVertical();
        }

        // ------------------------------------------------------------
        // Style Guide
        // ------------------------------------------------------------
        private void DrawStyleGuidePanel()
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "Style Guide",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "The style guide focuses on visual rules, artistic intent, and "
                + "practical guidelines for creating consistent content using Hardline.",
                EditorStyles.wordWrappedLabel
            );

            EditorGUILayout.Space();

            DrawDocLink(
                "Visual Principles",
                $"{DocsRootUrl}/style/principles"
            );

            DrawDocLink(
                "Color & Materials",
                $"{DocsRootUrl}/style/color-materials"
            );

            DrawDocLink(
                "Composition & Examples",
                $"{DocsRootUrl}/style/examples"
            );

            EditorGUILayout.EndVertical();
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------
        private void DrawDocLink(string label, string url)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                EditorGUIUtility.IconContent("_Help"),
                GUILayout.Width(18),
                GUILayout.Height(18)
            );

            if (GUILayout.Button(label, EditorStyles.linkLabel))
            {
                Application.OpenURL(url);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }
    }

}