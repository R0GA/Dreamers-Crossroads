# Skybox API

The Skybox API controls the procedural Hardline sky.

This includes:
- Base and accent colors
- Gradient banding
- Procedural starfield

All skybox rendering is screen-space and global.

---

## Colors

### `SetBaseColor(Color v)`
### `GetBaseColor() → Color`

Controls the primary sky color.

---

### `SetAccentColor(Color v)`
### `GetAccentColor() → Color`

Controls the secondary gradient color.

---

### `SetStarsColor(Color v)`
### `GetStarsColor() → Color`

Controls the tint of the procedural stars.

---

## Bands

### `SetBandIntensity(float v)`
### `GetBandIntensity() → float`

Controls how strong the gradient bands are.

Valid range:
`0.0 – 1.0`

---

### `SetBandAdd(float v)`
### `GetBandAdd() → float`

Offsets the bands upward or downward.

Valid range:
`-1.0 – 1.0`

---

### `SetBandHorizontalWidth(float v)`
### `GetBandHorizontalWidth() → float`

Controls how wide bands are vertically.

Valid range:
`0.0 – 1.0`

---

### `SetBandContrast(float v)`
### `GetBandContrast() → float`

Controls how sharp the band transitions are.

Valid range:
`0.0 – 1.0`

---

## Starmap

### `SetStarmapDensity(float v)`
### `GetStarmapDensity() → float`

Controls how dense the staremap is (similar to scale).

Valid range:
`100 – 1000`

---

### `SetStarmapScale(float v)`
### `GetStarmapScale() → float`

Controls the size of stars.

Valid range:
`0.05 – 0.5`

---

### `SetStarmapSeed(float v)`
### `GetStarmapSeed() → float`

Controls amount of stars seeded into the sky.

Valid range:
`0.0 – 0.5`

---

### `SetStarmapTilt(float v)`
### `GetStarmapTilt() → float`

Tilts the starfield.

Valid range:
`20 – 100`
