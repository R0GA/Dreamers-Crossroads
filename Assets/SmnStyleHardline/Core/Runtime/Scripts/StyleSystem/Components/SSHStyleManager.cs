namespace SmnStyleHardline.Core
{
    /*
 * SSH Style Manager
 * Central singleton component responsible for initializing, wiring, and ticking
 * all SSH style and logic controllers across play mode and edit mode.
 */

    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [ExecuteAlways]
    public class SSHStyleManager : MonoBehaviour
    {
        // ======================================================================
        // Public Static Access
        // ======================================================================

        public static SSHStyleManager Instance;
        public static SSHStyleAPI API { get; private set; }


        // ======================================================================
        // Serialized Data
        // ======================================================================

        [SerializeField] SSHSystemAsset systemAsset;


        // ======================================================================
        // Logic Controllers
        // ======================================================================

        LogicControllerPreset logicPresets;
        LogicControllerEnvironment logicEnvironment;


        // ======================================================================
        // Style Controllers
        // ======================================================================

        StyleControllerEnvironment styleEnvironment;
        StyleControllerSkybox styleSkybox;
        StyleControllerOutline styleOutline;


        // ======================================================================
        // Unity Lifecycle Hooks
        // ======================================================================

        void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            EnsureInitialized();

#if UNITY_EDITOR
            Undo.undoRedoPerformed += OnUndoRedo;
#endif
        }

#if UNITY_EDITOR
        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
#endif

        void Update()
        {
#if UNITY_EDITOR
            // Edit-mode ticking for immediate visual feedback
            if (!Application.isPlaying)
            {
                EnsureInitialized();
                logicEnvironment?.Tick();
                return;
            }
#endif

            int tick = GetLightSyncTick_Internal();
            if (tick > 0 && Time.frameCount % tick == 0)
            {
                logicEnvironment?.Tick();
            }
        }


        // ======================================================================
        // Initialization & Wiring
        // ======================================================================

        /// <summary>
        /// Lazily creates and initializes all controllers and the public API.
        /// Safe to call repeatedly in both edit and play mode.
        /// </summary>
        void EnsureInitialized()
        {
            bool createdSomething = false;

            if (styleEnvironment == null)
            {
                styleEnvironment = new StyleControllerEnvironment();
                styleEnvironment.Init(systemAsset);
                createdSomething = true;
            }

            if (styleSkybox == null)
            {
                styleSkybox = new StyleControllerSkybox();
                styleSkybox.Init(systemAsset);
                createdSomething = true;
            }

            if (styleOutline == null)
            {
                styleOutline = new StyleControllerOutline();
                styleOutline.Init(systemAsset);
                createdSomething = true;
            }

            if (logicEnvironment == null)
            {
                logicEnvironment = new LogicControllerEnvironment();
                logicEnvironment.Init(styleEnvironment);
                createdSomething = true;
            }

            if (logicPresets == null)
            {
                logicPresets = new LogicControllerPreset();
                logicPresets.Init(styleEnvironment, styleSkybox, styleOutline);
                createdSomething = true;
            }

            if (API == null && createdSomething)
            {
                API = new SSHStyleAPI(
                    logicPresets,
                    logicEnvironment,
                    styleEnvironment,
                    styleSkybox,
                    styleOutline,
                    this
                );
            }
        }


        // ======================================================================
        // Internal API – System Asset
        // ======================================================================

        internal SSHSystemAsset GetSystemAsset_Internal()
        {
            return systemAsset;
        }

        internal void SetSystemAsset_Internal(SSHSystemAsset asset)
        {
            systemAsset = asset;

            styleEnvironment?.Init(asset);
            styleOutline?.Init(asset);
            styleSkybox?.Init(asset);
        }


        // ======================================================================
        // Internal API – Light Sync
        // ======================================================================

        internal int GetLightSyncTick_Internal()
        {
            if (systemAsset == null)
                return 1;

            return Mathf.Max(1, systemAsset.Core.LightSyncTick);
        }

        internal void SetLightSyncTick_Internal(int tick)
        {
            if (systemAsset == null)
                return;

            systemAsset.Core.LightSyncTick = Mathf.Max(1, tick);

#if UNITY_EDITOR
            EditorUtility.SetDirty(systemAsset);
#endif
        }

        internal void ForceEnvironmentTick_Internal()
        {
            logicEnvironment?.Tick();
        }


        // ======================================================================
        // Editor Hooks
        // ======================================================================

#if UNITY_EDITOR
        void OnUndoRedo()
        {
            if (systemAsset == null)
                return;

            styleEnvironment?.Init(systemAsset);
            styleSkybox?.Init(systemAsset);
            styleOutline?.Init(systemAsset);

            logicEnvironment?.RequestImmediateLightSync();
        }
#endif
    }

}