# Preset API

The Preset API applies complete style snapshots to the active system.

A preset is stored in an `SSHStyleAsset`.  
It contains a full copy of:

- Environment state
- Skybox state
- Outline state

Applying a preset updates all style controllers at once.

---

## Applying Presets

### `Apply(SSHStyleAsset asset)`

Applies the given style preset to the current system state.

This:
- Overwrites all style values
- Updates shaders
- Updates skybox
- Updates outlines
- Updates environment lighting

This is the same operation used by:
- The demo day/night cycle
- Preset blending systems
- Editor “Load Preset” buttons

Presets are static snapshots.  
They do not contain animation or curves.

To animate styles:
- Blend multiple presets
- Interpolate values
- Apply the result via the API

---

## Notes

Applying a preset affects the entire scene.

All cameras, materials, skybox, and outlines update immediately.
