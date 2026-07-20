namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using System.IO;
    using System.Linq;

    public class SSHWizardLogic
    {
        // ------------------------------------------------------------
        // Public State Model
        // ------------------------------------------------------------
        public struct WizardSystemState
        {
            public bool ModulePresentAssets;
            public bool ModulePresentDemo;
            public bool ModulePresentWizard;
            public bool ModulePresentDocumentation;

            public RenderPipelineAsset QualityRenderPipelineAsset;
            public RenderPipelineAsset GraphicsRenderPipelineAsset;

            public RenderPipelineAsset HardlineRenderPipelineAsset;

            public string HardlineRootPath;
        }

        // ------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------
        public SSHWizardLogic()
        {
        }

        public void Tick()
        {
        }

        public void SelectLocalReadme()
        {
            string hardlineRoot = FindHardlineRootFolder();
            string readmePath = Path.Combine(
                hardlineRoot,
                "Documentation",
                "README.md"
            );

            // Focus on the README file if it exists
            if (File.Exists(readmePath))
            {
                Selection.activeObject =
                    AssetDatabase.LoadAssetAtPath<Object>(readmePath);
            }
        }


        // ------------------------------------------------------------
        // System State Evaluation
        // ------------------------------------------------------------
        public WizardSystemState GetWizardSystemState()
        {
            WizardSystemState state = new WizardSystemState();

            string hardlineRoot = FindHardlineRootFolder();
            state.HardlineRootPath = hardlineRoot;

            if (!string.IsNullOrEmpty(hardlineRoot))
            {
                state.ModulePresentAssets =
                    Directory.Exists(Path.Combine(hardlineRoot, "Assets", "SampleAssets"));

                state.ModulePresentDemo =
                    Directory.Exists(Path.Combine(hardlineRoot, "Demo"));

                state.ModulePresentWizard =
                    Directory.Exists(Path.Combine(hardlineRoot, "Wizard"));

                state.ModulePresentDocumentation =
                    Directory.Exists(Path.Combine(hardlineRoot, "Documentation")) ||
                    Directory.Exists(Path.Combine(hardlineRoot, "Docs"));

                state.HardlineRenderPipelineAsset =
                    FindHardlineRenderPipelineAsset(hardlineRoot);
            }

            state.QualityRenderPipelineAsset = QualitySettings.renderPipeline;
            state.GraphicsRenderPipelineAsset = GraphicsSettings.defaultRenderPipeline;

            return state;
        }

        // ------------------------------------------------------------
        // Safe Removal API
        // ------------------------------------------------------------
        public void RemoveModule(string assetRelativePath)
        {
            if (string.IsNullOrEmpty(assetRelativePath))
                return;

            if (!assetRelativePath.StartsWith("Assets/"))
            {
                Debug.LogError("[SSH Wizard] Refusing to delete non-Assets path.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(assetRelativePath))
            {
                Debug.LogWarning($"[SSH Wizard] Folder not found: {assetRelativePath}");
                return;
            }

            Debug.Log($"[SSH Wizard] Scheduled removal: {assetRelativePath}");

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.DeleteAsset(assetRelativePath);
                AssetDatabase.Refresh();
            };
        }

        public void RemoveWizardModule(string wizardPath)
        {
            Debug.Log("[SSH Wizard] Removing Wizard (self-removal)");

            EditorApplication.delayCall += () =>
            {
                EditorWindow.GetWindow<EditorSSHWizardWindow>()?.Close();

                AssetDatabase.DeleteAsset(wizardPath);
                AssetDatabase.Refresh();
            };
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------
        private string FindHardlineRootFolder()
        {
            string[] candidatePaths = AssetDatabase
                .GetAllAssetPaths()
                .Where(p =>
                    p.StartsWith("Assets/") &&
                    Directory.Exists(p) &&
                    Path.GetFileName(p) == "SmnStyleHardline")
                .ToArray();

            if (candidatePaths.Length == 0)
                return null;

            return candidatePaths
                .OrderBy(p => p.Count(c => c == '/'))
                .First();
        }

        private RenderPipelineAsset FindHardlineRenderPipelineAsset(string hardlineRoot)
        {
            string corePath = Path.Combine(hardlineRoot, "Core");

            if (!Directory.Exists(corePath))
                return null;

            string[] guids = AssetDatabase.FindAssets(
                "t:RenderPipelineAsset",
                new[] { corePath }
            );

            if (guids.Length == 0)
                return null;

            string assetPath = AssetDatabase.GUIDToAssetPath(
                guids.OrderBy(g => AssetDatabase.GUIDToAssetPath(g)).First()
            );

            return AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(assetPath);
        }
    }

}