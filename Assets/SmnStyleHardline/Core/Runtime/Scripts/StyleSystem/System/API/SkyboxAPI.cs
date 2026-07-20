namespace SmnStyleHardline.Core
{
    /*
 * Skybox API
 * Exposes runtime control over skybox colors, bands,
 * and procedural starmap parameters.
 */

    using UnityEngine;


    // ==========================================================================
    // Skybox API
    // ==========================================================================

    public sealed class SkyboxAPI
    {
        readonly StyleControllerSkybox style;

        internal SkyboxAPI(StyleControllerSkybox styleSkybox)
        {
            style = styleSkybox;
        }


        // ======================================================================
        // Colors
        // ======================================================================

        /// <summary>
        /// Sets the base skybox color.
        /// </summary>
        public void SetBaseColor(Color v)
            => style.SetBaseColor(v);

        /// <summary>
        /// Returns the base skybox color.
        /// </summary>
        public Color GetBaseColor()
            => style.GetBaseColor();

        /// <summary>
        /// Sets the accent skybox color.
        /// </summary>
        public void SetAccentColor(Color v)
            => style.SetAccentColor(v);

        /// <summary>
        /// Returns the accent skybox color.
        /// </summary>
        public Color GetAccentColor()
            => style.GetAccentColor();

        /// <summary>
        /// Sets the star color used by the skybox.
        /// </summary>
        public void SetStarsColor(Color v)
            => style.SetStarsColor(v);

        /// <summary>
        /// Returns the star color used by the skybox.
        /// </summary>
        public Color GetStarsColor()
            => style.GetStarsColor();


        // ======================================================================
        // Bands
        // ======================================================================

        /// <summary>
        /// Sets the intensity of skybox bands.
        /// <para>Valid range: <c>0.0 – 1.0</c></para>
        /// </summary>
        public void SetBandIntensity(float v)
            => style.SetBandIntensity(v);

        /// <summary>
        /// Returns the intensity of skybox bands.
        /// <para>Range: <c>0.0 – 1.0</c></para>
        /// </summary>
        public float GetBandIntensity()
            => style.GetBandIntensity();

        /// <summary>
        /// Sets the additive contribution of skybox bands.
        /// <para>Valid range: <c>-1.0 – 1.0</c></para>
        /// </summary>
        public void SetBandAdd(float v)
            => style.SetBandAdd(v);

        /// <summary>
        /// Returns the additive contribution of skybox bands.
        /// <para>Range: <c>-1.0 – 1.0</c></para>
        /// </summary>
        public float GetBandAdd()
            => style.GetBandAdd();

        /// <summary>
        /// Sets the horizontal width of skybox bands.
        /// <para>Valid range: <c>0.0 – 1.0</c></para>
        /// </summary>
        public void SetBandHorizontalWidth(float v)
            => style.SetBandHorizontalWidth(v);

        /// <summary>
        /// Returns the horizontal width of skybox bands.
        /// <para>Range: <c>0.0 – 1.0</c></para>
        /// </summary>
        public float GetBandHorizontalWidth()
            => style.GetBandHorizontalWidth();

        /// <summary>
        /// Sets the contrast of skybox bands.
        /// <para>Valid range: <c>0.0 – 1.0</c></para>
        /// </summary>
        public void SetBandContrast(float v)
            => style.SetBandContrast(v);

        /// <summary>
        /// Returns the contrast of skybox bands.
        /// <para>Range: <c>0.0 – 1.0</c></para>
        /// </summary>
        public float GetBandContrast()
            => style.GetBandContrast();


        // ======================================================================
        // Starmap
        // ======================================================================

        /// <summary>
        /// Sets the density of the procedural starmap.
        /// <para>Valid range: <c>100 – 1000</c></para>
        /// </summary>
        public void SetStarmapDensity(float v)
            => style.SetStarmapDensity(v);

        /// <summary>
        /// Returns the density of the procedural starmap.
        /// <para>Range: <c>100 – 1000</c></para>
        /// </summary>
        public float GetStarmapDensity()
            => style.GetStarmapDensity();

        /// <summary>
        /// Sets the scale of the procedural starmap.
        /// <para>Valid range: <c>0.05 – 0.5</c></para>
        /// </summary>
        public void SetStarmapScale(float v)
            => style.SetStarmapScale(v);

        /// <summary>
        /// Returns the scale of the procedural starmap.
        /// <para>Range: <c>0.05 – 0.5</c></para>
        /// </summary>
        public float GetStarmapScale()
            => style.GetStarmapScale();

        /// <summary>
        /// Sets the seed value used by the procedural starmap.
        /// <para>Valid range: <c>0.0 – 0.5</c></para>
        /// </summary>
        public void SetStarmapSeed(float v)
            => style.SetStarmapSeed(v);

        /// <summary>
        /// Returns the seed value used by the procedural starmap.
        /// <para>Range: <c>0.0 – 0.5</c></para>
        /// </summary>
        public float GetStarmapSeed()
            => style.GetStarmapSeed();

        /// <summary>
        /// Sets the tilt of the procedural starmap.
        /// <para>Valid range: <c>20 – 100</c></para>
        /// </summary>
        public void SetStarmapTilt(float v)
            => style.SetStarmapTilt(v);

        /// <summary>
        /// Returns the tilt of the procedural starmap.
        /// <para>Range: <c>20 – 100</c></para>
        /// </summary>
        public float GetStarmapTilt()
            => style.GetStarmapTilt();
    }

}