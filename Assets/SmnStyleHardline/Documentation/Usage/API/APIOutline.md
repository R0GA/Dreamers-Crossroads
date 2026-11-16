# Outline API

The Outline API controls all outline rendering in Hardline.

This includes:
- Color-based outlines
- Geometry-based outlines
- Noise, clipping, and depth response

Outlines are global and screen-space.

---

## Color Outlines

These are outlines that are based on the color differences around the target pixel.

---

### `SetColorOutlineColor(Color v)`
### `GetColorOutlineColor() → Color`

Controls the outline color.

---

### `SetColorOutlineStrength(float v)`
### `GetColorOutlineStrength() → float`

Controls line thickness.

Valid range:
`0.05 – 3.0`

---

### `SetColorOutlineNoiseScale(float v)`
### `GetColorOutlineNoiseScale() → float`

Controls the scale of stylized line noise.

Valid range:
`0.0 – 10.0`

---

### `SetColorOutlineNoiseIntensity(float v)`
### `GetColorOutlineNoiseIntensity() → float`

Controls how strong the noise distortion is.

Valid range:
`0.0 – 5.0`

---

### `SetColorOutlineNoiseAdd(float v)`
### `GetColorOutlineNoiseAdd() → float`

Removes noise from the base outline.

Valid range:
`0.0 – 1.0`

---

### `SetColorOutlineClip(float v)`
### `GetColorOutlineClip() → float`

Controls when outlines disappear based on depth.

Valid range:
`0.0 – 1.0`

---

## Geometry Outlines

These are edge-based outlines derived from normals and depth.

---

### `SetGeoOutlineColor(Color v)`
### `GetGeoOutlineColor() → Color`

Controls geometry outline color.

---

### `SetGeoOutlineNormalStrength(float v)`
### `GetGeoOutlineNormalStrength() → float`

Controls how strongly normals generate edges.

Valid range:
`0.4 – 4.0`

---

### `SetGeoOutlineNormalCutoffMaskStrength(float v)`
### `GetGeoOutlineNormalCutoffMaskStrength() → float`

Controls the height cutoff for the Geometry based outlines,
specifically how aggresively the cutoff happens.

Valid range:
`0.0 – 10.0`

---

### `SetGeoOutlineNormalCutoffMaskAdd(float v)`
### `GetGeoOutlineNormalCutoffMaskAdd() → float`

Offsets the cutoff mask.

Valid range:
`-10.0 – 10.0`

---

### `SetGeoOutlineDepthStrengthStart(float v)`
### `GetGeoOutlineDepthStrengthStart() → float`

Depth response near the camera.

Valid range:
`0.04 – 10.0`

---

### `SetGeoOutlineDepthStrengthEnd(float v)`
### `GetGeoOutlineDepthStrengthEnd() → float`

Depth response far from the camera.

Valid range:
`0.0 – 10.0`

---

### `SetGeoOutlineDepthAdd(float v)`
### `GetGeoOutlineDepthAdd() → float`

Adds depth-based outline contribution.

Valid range:
`0.0 – 10.0`

---

### `SetGeoOutlineDepthMult(float v)`
### `GetGeoOutlineDepthMult() → float`

Multiplies depth-based outline strength.

Valid range:
`0.0 – 10.0`
