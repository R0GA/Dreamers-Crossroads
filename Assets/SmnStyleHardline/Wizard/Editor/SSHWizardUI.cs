namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using UnityEngine;

    public class SSHWizardUI
    {
        private const string ShowWizardPrefKey = "SSH_ShowWizardOnStartup";

        private enum Tab
        {
            Setup,
            Removal,
            Docs
        }

        private readonly SSHWizardLogic logic;

        private readonly SSHWizardUITabSetup tabSetup;
        private readonly SSHWizardUITabRemoval tabRemoval;
        private readonly SSHWizardUITabDocs tabDocs;

        private Tab currentTab;
        private bool showOnStartup;

        public SSHWizardUI(SSHWizardLogic logic)
        {
            this.logic = logic;

            tabSetup = new SSHWizardUITabSetup(logic);
            tabRemoval = new SSHWizardUITabRemoval(logic);
            tabDocs = new SSHWizardUITabDocs();

            showOnStartup = EditorPrefs.GetBool(ShowWizardPrefKey, true);
        }

        // ------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------
        public void Draw(SSHWizardLogic.WizardSystemState state)
        {
            DrawHeader();
            EditorGUILayout.Space();

            DrawTabBar();
            EditorGUILayout.Space();

            DrawCurrentTab(state);

            GUILayout.FlexibleSpace();
            DrawFooter();
        }

        // ------------------------------------------------------------
        // Header
        // ------------------------------------------------------------
        private void DrawHeader()
        {
            EditorGUILayout.LabelField(
                "SummonStyle: Hardline Wizard",
                EditorStyles.boldLabel
            );
        }

        // ------------------------------------------------------------
        // Tabs
        // ------------------------------------------------------------
        private void DrawTabBar()
        {
            currentTab = (Tab)GUILayout.Toolbar(
                (int)currentTab,
                new[] { "Setup", "Removal", "Docs" }
            );
        }

        private void DrawCurrentTab(SSHWizardLogic.WizardSystemState state)
        {
            switch (currentTab)
            {
                case Tab.Setup:
                    tabSetup.Draw(state);
                    break;

                case Tab.Removal:
                    tabRemoval.Draw(state);
                    break;

                case Tab.Docs:
                    tabDocs.Draw();
                    break;
            }
        }

        // ------------------------------------------------------------
        // Footer
        // ------------------------------------------------------------
        private void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            bool newValue = EditorGUILayout.ToggleLeft(
                "Show this wizard on domain reload",
                showOnStartup
            );

            if (newValue != showOnStartup)
            {
                showOnStartup = newValue;
                EditorPrefs.SetBool(ShowWizardPrefKey, showOnStartup);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

}