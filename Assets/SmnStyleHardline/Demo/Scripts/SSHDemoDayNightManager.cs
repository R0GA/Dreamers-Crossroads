namespace SmnStyleHardline.Demo
{
    /*
 * SSH Demo Day/Night Manager
 * Demonstrates time-based blending of SSH style presets and application
 * of the resulting values through the SSH style system API.
 */

    using UnityEngine;
    using SmnStyleHardline.Core;

    // ==========================================================================
    // Demo Day / Night Manager
    // ==========================================================================

    [ExecuteAlways]
    class SSHDemoDayNightManager : MonoBehaviour
    {
        [SerializeField] bool enableSystem = true;

        // ======================================================================
        // Style Presets
        // ======================================================================
        // Presets defining the four key points of the day cycle.
        // Values are blended locally and applied via the style system.

        [SerializeField] SSHStyleAsset presetStyleSunrise;
        [SerializeField] SSHStyleAsset presetStyleDay;
        [SerializeField] SSHStyleAsset presetStyleSunset;
        [SerializeField] SSHStyleAsset presetStyleNight;


        // ======================================================================
        // Cycle Configuration
        // ======================================================================
        // 0.00 = Sunrise
        // 0.25 = Day
        // 0.50 = Sunset
        // 0.75 = Night

        [Range(0f, 1f)]
        public float cycleValue = 0.25f;

        [Min(0f)]
        public float cycleSpeed = 0.1f;

        public bool cyclePlaying = false;


        // ======================================================================
        // Cycle Remapping
        // ======================================================================
        // Optional remapping of the normalized cycle value.

        [SerializeField]
        AnimationCurve cycleRemapCurve =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);


        // ======================================================================
        // Application Throttling
        // ======================================================================
        // Controls how often blended values are pushed to the style system.

        [Min(1)]
        public int applyEveryNFrames = 1;


        // ======================================================================
        // State
        // ======================================================================

        float lastAppliedCycleValue = -1f;
        int frameCounter = 0;


        // ======================================================================
        // Unity Lifecycle
        // ======================================================================

        void Update()
        {
            if (!enableSystem)
                return;

            if (Application.isPlaying && cyclePlaying)
            {
                cycleValue += cycleSpeed * Time.deltaTime;
                cycleValue = Mathf.Repeat(cycleValue, 1f);
            }

            frameCounter++;

            bool shouldApply =
                applyEveryNFrames <= 1 ||
                frameCounter % applyEveryNFrames == 0;

            if (shouldApply)
                ApplyIfChanged();
        }

        void OnValidate()
        {
            if (!enableSystem)
                return;

            frameCounter = 0;
            ApplyIfChanged(force: true);
        }

#if UNITY_EDITOR

        void OnEnable()
        {
            if (!enableSystem)
                return;

            frameCounter = 0;
            ApplyIfChanged(force: true);
        }

#endif

        // ======================================================================
        // Public API
        // ======================================================================

        public void SetSystemEnabled(bool enabled)
        {
            enableSystem = enabled;

            if (!enableSystem)
                return;

            frameCounter = 0;
            ApplyIfChanged(force: true);
        }


        // ======================================================================
        // Cycle Evaluation
        // ======================================================================

        void ApplyIfChanged(bool force = false)
        {
            float remappedValue =
                GetRemappedCycleValue(cycleValue);

            if (!force &&
                Mathf.Approximately(
                    remappedValue,
                    lastAppliedCycleValue))
                return;

            ApplyCycleTime(remappedValue);
            lastAppliedCycleValue = remappedValue;
        }

        float GetRemappedCycleValue(float value)
        {
            value = Mathf.Repeat(value, 1f);

            if (cycleRemapCurve == null ||
                cycleRemapCurve.length == 0)
                return value;

            float remapped =
                cycleRemapCurve.Evaluate(value);

            return Mathf.Repeat(remapped, 1f);
        }


        // ======================================================================
        // Style Blending
        // ======================================================================
        // The cycle is split into four quarters.
        // Each quarter linearly blends between two adjacent presets.

        void ApplyCycleTime(float value)
        {
            value = Mathf.Repeat(value, 1f);

            if (value < 0.25f)
            {
                ApplyStyleLerp(
                    value / 0.25f,
                    presetStyleSunrise,
                    presetStyleDay
                );
            }
            else if (value < 0.5f)
            {
                ApplyStyleLerp(
                    (value - 0.25f) / 0.25f,
                    presetStyleDay,
                    presetStyleSunset
                );
            }
            else if (value < 0.75f)
            {
                ApplyStyleLerp(
                    (value - 0.5f) / 0.25f,
                    presetStyleSunset,
                    presetStyleNight
                );
            }
            else
            {
                ApplyStyleLerp(
                    (value - 0.75f) / 0.25f,
                    presetStyleNight,
                    presetStyleSunrise
                );
            }
        }


        // ======================================================================
        // Style Application
        // ======================================================================
        // Blends preset values locally and applies the results
        // through the SSHStyleAPI.

        void ApplyStyleLerp(float t, SSHStyleAsset a, SSHStyleAsset b)
        {
            if (a == null || b == null)
                return;

            if (SSHStyleManager.API == null)
                return;

            t = Mathf.Clamp01(t);

            // ----- Environment ----- //

            SSHStyleManager.API.Environment.SetShadowColor(
                Color.Lerp(a.Environment.ShadowColor, b.Environment.ShadowColor, t));
            SSHStyleManager.API.Environment.SetShadowIntensity(
                Mathf.Lerp(a.Environment.ShadowIntensity, b.Environment.ShadowIntensity, t));
            SSHStyleManager.API.Environment.SetShadowClip(
                Mathf.Lerp(a.Environment.ShadowClip, b.Environment.ShadowClip, t));
            SSHStyleManager.API.Environment.SetShadowRangeMult(
                Mathf.Lerp(a.Environment.ShadowRangeMult, b.Environment.ShadowRangeMult, t));

            SSHStyleManager.API.Environment.SetEnvironmentLightColor(
                Color.Lerp(a.Environment.EnvironmentLightColor, b.Environment.EnvironmentLightColor, t));
            SSHStyleManager.API.Environment.SetEnvironmentLightIntensity(
                Mathf.Lerp(a.Environment.EnvironmentLightIntensity, b.Environment.EnvironmentLightIntensity, t));
            SSHStyleManager.API.Environment.SetEnvironmentLightInfluence(
                Mathf.Lerp(a.Environment.EnvironmentLightInfluence, b.Environment.EnvironmentLightInfluence, t));

            SSHStyleManager.API.Environment.RequestImmediateLightSync();


            // ----- Skybox ----- //

            SSHStyleManager.API.Skybox.SetBaseColor(
                Color.Lerp(a.Skybox.BaseColor, b.Skybox.BaseColor, t));
            SSHStyleManager.API.Skybox.SetAccentColor(
                Color.Lerp(a.Skybox.AccentColor, b.Skybox.AccentColor, t));
            SSHStyleManager.API.Skybox.SetStarsColor(
                Color.Lerp(a.Skybox.StarsColor, b.Skybox.StarsColor, t));

            SSHStyleManager.API.Skybox.SetBandIntensity(
                Mathf.Lerp(a.Skybox.BandIntensity, b.Skybox.BandIntensity, t));
            SSHStyleManager.API.Skybox.SetBandAdd(
                Mathf.Lerp(a.Skybox.BandAdd, b.Skybox.BandAdd, t));
            SSHStyleManager.API.Skybox.SetBandHorizontalWidth(
                Mathf.Lerp(a.Skybox.BandHorizontalWidth, b.Skybox.BandHorizontalWidth, t));
            SSHStyleManager.API.Skybox.SetBandContrast(
                Mathf.Lerp(a.Skybox.BandContrast, b.Skybox.BandContrast, t));

            SSHStyleManager.API.Skybox.SetStarmapDensity(
                Mathf.Lerp(a.Skybox.StarmapDensity, b.Skybox.StarmapDensity, t));
            SSHStyleManager.API.Skybox.SetStarmapScale(
                Mathf.Lerp(a.Skybox.StarmapScale, b.Skybox.StarmapScale, t));
            SSHStyleManager.API.Skybox.SetStarmapSeed(
                Mathf.Lerp(a.Skybox.StarmapSeed, b.Skybox.StarmapSeed, t));
            SSHStyleManager.API.Skybox.SetStarmapTilt(
                Mathf.Lerp(a.Skybox.StarmapTilt, b.Skybox.StarmapTilt, t));


            // ----- Outline ----- //

            SSHStyleManager.API.Outline.SetColorOutlineColor(
                Color.Lerp(a.Outline.ColorOutlineColor, b.Outline.ColorOutlineColor, t));
            SSHStyleManager.API.Outline.SetColorOutlineStrength(
                Mathf.Lerp(a.Outline.ColorOutlineStrength, b.Outline.ColorOutlineStrength, t));
            SSHStyleManager.API.Outline.SetColorOutlineNoiseScale(
                Mathf.Lerp(a.Outline.ColorOutlineNoiseScale, b.Outline.ColorOutlineNoiseScale, t));
            SSHStyleManager.API.Outline.SetColorOutlineNoiseIntensity(
                Mathf.Lerp(a.Outline.ColorOutlineNoiseIntensity, b.Outline.ColorOutlineNoiseIntensity, t));
            SSHStyleManager.API.Outline.SetColorOutlineNoiseAdd(
                Mathf.Lerp(a.Outline.ColorOutlineNoiseAdd, b.Outline.ColorOutlineNoiseAdd, t));
            SSHStyleManager.API.Outline.SetColorOutlineClip(
                Mathf.Lerp(a.Outline.ColorOutlineClip, b.Outline.ColorOutlineClip, t));

            SSHStyleManager.API.Outline.SetGeoOutlineColor(
                Color.Lerp(a.Outline.GeoOutlineColor, b.Outline.GeoOutlineColor, t));
            SSHStyleManager.API.Outline.SetGeoOutlineNormalStrength(
                Mathf.Lerp(a.Outline.GeoOutlineNormalStrength, b.Outline.GeoOutlineNormalStrength, t));
            SSHStyleManager.API.Outline.SetGeoOutlineNormalCutoffMaskStrength(
                Mathf.Lerp(a.Outline.GeoOutlineNormalCutoffMaskStrength, b.Outline.GeoOutlineNormalCutoffMaskStrength, t));
            SSHStyleManager.API.Outline.SetGeoOutlineNormalCutoffMaskAdd(
                Mathf.Lerp(a.Outline.GeoOutlineNormalCutoffMaskAdd, b.Outline.GeoOutlineNormalCutoffMaskAdd, t));
            SSHStyleManager.API.Outline.SetGeoOutlineDepthStrengthStart(
                Mathf.Lerp(a.Outline.GeoOutlineDepthStrengthStart, b.Outline.GeoOutlineDepthStrengthStart, t));
            SSHStyleManager.API.Outline.SetGeoOutlineDepthStrengthEnd(
                Mathf.Lerp(a.Outline.GeoOutlineDepthStrengthEnd, b.Outline.GeoOutlineDepthStrengthEnd, t));
            SSHStyleManager.API.Outline.SetGeoOutlineDepthAdd(
                Mathf.Lerp(a.Outline.GeoOutlineDepthAdd, b.Outline.GeoOutlineDepthAdd, t));
            SSHStyleManager.API.Outline.SetGeoOutlineDepthMult(
                Mathf.Lerp(a.Outline.GeoOutlineDepthMult, b.Outline.GeoOutlineDepthMult, t));
        }
    }

}