namespace SmnStyleHardline.Core
{
    /*
 * Logic Controller – Environment
 * Collects nearby point lights and feeds the closest ones into
 * the style environment for shader-side lighting support.
 */

    using UnityEngine;
    using System.Collections.Generic;

    public class LogicControllerEnvironment
    {
        // ======================================================================
        // Constants
        // ======================================================================

        const int MAX_LIGHTS = StyleControllerEnvironment.MaxAdditionalLights;
        const float LIGHT_REFRESH_INTERVAL = 1f;


        // ======================================================================
        // Cached Data
        // ======================================================================

        readonly Vector4[] additionalLightData = new Vector4[MAX_LIGHTS];
        readonly List<Light> cachedLights = new List<Light>(32);
        readonly float[] distances = new float[32];

        int additionalLightCount;
        float nextRefreshTime;

        Camera cachedCamera;


        // ======================================================================
        // References
        // ======================================================================

        StyleControllerEnvironment style;


        // ======================================================================
        // Lifecycle
        // ======================================================================

        /// <summary>
        /// Initializes the controller and clears any previously set light data.
        /// </summary>
        public void Init(StyleControllerEnvironment styleController)
        {
            style = styleController;
            CacheCamera(true);
            RefreshLightCache();
            style.ClearAdditionalLights();
        }

        /// <summary>
        /// Periodically refreshes the light cache and updates closest-light data.
        /// </summary>
        public void Tick()
        {
            CacheCamera(false);

            if (Time.time >= nextRefreshTime)
            {
                RefreshLightCache();
                nextRefreshTime = Time.time + LIGHT_REFRESH_INTERVAL;
            }

            RefreshAdditionalLightData();
        }

        /// <summary>
        /// Forces an immediate rescan and update, bypassing refresh timers.
        /// </summary>
        public void RequestImmediateLightSync()
        {
            CacheCamera(true);
            RefreshLightCache();
            RefreshAdditionalLightData();
        }


        // ======================================================================
        // Camera Handling
        // ======================================================================

        void CacheCamera(bool force)
        {
            if (!force && cachedCamera != null && cachedCamera.isActiveAndEnabled)
                return;

            cachedCamera = Camera.main;
        }


        // ======================================================================
        // Light Collection
        // ======================================================================

        void RefreshLightCache()
        {
            cachedLights.Clear();

            Light[] lights =
                Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            if (lights == null)
                return;

            for (int i = 0; i < lights.Length; i++)
            {
                Light l = lights[i];
                if (l != null && l.type == LightType.Point)
                    cachedLights.Add(l);
            }
        }


        // ======================================================================
        // Light Selection & Upload
        // ======================================================================

        void RefreshAdditionalLightData()
        {
            additionalLightCount = 0;

            if (cachedCamera == null || cachedLights.Count == 0)
            {
                style.ClearAdditionalLights();
                return;
            }

            Vector3 camPos = cachedCamera.transform.position;
            int lightCount = cachedLights.Count;

            if (distances.Length < lightCount)
                lightCount = distances.Length;

            for (int i = 0; i < lightCount; i++)
            {
                Light l = cachedLights[i];

                if (l == null || !l.isActiveAndEnabled)
                {
                    distances[i] = float.MaxValue;
                    continue;
                }

                distances[i] =
                    Vector3.SqrMagnitude(l.transform.position - camPos);
            }

            for (int slot = 0; slot < MAX_LIGHTS; slot++)
            {
                int bestIndex = -1;
                float bestDist = float.MaxValue;

                for (int i = 0; i < lightCount; i++)
                {
                    if (distances[i] < bestDist)
                    {
                        bestDist = distances[i];
                        bestIndex = i;
                    }
                }

                if (bestIndex < 0 || bestDist == float.MaxValue)
                    break;

                Light l = cachedLights[bestIndex];
                Vector3 pos = l.transform.position;

                additionalLightData[additionalLightCount] =
                    new Vector4(pos.x, pos.y, pos.z, l.range);

                additionalLightCount++;
                distances[bestIndex] = float.MaxValue;
            }

            style.ClearAdditionalLights();

            for (int i = 0; i < additionalLightCount; i++)
                style.SetAdditionalLight(i, additionalLightData[i]);

            style.SetAdditionalLightCount(additionalLightCount);
        }
    }

}