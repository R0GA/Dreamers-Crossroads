namespace SmnStyleHardline.Core
{
    /*
 * Preset API
 * Applies complete style presets to all active style controllers.
 */


    // ==========================================================================
    // Preset API
    // ==========================================================================

    public sealed class PresetAPI
    {
        readonly LogicControllerPreset logic;

        internal PresetAPI(LogicControllerPreset logicPresets)
        {
            logic = logicPresets;
        }

        /// <summary>
        /// Applies a style preset asset to the current system state.
        /// </summary>
        /// <param name="asset">The preset asset to apply.</param>
        public void Apply(SSHStyleAsset asset)
        {
            logic.Apply(asset);
        }
    }

}