# Style Configuration

This document describes how **Hardline style values are authored, stored, and used**.

The workflow is:

1. You **author a style** in the `SSHStyleManager` inspector.  
2. When the look is correct, you **save it as a preset** (``` SSHStyleAsset ```).  
3. At runtime, gameplay systems **apply or blend presets** using the Hardline API.

The inspector is your **authoring tool**.  
Presets are your **transport format**.  
The API is your **runtime control surface**.

---

# Environment

The Environment controls **global lighting and shadowing**.  
These values define how all materials are shaded.

This is not Unity lighting.  
These values drive Hardline’s internal environment model.

---

## Shadow Color

The global tint applied to shadowed regions.

This color is multiplied into shadowed pixels and defines the overall shadow mood.

**Range**  
0.0 → 2.0

**Inspector**  
`Environment → Shadows → Shadow Color`

**API**  
``` API.Environment.SetShadowColor(Color) ```  
``` API.Environment.GetShadowColor() ```

---

## Shadow Intensity

The strength of the shadow contribution.

Higher values produce darker, more dominant shadows.  
Lower values soften or remove shadowing.

**Range**  
0.0 → 2.0

**Inspector**  
`Environment → Shadows → Shadow Intensity`

**API**  
``` API.Environment.SetShadowIntensity(float) ```  
``` API.Environment.GetShadowIntensity() ```

---

## Shadow Clip

Controls how shadows become solid bands instead of smooth gradients.

Higher values create chunkier, more graphic shadow shapes.

**Range**  
0.0 → 1.0

**Inspector**  
`Environment → Shadows → Shadow Clip`

**API**  
``` API.Environment.SetShadowClip(float) ```  
``` API.Environment.GetShadowClip() ```

---

## Shadow Range Multiplier

Scales how far shadow cancellation reaches from occlusion volumes.

Higher values make shadows persist farther.  
Lower values cause shadows to cancel sooner.

**Range**  
0.0 → 4.0

**Inspector**  
`Environment → Shadows → Shadow Range Mult`

**API**  
``` API.Environment.SetShadowRangeMult(float) ```  
``` API.Environment.GetShadowRangeMult() ```

---

## Environment Light Color

The global ambient fill color applied to all materials.

**Range**  
Color

**Inspector**  
`Environment → Environment Light → Light Color`

**API**  
``` API.Environment.SetEnvironmentLightColor(Color) ```  
``` API.Environment.GetEnvironmentLightColor() ```

---

## Environment Light Intensity

The strength of the global ambient environment light.

**Range**  
0.0 → 2.0

**Inspector**  
`Environment → Environment Light → Light Intensity`

**API**  
``` API.Environment.SetEnvironmentLightIntensity(float) ```  
``` API.Environment.GetEnvironmentLightIntensity() ```

---

## Environment Light Influence

Controls how strongly additional lights cancel the environment light.

**Range**  
0.0 → 1.0

**Inspector**  
`Environment → Environment Light → Light Influence`

**API**  
``` API.Environment.SetEnvironmentLightInfluence(float) ```  
``` API.Environment.GetEnvironmentLightInfluence() ```

---

# Skybox

The Skybox controls **all background rendering** in Hardline.

---

## Base Color

Primary sky color.

**Inspector**  
`Skybox → Colors → Base`

**API**  
``` API.Skybox.SetBaseColor(Color) ```  
``` API.Skybox.GetBaseColor() ```

---

## Accent Color

Secondary sky color.

**Inspector**  
`Skybox → Colors → Accent`

**API**  
``` API.Skybox.SetAccentColor(Color) ```  
``` API.Skybox.GetAccentColor() ```

---

## Stars Color

Star tint.

**Inspector**  
`Skybox → Colors → Stars`

**API**  
``` API.Skybox.SetStarsColor(Color) ```  
``` API.Skybox.GetStarsColor() ```

---

## Band Intensity

Strength of sky bands.

**Range**  
0.0 → 1.0

**Inspector**  
`Skybox → Bands → Intensity`

**API**  
``` API.Skybox.SetBandIntensity(float) ```  
``` API.Skybox.GetBandIntensity() ```

---

## Band Add

Vertical band offset.

**Range**  
-1.0 → 1.0

**Inspector**  
`Skybox → Bands → Add`

**API**  
``` API.Skybox.SetBandAdd(float) ```  
``` API.Skybox.GetBandAdd() ```

---

## Band Horizontal Width

Band height.

**Range**  
0.0 → 1.0

**Inspector**  
`Skybox → Bands → Horizontal Width`

**API**  
``` API.Skybox.SetBandHorizontalWidth(float) ```  
``` API.Skybox.GetBandHorizontalWidth() ```

---

## Band Contrast

Band sharpness.

**Range**  
0.0 → 1.0

**Inspector**  
`Skybox → Bands → Contrast`

**API**  
``` API.Skybox.SetBandContrast(float) ```  
``` API.Skybox.GetBandContrast() ```

---

## Starmap Density

Star count.

**Range**  
100 → 1000

**Inspector**  
`Skybox → Starmap → Density`

**API**  
``` API.Skybox.SetStarmapDensity(float) ```  
``` API.Skybox.GetStarmapDensity() ```

---

## Starmap Scale

Star size.

**Range**  
0.05 → 0.5

**Inspector**  
`Skybox → Starmap → Scale`

**API**  
``` API.Skybox.SetStarmapScale(float) ```  
``` API.Skybox.GetStarmapScale() ```

---

## Starmap Seed

Star randomness.

**Range**  
0.0 → 0.5

**Inspector**  
`Skybox → Starmap → Seed`

**API**  
``` API.Skybox.SetStarmapSeed(float) ```  
``` API.Skybox.GetStarmapSeed() ```

---

## Starmap Tilt

Starfield rotation.

**Range**  
20 → 100

**Inspector**  
`Skybox → Starmap → Tilt`

**API**  
``` API.Skybox.SetStarmapTilt(float) ```  
``` API.Skybox.GetStarmapTilt() ```

---

# Outlines

---

## Color Outline Color

**Inspector**  
`Outlines → Color Outline → Color`

**API**  
``` API.Outline.SetColorOutlineColor(Color) ```  
``` API.Outline.GetColorOutlineColor() ```

---

## Color Outline Strength

**Range**  
0.05 → 3.0

**Inspector**  
`Outlines → Color Outline → Strength`

**API**  
``` API.Outline.SetColorOutlineStrength(float) ```  
``` API.Outline.GetColorOutlineStrength() ```

---

## Color Outline Noise Scale

**Range**  
0.0 → 10.0

**Inspector**  
`Outlines → Color Outline → Noise Scale`

**API**  
``` API.Outline.SetColorOutlineNoiseScale(float) ```  
``` API.Outline.GetColorOutlineNoiseScale() ```

---

## Color Outline Noise Intensity

**Range**  
0.0 → 5.0

**Inspector**  
`Outlines → Color Outline → Noise Intensity`

**API**  
``` API.Outline.SetColorOutlineNoiseIntensity(float) ```  
``` API.Outline.GetColorOutlineNoiseIntensity() ```

---

## Color Outline Noise Add

**Range**  
0.0 → 1.0

**Inspector**  
`Outlines → Color Outline → Noise Add`

**API**  
``` API.Outline.SetColorOutlineNoiseAdd(float) ```  
``` API.Outline.GetColorOutlineNoiseAdd() ```

---

## Color Outline Clip

**Range**  
0.0 → 1.0

**Inspector**  
`Outlines → Color Outline → Clip`

**API**  
``` API.Outline.SetColorOutlineClip(float) ```  
``` API.Outline.GetColorOutlineClip() ```

---

## Geo Outline Color

**Inspector**  
`Outlines → Geo Outline → Color`

**API**  
``` API.Outline.SetGeoOutlineColor(Color) ```  
``` API.Outline.GetGeoOutlineColor() ```

---

## Geo Normal Strength

**Range**  
0.4 → 4.0

**Inspector**  
`Outlines → Geo Outline → Normal Strength`

**API**  
``` API.Outline.SetGeoOutlineNormalStrength(float) ```  
``` API.Outline.GetGeoOutlineNormalStrength() ```

---

## Geo Normal Cutoff Strength

**Range**  
0.0 → 10.0

**Inspector**  
`Outlines → Geo Outline → Normal Cutoff Strength`

**API**  
``` API.Outline.SetGeoOutlineNormalCutoffMaskStrength(float) ```  
``` API.Outline.GetGeoOutlineNormalCutoffMaskStrength() ```

---

## Geo Normal Cutoff Add

**Range**  
-10.0 → 10.0

**Inspector**  
`Outlines → Geo Outline → Normal Cutoff Add`

**API**  
``` API.Outline.SetGeoOutlineNormalCutoffMaskAdd(float) ```  
``` API.Outline.GetGeoOutlineNormalCutoffMaskAdd() ```

---

## Geo Depth Strength Start

**Range**  
0.04 → 10.0

**Inspector**  
`Outlines → Geo Outline → Depth Strength Start`

**API**  
``` API.Outline.SetGeoOutlineDepthStrengthStart(float) ```  
``` API.Outline.GetGeoOutlineDepthStrengthStart() ```

---

## Geo Depth Strength End

**Range**  
0.0 → 10.0

**Inspector**  
`Outlines → Geo Outline → Depth Strength End`

**API**  
``` API.Outline.SetGeoOutlineDepthStrengthEnd(float) ```  
``` API.Outline.GetGeoOutlineDepthStrengthEnd() ```

---

## Geo Depth Add

**Range**  
0.0 → 10.0

**Inspector**  
`Outlines → Geo Outline → Depth Add`

**API**  
``` API.Outline.SetGeoOutlineDepthAdd(float) ```  
``` API.Outline.GetGeoOutlineDepthAdd() ```

---

## Geo Depth Mult

**Range**  
0.0 → 10.0

**Inspector**  
`Outlines → Geo Outline → Depth Mult`

**API**  
``` API.Outline.SetGeoOutlineDepthMult(float) ```  
``` API.Outline.GetGeoOutlineDepthMult() ```
