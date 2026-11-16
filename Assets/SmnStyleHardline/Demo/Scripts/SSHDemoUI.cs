namespace SmnStyleHardline.Demo
{
    /*
     * SSH Demo UI
     * Runtime demo UI controller responsible for wiring UI Toolkit elements
     * to camera control, day/night playback, and informational overlays.
     */

    using UnityEngine;
    using UnityEngine.UIElements;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public class SSHDemoUI : MonoBehaviour
    {
        // ======================================================================
        // Linked Components
        // ======================================================================

        [Header("Linked Components")]
        [SerializeField] private SSHDemoCamera demoCamera;
        [SerializeField] private SSHDemoDayNightManager dayNightManager;
        [SerializeField] private SSHDemoStyleSwitcher styleSwitcher;


        // ======================================================================
        // Time Of Day Icons
        // ======================================================================

        [Header("Time Of Day Icons")]
        [SerializeField] private Sprite iconSunrise;
        [SerializeField] private Sprite iconDay;
        [SerializeField] private Sprite iconSunset;
        [SerializeField] private Sprite iconNight;


        // ======================================================================
        // UI References
        // ======================================================================

        VisualElement _root;

        VisualElement _toggleBaseInfo;
        Image _toggleCheckInfo;

        VisualElement _toggleBaseFreeCam;
        Image _toggleCheckFreeCam;

        VisualElement _toggleBaseInteractionMode;
        Image _toggleCheckInteractionMode;

        VisualElement _panelStyle;
        VisualElement _panelDayNightControls;

        VisualElement _toggleBaseDayNightPlay;
        Image _toggleCheckDayNightPlay;

        Slider _timeSlider;
        Image _timeIcon;

        VisualElement _informationPanel;

        VisualElement _cinematicUI;
        Image _cinArrowLeft;
        Image _cinArrowRight;

        VisualElement _btnStyleLeft;
        VisualElement _btnStyleRight;
        Label _lblStyleName;

        Label _infoHeading;
        Label _infoContent;


        // ======================================================================
        // State
        // ======================================================================

        bool _infoEnabled = true;
        bool _uiVisible = true;
        bool _freeCamEnabled = true;
        bool _dayNightPlaying;
        bool _interactionModeEnabled;
        bool _ignoreSliderCallback;
        int _currentCinematicIndex;

        TimeOfDay _currentTimeOfDay = TimeOfDay.None;


        // ======================================================================
        // Internal Types
        // ======================================================================

        enum TimeOfDay
        {
            None,
            Sunrise,
            Day,
            Sunset,
            Night
        }


        // ======================================================================
        // Unity Lifecycle
        // ======================================================================

        void Awake()
        {
            CacheUI();
            HookInteractions();
            SyncFromSystems();
            RefreshVisualState();
            UpdateTimeIcon(
                dayNightManager != null
                    ? dayNightManager.cycleValue
                    : 0f
            );
            UpdateInfoText();
        }

        void Update()
        {
            if (Application.isPlaying && IsToggleUIKeyPressed())
            {
                _uiVisible = !_uiVisible;
                _root.style.display =
                    _uiVisible
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
            }

            if (dayNightManager != null &&
                dayNightManager.cyclePlaying)
            {
                float value = dayNightManager.cycleValue;

                _ignoreSliderCallback = true;
                _timeSlider.SetValueWithoutNotify(value);
                _ignoreSliderCallback = false;

                UpdateTimeIcon(value);
            }

            UpdateCinematicUIVisibility();
        }


        // ======================================================================
        // UI Wiring
        // ======================================================================

        void CacheUI()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _panelStyle = _root.Q<VisualElement>("ColStyle");
            _panelDayNightControls = _root.Q<VisualElement>("ColDayNightCycle");

            _toggleBaseInfo = _root.Q<VisualElement>("ToggleBaseInfo");
            _toggleCheckInfo = _root.Q<Image>("ToggleCheckInfo");

            _toggleBaseFreeCam = _root.Q<VisualElement>("ToggleBaseFreeCam");
            _toggleCheckFreeCam = _root.Q<Image>("ToggleCheckFreecam");

            _toggleBaseInteractionMode =
                _root.Q<VisualElement>("ToggleBaseInteractionMode");
            _toggleCheckInteractionMode =
                _root.Q<Image>("ToggleCheckInteractionMode");

            _toggleBaseDayNightPlay =
                _root.Q<VisualElement>("ToggleBaseDayNightPlay");
            _toggleCheckDayNightPlay =
                _root.Q<Image>("ToggleCheckDayNightPlay");

            _timeSlider = _root.Q<Slider>("SliderTime");
            _timeIcon = _root.Q<Image>("ImgTime");

            _informationPanel =
                _root.Q<VisualElement>("ColInformation");

            _cinematicUI =
                _root.Q<VisualElement>("ColCinCameraUI");
            _cinArrowLeft =
                _root.Q<Image>("BtnCinCamUIArrowLeft");
            _cinArrowRight =
                _root.Q<Image>("BtnCinCamUIArrowRight");

            _btnStyleLeft = _root.Q<VisualElement>("BtnStyleLeft");
            _btnStyleRight = _root.Q<VisualElement>("BtnStyleRight");
            _lblStyleName = _root.Q<Label>("LblStyleName");

            _infoHeading = _root.Q<Label>("TxtInfoHeading");
            _infoContent = _root.Q<Label>("TxtInfoContent");
        }

        void HookInteractions()
        {
            _toggleBaseInfo.RegisterCallback<ClickEvent>(
                _ => ToggleInfo());

            _toggleBaseFreeCam.RegisterCallback<ClickEvent>(
                _ => ToggleCameraMode());

            _toggleBaseInteractionMode.RegisterCallback<ClickEvent>(
                _ => ToggleInteractionMode());

            _toggleBaseDayNightPlay.RegisterCallback<ClickEvent>(
                _ => ToggleDayNightPlay());

            _timeSlider.RegisterValueChangedCallback(evt =>
            {
                if (_ignoreSliderCallback)
                    return;

                OnTimeSliderChanged(evt.newValue);
            });

            _cinArrowLeft.RegisterCallback<ClickEvent>(
                _ => StepCinematic(-1));
            _cinArrowRight.RegisterCallback<ClickEvent>(
                _ => StepCinematic(1));

            _btnStyleLeft.RegisterCallback<ClickEvent>(
                _ => ChangeStyle(false));
            _btnStyleRight.RegisterCallback<ClickEvent>(
                _ => ChangeStyle(true));
        }


        // ======================================================================
        // System Sync
        // ======================================================================

        void SyncFromSystems()
        {
            if (dayNightManager != null)
            {
                _dayNightPlaying =
                    dayNightManager.cyclePlaying;

                _timeSlider.SetValueWithoutNotify(
                    dayNightManager.cycleValue);
            }

            if (demoCamera != null)
            {
                _freeCamEnabled =
                    demoCamera.mode ==
                    SSHDemoCameraMode.Freecam;
            }
        }


        // ======================================================================
        // UI Actions
        // ======================================================================

        void ChangeStyle(bool directionForward)
        {
            if (styleSwitcher == null)
                return;

            int count = styleSwitcher.GetStylePresetCount();
            if (count == 0)
                return;

            int currentIndex = styleSwitcher.GetCurrentStyleIndex();

            int newIndex = directionForward
                ? currentIndex + 1
                : currentIndex - 1;

            // Wrap around
            if (newIndex < 0)
                newIndex = count - 1;
            else if (newIndex >= count)
                newIndex = 0;

            if (styleSwitcher.ApplyStylePreset(newIndex))
            {
                UpdateStyleLabel();
            }
        }

        void UpdateStyleLabel()
        {
            if (_lblStyleName == null)
                return;

            if (styleSwitcher == null ||
                styleSwitcher.GetStylePresetCount() == 0)
            {
                _lblStyleName.text = "No Presets";
                return;
            }

            _lblStyleName.text = styleSwitcher.GetCurrentStyleName();
        }
        void ToggleInfo()
        {
            _infoEnabled = !_infoEnabled;
            ApplyInfoState();
        }

        void ToggleCameraMode()
        {
            _freeCamEnabled = !_freeCamEnabled;

            if (demoCamera != null)
            {
                if (_freeCamEnabled)
                    demoCamera.EnterFreecamMode();
                else
                    demoCamera.EnterCinematicMode();
            }

            RefreshVisualState();
            UpdateInfoText();
        }

        void ToggleDayNightPlay()
        {
            _dayNightPlaying = !_dayNightPlaying;

            if (dayNightManager != null)
                dayNightManager.cyclePlaying =
                    _dayNightPlaying;

            _toggleCheckDayNightPlay.style.display =
                _dayNightPlaying
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        void OnTimeSliderChanged(float value)
        {
            if (dayNightManager != null)
            {
                dayNightManager.cycleValue = value;
                dayNightManager.cyclePlaying = false;
            }

            _dayNightPlaying = false;
            _toggleCheckDayNightPlay.style.display =
                DisplayStyle.None;

            UpdateTimeIcon(value);
        }

        void ToggleInteractionMode()
        {
            _interactionModeEnabled = !_interactionModeEnabled;
            ApplyInteractionModeState();
        }

        // ======================================================================
        // Interaction Mode State
        // ======================================================================

        void ApplyInteractionModeState()
        {
            _toggleCheckInteractionMode.style.display =
                _interactionModeEnabled
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            if (_interactionModeEnabled)
                EnterInteractionMode();
            else
                ExitInteractionMode();
        }

        void EnterInteractionMode()
        {

            if (_panelStyle != null)
                _panelStyle.style.display = DisplayStyle.Flex;

            if (_panelDayNightControls != null)
                _panelDayNightControls.style.display = DisplayStyle.None;

            dayNightManager.SetSystemEnabled(false);
            styleSwitcher.ApplyStylePreset(styleSwitcher.GetCurrentStyleIndex());
            UpdateStyleLabel();

        }

        void ExitInteractionMode()
        {
            if (_panelDayNightControls != null)
                _panelDayNightControls.style.display = DisplayStyle.Flex;

            if (_panelStyle != null)
                _panelStyle.style.display = DisplayStyle.None;

            dayNightManager.SetSystemEnabled(true);
        }

        // ======================================================================
        // Visual State
        // ======================================================================

        void UpdateTimeIcon(float value)
        {
            value = Mathf.Repeat(value, 1f);

            TimeOfDay newTime =
                value < 0.25f ? TimeOfDay.Sunrise :
                value < 0.5f ? TimeOfDay.Day :
                value < 0.75f ? TimeOfDay.Sunset :
                                TimeOfDay.Night;

            if (newTime == _currentTimeOfDay)
                return;

            _currentTimeOfDay = newTime;

            _timeIcon.sprite = newTime switch
            {
                TimeOfDay.Sunrise => iconSunrise,
                TimeOfDay.Day => iconDay,
                TimeOfDay.Sunset => iconSunset,
                TimeOfDay.Night => iconNight,
                _ => _timeIcon.sprite
            };
        }

        void UpdateCinematicUIVisibility()
        {
            if (_cinematicUI == null ||
                demoCamera == null)
                return;

            bool show =
                demoCamera.mode ==
                SSHDemoCameraMode.Cinematic;

            _cinematicUI.style.display =
                show
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        void StepCinematic(int direction)
        {
            if (demoCamera == null ||
                demoCamera.cinematicPresets == null ||
                demoCamera.cinematicPresets.Length == 0)
                return;

            _currentCinematicIndex =
                (_currentCinematicIndex +
                 direction +
                 demoCamera.cinematicPresets.Length)
                % demoCamera.cinematicPresets.Length;

            demoCamera.MoveToCinematicPreset(
                _currentCinematicIndex);
        }

        void ApplyInfoState()
        {
            _toggleCheckInfo.style.display =
                _infoEnabled
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            _informationPanel.style.display =
                _infoEnabled
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }

        void UpdateInfoText()
        {
            if (_freeCamEnabled)
            {
                _infoHeading.text =
                    "Information: Freecam";

                _infoContent.text =
                    "- RMB + Mouse: Look\n" +
                    "- WASD / QE: Move\n" +
                    "- F4: Toggle UI";
            }
            else
            {
                _infoHeading.text =
                    "Information: Cinematic Camera";

                _infoContent.text =
                    "- Arrows (Or A/D): Change Shot\n" +
                    "- F4: Toggle UI";
            }
        }

        void RefreshVisualState()
        {
            ApplyInfoState();

            _toggleCheckFreeCam.style.display =
                _freeCamEnabled
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            _toggleCheckDayNightPlay.style.display =
                _dayNightPlaying
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;

            // Ensure interaction mode drives panel visibility + toggle check
            ApplyInteractionModeState();

            UpdateCinematicUIVisibility();
        }


        // ======================================================================
        // Input
        // ======================================================================

        bool IsToggleUIKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null &&
                   Keyboard.current.f4Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.F4);
#else
            return false;
#endif
        }
    }

}