namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using System.IO;
    using SmnStyleHardline.Editor;

    public class SSHWizardUITabRemoval
    {
        private readonly SSHWizardLogic logic;

        public SSHWizardUITabRemoval(SSHWizardLogic logic)
        {
            this.logic = logic;
        }

        public void Draw(SSHWizardLogic.WizardSystemState state)
        {
            EditorGUILayout.LabelField("Removal", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawIndividualRemovalPanel(state);
            EditorGUILayout.Space();

            DrawBulkRemovalPanel(state);
        }

        // ------------------------------------------------------------
        // Individual Removal Panel
        // ------------------------------------------------------------
        private void DrawIndividualRemovalPanel(
            SSHWizardLogic.WizardSystemState state
        )
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "Remove Individual Modules",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            SSHWizardUIStyle.DrawRemovalRow(
                "Sample Assets",
                state.ModulePresentAssets,
                "Remove",
                "This will remove Sample Assets.\n\nThe Demo depends on Sample Assets and will also be removed.",
                () =>
                {
                    logic.RemoveModule(
                        Path.Combine(state.HardlineRootPath, "Assets", "SampleAssets")
                    );
                    logic.RemoveModule(
                        Path.Combine(state.HardlineRootPath, "Demo")
                    );
                }
            );

            SSHWizardUIStyle.DrawRemovalRow(
                "Demo (Scene + Scripts)",
                state.ModulePresentDemo,
                "Remove",
                "This will remove the Demo content.",
                () =>
                {
                    logic.RemoveModule(
                        Path.Combine(state.HardlineRootPath, "Demo")
                    );
                }
            );

            SSHWizardUIStyle.DrawRemovalRow(
                "Wizard",
                state.ModulePresentWizard,
                "Remove",
                "This will remove the Wizard UI and editor tooling.",
                () =>
                {
                    logic.RemoveWizardModule(
                        Path.Combine(state.HardlineRootPath, "Wizard")
                    );
                }
            );

            SSHWizardUIStyle.DrawRemovalRow(
                "Documentation",
                state.ModulePresentDocumentation,
                "Remove",
                "This will remove all documentation files.",
                () =>
                {
                    string docsPath = Directory.Exists(
                        Path.Combine(state.HardlineRootPath, "Documentation")
                    )
                        ? Path.Combine(state.HardlineRootPath, "Documentation")
                        : Path.Combine(state.HardlineRootPath, "Docs");

                    logic.RemoveModule(docsPath);
                }
            );

            EditorGUILayout.EndVertical();
        }

        // ------------------------------------------------------------
        // Bulk Removal Panel
        // ------------------------------------------------------------
        private void DrawBulkRemovalPanel(
            SSHWizardLogic.WizardSystemState state
        )
        {
            bool anyPresent =
                state.ModulePresentAssets ||
                state.ModulePresentDemo ||
                state.ModulePresentWizard ||
                state.ModulePresentDocumentation;

            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "Clean / Bulk Actions",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            SSHWizardUIStyle.DrawRemovalRow(
                "All Non-Core Modules",
                anyPresent,
                "Clean",
                "This will remove all optional modules.\n\nCore files will be preserved.",
                () =>
                {
                    if (state.ModulePresentAssets)
                    {
                        logic.RemoveModule(
                            Path.Combine(state.HardlineRootPath, "Assets", "SampleAssets")
                        );
                    }

                    if (state.ModulePresentDemo)
                    {
                        logic.RemoveModule(
                            Path.Combine(state.HardlineRootPath, "Demo")
                        );
                    }

                    if (state.ModulePresentDocumentation)
                    {
                        logic.RemoveModule(
                            Directory.Exists(
                                Path.Combine(state.HardlineRootPath, "Documentation")
                            )
                                ? Path.Combine(state.HardlineRootPath, "Documentation")
                                : Path.Combine(state.HardlineRootPath, "Docs")
                        );
                    }

                    if (state.ModulePresentWizard)
                    {
                        logic.RemoveWizardModule(
                            Path.Combine(state.HardlineRootPath, "Wizard")
                        );
                    }
                }
            );

            EditorGUILayout.EndVertical();
        }
    }

}