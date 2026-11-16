namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using SmnStyleHardline.Editor;

    public class SSHWizardUITabSetup
    {
        private readonly SSHWizardLogic logic;

        public SSHWizardUITabSetup(SSHWizardLogic logic)
        {
            this.logic = logic;
        }

        public void Draw(SSHWizardLogic.WizardSystemState state)
        {
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawRenderPipelinePanel(state);
            EditorGUILayout.Space();

            DrawPresentModulesPanel(state);
        }

        // ------------------------------------------------------------
        // Render Pipeline Panel
        // ------------------------------------------------------------
        private void DrawRenderPipelinePanel(
            SSHWizardLogic.WizardSystemState state
        )
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "Universal Render Pipeline",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            RenderPipelineAsset hardlineRP = state.HardlineRenderPipelineAsset;
            RenderPipelineAsset activeRP = state.GraphicsRenderPipelineAsset;

            if (hardlineRP == null)
            {
                SSHWizardUIStyle.DrawStatusPanel(
                    "Render Pipeline Asset",
                    "Hardline Render Pipeline Asset was not found.",
                    SSHWizardUIStyle.Status.Error
                );
            }
            else if (activeRP == hardlineRP)
            {
                SSHWizardUIStyle.DrawStatusPanel(
                    "Render Pipeline Asset",
                    "Hardline Render Pipeline Asset is currently active.",
                    SSHWizardUIStyle.Status.Done
                );
            }
            else
            {
                SSHWizardUIStyle.DrawStatusPanel(
                    "Render Pipeline Asset",
                    "Hardline Render Pipeline Asset is available but not active.",
                    SSHWizardUIStyle.Status.Warning
                );

                EditorGUILayout.Space();

                if (GUILayout.Button("Apply Hardline Render Pipeline Asset"))
                {
                    ApplyHardlineRenderPipeline(hardlineRP);
                }
            }

            EditorGUILayout.EndVertical();
        }

        // ------------------------------------------------------------
        // Present Modules Panel
        // ------------------------------------------------------------
        private void DrawPresentModulesPanel(
            SSHWizardLogic.WizardSystemState state
        )
        {
            EditorGUILayout.BeginVertical(EditorSSHGUIStyle.PaddedHelpBox);

            EditorGUILayout.LabelField(
                "Optional Modules",
                EditorStyles.boldLabel
            );

            EditorGUILayout.Space();

            DrawModuleStatus("Sample Assets", state.ModulePresentAssets);
            DrawModuleStatus("Demo", state.ModulePresentDemo);
            DrawModuleStatus("Wizard", state.ModulePresentWizard);
            DrawModuleStatus("Documentation", state.ModulePresentDocumentation);

            EditorGUILayout.EndVertical();
        }

        private void DrawModuleStatus(string moduleName, bool present)
        {
            SSHWizardUIStyle.DrawStatusPanel(
                moduleName,
                present
                    ? "Module is installed."
                    : "Module is not installed (optional).",
                present
                    ? SSHWizardUIStyle.Status.Done
                    : SSHWizardUIStyle.Status.Empty
            );
        }

        // ------------------------------------------------------------
        // Apply RP
        // ------------------------------------------------------------
        private void ApplyHardlineRenderPipeline(RenderPipelineAsset rpAsset)
        {
            Undo.RecordObject(
                GraphicsSettings.GetGraphicsSettings(),
                "Apply Hardline Render Pipeline"
            );

            GraphicsSettings.defaultRenderPipeline = rpAsset;
            QualitySettings.renderPipeline = rpAsset;

            EditorUtility.SetDirty(GraphicsSettings.GetGraphicsSettings());
        }
    }

}