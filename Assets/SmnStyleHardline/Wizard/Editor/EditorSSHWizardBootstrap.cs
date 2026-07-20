namespace SmnStyleHardline.Wizard
{
    using UnityEditor;

    [InitializeOnLoad]
    public static class SSHWizardBootstrap
    {
        private const string ShowWizardPrefKey = "SSH_ShowWizardOnStartup";

        static SSHWizardBootstrap()
        {
            EditorApplication.delayCall += TryOpenWizard;
        }

        private static void TryOpenWizard()
        {
            if (!EditorPrefs.HasKey(ShowWizardPrefKey))
            {
                // Default behavior on first install
                EditorPrefs.SetBool(ShowWizardPrefKey, true);
            }

            if (EditorPrefs.GetBool(ShowWizardPrefKey))
            {
                EditorSSHWizardWindow.OpenWindow();
            }
        }
    }

}