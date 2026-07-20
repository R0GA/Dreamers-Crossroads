namespace SmnStyleHardline.Editor
{
    /*
 * Editor GUI Styles
 * Shared editor-only GUIStyle definitions used across
 * SSH style system inspectors.
 */

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


// ==========================================================================
// Editor GUI Styles
// ==========================================================================

public static class EditorSSHGUIStyle
{
    static GUIStyle paddedHelpBox;


    // ======================================================================
    // Styles
    // ======================================================================

    public static GUIStyle PaddedHelpBox
    {
        get
        {
            if (paddedHelpBox == null)
            {
                paddedHelpBox = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(
                        left: 10,
                        right: 10,
                        top: 8,
                        bottom: 8
                    )
                };
            }

            return paddedHelpBox;
        }
    }
}
#endif

}