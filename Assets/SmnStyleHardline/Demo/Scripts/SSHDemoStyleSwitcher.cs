namespace SmnStyleHardline.Demo
{
    using SmnStyleHardline.Core;
    using UnityEngine;

    public class SSHDemoStyleSwitcher : MonoBehaviour
    {
        [SerializeField]
        private int currentStyleIndex = 0;

        public SSHDemoStyleMeta[] stylePresets;

        private int lastAppliedIndex = -1;

        public bool ApplyStylePreset(int index)
        {
            if (stylePresets == null || stylePresets.Length == 0)
            {
                Debug.LogWarning("No style presets available.");
                return false;
            }

            index = Mathf.Clamp(index, 0, stylePresets.Length - 1);

            SSHStyleAsset preset = stylePresets[index].stylePreset;
            if (preset == null)
            {
                Debug.LogWarning("Style preset is null.");
                return false;
            }

            if (SSHStyleManager.API == null)
            {
                return false;
            }

            SSHStyleManager.API.Preset.Apply(preset);

            currentStyleIndex = index;
            lastAppliedIndex = index;

            return true;
        }

        public int GetCurrentStyleIndex()
        {
            return currentStyleIndex;
        }

        public int GetStylePresetCount()
        {
            return stylePresets != null ? stylePresets.Length : 0;
        }

        public string GetCurrentStyleName()
        {
            if (stylePresets == null || stylePresets.Length == 0)
                return "No Presets";

            int clampedIndex = Mathf.Clamp(currentStyleIndex, 0, stylePresets.Length - 1);
            return stylePresets[clampedIndex].presetName;
        }

        private void OnValidate()
        {
            if (stylePresets == null || stylePresets.Length == 0)
                return;

            int clampedIndex = Mathf.Clamp(currentStyleIndex, 0, stylePresets.Length - 1);

            if (clampedIndex != currentStyleIndex)
                currentStyleIndex = clampedIndex;

            if (clampedIndex != lastAppliedIndex)
            {
                ApplyStylePreset(clampedIndex);
            }
        }
    }

    [System.Serializable]
    public class SSHDemoStyleMeta
    {
        public SSHStyleAsset stylePreset;
        public string presetName;
    }
}