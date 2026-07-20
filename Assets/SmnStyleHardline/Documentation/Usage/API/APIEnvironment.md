# Environment API

The Environment API controls all global lighting and shadow behavior in Hardline.

This includes:
- Shadow color and strength
- Shadow cutoff and range
- Environment light color and intensity
- Additional light masks

This API does not control real Unity lighting.
It controls how the Hardline environment system shades the scene.

---

## Shadows

### `SetShadowColor(Color v)`
### `GetShadowColor() → Color`

Controls the global tint applied to all shadows.

---

### `SetShadowIntensity(float v)`
### `GetShadowIntensity() → float`

Multiplier applied to shadow strength.

Valid range:
`0.0 – 2.0`

Higher values produce darker shadows.

---

### `SetShadowClip(float v)`
### `GetShadowClip() → float`

Controls where shadow bands clip.

Valid range:
`0.0 – 1.0`

Higher values bigger, chunky shadows. This is at what darkness in the shadow sample we should cut out for the solid shadow.

E.g. Distance from a light source shadows are cancelled out.

---

### `SetShadowRangeMult(float v)`
### `GetShadowRangeMult() → float`

Scales how far from occlusion sources at which shadows are cancelled.

Valid range:
`0.0 – 4.0`

---

## Environment Light

### `SetEnvironmentLightColor(Color v)`
### `GetEnvironmentLightColor() → Color`

Controls the ambient fill color applied to all materials.

---

### `SetEnvironmentLightIntensity(float v)`
### `GetEnvironmentLightIntensity() → float`

Controls how strong the environment light is.

Valid range:
`0.0 – 2.0`

---

### `SetEnvironmentLightInfluence(float v)`
### `GetEnvironmentLightInfluence() → float`

Controls how strongly lights cancel out environment lighting.

Valid range:
`0.0 – 1.0`

Higher values cancel out more of the environment light, useful for night-time or dark scenes, where lights should return visibility and color back to baseline.

---

## Additional Lights

These are not real lights.  
They are distance masks used to locally modify environment and shadow layers.

### `GetAdditionalLightCount() → int`

Returns how many additional lights are currently active.

This is capped by:

`StyleControllerEnvironment.MaxAdditionalLights`

---

### `GetAdditionalLight(int index) → Vector4`

Returns light data at the given slot.

The vector contains:
- `xyz` → world position  
- `w` → range  

Returns `Vector4.zero` if the index is invalid.

---

## Synchronization

### `RequestImmediateLightSync()`

Forces a rescan and upload of all additional lights.

Use this when:
- Lights are enabled or disabled
- Lights are moved
- The camera jumps large distances
