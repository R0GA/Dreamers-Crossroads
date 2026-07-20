namespace SmnStyleHardline.Wizard
{
    using UnityEditor;
    using UnityEngine;

    public class EditorSSHWizardWindow : EditorWindow
    {
        private const string WindowTitle = "SSH Wizard";

        private SSHWizardLogic logic;
        private SSHWizardUI ui;

        [MenuItem("Window/SummonStyle: Hardline/SummonStyle: Hardline Wizard")]
        public static void OpenWindow()
        {
            EditorSSHWizardWindow window = GetWindow<EditorSSHWizardWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(420, 320);
            window.Show();
        }

        private void OnEnable()
        {
            logic = new SSHWizardLogic();
            ui = new SSHWizardUI(logic);
        }

        private void OnGUI()
        {
            logic.Tick();

            SSHWizardLogic.WizardSystemState state =
                logic.GetWizardSystemState();

            ui.Draw(state);
        }
    }

}