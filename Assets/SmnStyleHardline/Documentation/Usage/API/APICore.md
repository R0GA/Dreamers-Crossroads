# Core API

The Core API exposes global system state and lifecycle control for **Summon Style: Hardline**.

This API is responsible for:
- Which `SSHSystemAsset` is active
- How often environment lighting is synchronized
- Forcing immediate environment updates

All functions operate on the currently active `SSHStyleManager`.

---

## System Asset

### `GetSystemAsset() → SSHSystemAsset`

Returns the currently assigned system asset.

If no system asset is active, this returns `null`.

The system asset contains:
- Environment configuration
- Skybox configuration
- Outline configuration
- All global style state

---

### `SetSystemAsset(SSHSystemAsset asset)`

Assigns a new system asset and reinitializes all style controllers.

This:
- Rebinds all environment, skybox, and outline state
- Pushes all values into shaders
- Rebuilds runtime style state

This is equivalent to changing the System Asset in the `SSHStyleManager` inspector.

---

### `HasSystemAsset() → bool`

Returns whether a system asset is currently assigned.

This is equivalent to checking:

`GetSystemAsset() `= null`

---

## Environment Synchronization

The environment system periodically scans Unity lights and uploads their data to shaders.  
This is controlled by a frame-based tick system.

---

### `SetLightSyncTick(int tick)`

Sets how often environment light synchronization runs.

This is measured in frames.

Examples:
- `1` → update every frame  
- `10` → update every 10 frames  
- `60` → update once per second at 60 FPS  

Values less than `1` are clamped internally.

Lower values:
- More responsive light updates  
- Higher CPU cost  

Higher values:
- Less frequent updates  
- Lower CPU cost  

---

### `GetLightSyncTick() → int`

Returns the current frame interval between environment light sync operations.

---

### `ForceEnvironmentTick()`

Immediately performs a full environment update.

This:
- Re-evaluates all lights
- Rebuilds additional-light data
- Uploads new values to shaders

Use this after:
- Enabling or disabling lights
- Moving lights
- Teleporting the camera
- Making large scene changes

---

## Notes

Core API calls affect the entire scene.  
There is always exactly one active system asset per `SSHStyleManager`.
