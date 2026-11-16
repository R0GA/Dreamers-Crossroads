namespace SmnStyleHardline.Core
{
    /*
 * SSH Style API
 * Primary entry point exposing all runtime style control surfaces,
 * including core state, presets, and individual style domains.
 */


    // ==========================================================================
    // Root Style API
    // ==========================================================================

    public sealed class SSHStyleAPI
    {
        /// <summary>
        /// Provides access to global system state and core runtime controls.
        /// </summary>
        public CoreAPI Core { get; }

        /// <summary>
        /// Provides access to style preset application.
        /// </summary>
        public PresetAPI Preset { get; }

        /// <summary>
        /// Provides access to environment lighting and shadow controls.
        /// </summary>
        public EnvironmentAPI Environment { get; }

        /// <summary>
        /// Provides access to skybox rendering controls.
        /// </summary>
        public SkyboxAPI Skybox { get; }

        /// <summary>
        /// Provides access to outline rendering controls.
        /// </summary>
        public OutlineAPI Outline { get; }


        // ======================================================================
        // Construction
        // ======================================================================

        internal SSHStyleAPI(
            LogicControllerPreset logicPresets,
            LogicControllerEnvironment logicEnvironment,
            StyleControllerEnvironment styleEnvironment,
            StyleControllerSkybox styleSkybox,
            StyleControllerOutline styleOutline,
            SSHStyleManager manager)
        {
            Preset = new PresetAPI(logicPresets);
            Environment = new EnvironmentAPI(styleEnvironment, logicEnvironment);
            Skybox = new SkyboxAPI(styleSkybox);
            Outline = new OutlineAPI(styleOutline);
            Core = new CoreAPI(manager);
        }
    }

}