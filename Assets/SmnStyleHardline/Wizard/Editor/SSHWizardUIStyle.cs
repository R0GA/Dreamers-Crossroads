namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using UnityEngine;
    using SmnStyleHardline.Editor;

    public static class SSHWizardUIStyle
    {
        public enum Status
        {
            Done,
            Warning,
            Error,
            Empty
        }

        private const float StatusIconWidth = 20f;
        private const float ActionButtonWidth = 160f;

        // ------------------------------------------------------------
        // Status Panel
        // ------------------------------------------------------------
        public static void DrawStatusPanel(
            string title,
            string message,
            Status status
        )
        {
            GUIStyle boxStyle = GetBoxStyle(status);
            GUIContent icon = GetIcon(status);

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));

            EditorGUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(title))
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (!string.IsNullOrEmpty(message))
                EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // ------------------------------------------------------------
        // Removal Row
        // ------------------------------------------------------------
        public static void DrawRemovalRow(
            string label,
            bool present,
            string buttonLabel,
            string confirmationMessage,
            System.Action onConfirm
        )
        {
            Status status = present ? Status.Done : Status.Empty;
            GUIContent icon = GetIcon(status);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(icon, GUILayout.Width(StatusIconWidth), GUILayout.Height(18));
            EditorGUILayout.LabelField(label, GUILayout.ExpandWidth(true));

            using (new EditorGUI.DisabledScope(!present))
            {
                if (GUILayout.Button(buttonLabel, GUILayout.Width(ActionButtonWidth)))
                {
                    bool confirm = EditorUtility.DisplayDialog(
                        "Confirm Removal",
                        confirmationMessage,
                        "Proceed",
                        "Cancel"
                    );

                    if (confirm)
                        onConfirm?.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        // ------------------------------------------------------------
        // Styling Internals
        // ------------------------------------------------------------
        private static GUIStyle GetBoxStyle(Status status)
        {
            GUIStyle style = new GUIStyle(EditorSSHGUIStyle.PaddedHelpBox);
            Color tint = GetTintColor(status);

            if (style.normal.background != null && tint.a > 0f)
                style.normal.background = TintTexture(style.normal.background, tint);

            return style;
        }

        private static Color GetTintColor(Status status)
        {
            switch (status)
            {
                case Status.Done: return new Color(0.35f, 0.75f, 0.45f, 0.15f);
                case Status.Warning: return new Color(1.0f, 0.7f, 0.2f, 0.18f);
                case Status.Error: return new Color(1.0f, 0.3f, 0.3f, 0.20f);
                case Status.Empty: return new Color(0.7f, 0.7f, 0.7f, 0.08f);
                default: return Color.clear;
            }
        }

        private static GUIContent GetIcon(Status status)
        {
            switch (status)
            {
                case Status.Done:
                    return EditorGUIUtility.IconContent("TestPassed");
                case Status.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon");
                case Status.Error:
                    return EditorGUIUtility.IconContent("console.erroricon");
                case Status.Empty:
                    return EditorGUIUtility.IconContent("Toolbar Minus");
                default:
                    return GUIContent.none;
            }
        }

        private static Texture2D TintTexture(Texture2D source, Color tint)
        {
            Texture2D tex = Object.Instantiate(source);
            Color[] pixels = tex.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.Lerp(pixels[i], tint, tint.a);

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }

}