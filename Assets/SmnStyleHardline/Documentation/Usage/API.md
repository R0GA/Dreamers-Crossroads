# API Reference

The Hardline API is how gameplay systems and runtime logic interact with the style system.  
It provides a stable, centralized way to control all visual state without touching materials, lights, or render pipeline configuration.

All interaction goes through a single entry point:

`SSHStyleManager.API`

This object exposes the full runtime control surface for the active Hardline system.

---

## Lifetime and Safety

The API is only valid while a scene contains an active `SSHStyleManager`.

Internally, the API is bound to Unity’s scene and render lifecycle.  
If no manager exists, the API will be null.

Always guard your calls:

` var api = SSHStyleManager.API;  
` if (api == null) return; `

This prevents systems from attempting to write to a scene that has no active style controller.

---

## API Structure

The root API is split into domain-specific sub-APIs.

| API | Responsibility |
|-----|----------------|
| `Core` | Global system state and runtime control |
| `Preset` | Apply complete style presets |
| `Environment` | Ambient light, shadows, and additional lights |
| `Skybox` | Procedural skybox and stars |
| `Outline` | Color and geometry-based outlines |

All are accessed from the same root:

` SSHStyleManager.API.Environment `  
` SSHStyleManager.API.Skybox `  
` SSHStyleManager.API.Outline `

---

## Core

System-level controls and lifecycle state.

Used to:
- Read or assign the active `SSHSystemAsset`  
- Control environment synchronization frequency  
- Force immediate environment updates  

→ [APICore.md](./API/APICore.md)

---

## Preset

Applies complete `SSHStyleAsset` snapshots.

Used to:
- Load a preset  
- Switch or blend styles at runtime  

This is how the demo day/night system applies styles.

→ [APIPreset.md](./API/APIPreset.md)

---

## Environment

Controls all global lighting and shadow state.

Provides access to:
- Shadow color, intensity, clip, and range  
- Environment light color and intensity  
- Environment influence  
- Additional light synchronization and data  

Used by:
- Time of day systems  
- Weather and mood systems  
- Interior / exterior transitions  

→ [APIEnvironment.md](./API/APIEnvironment.md)

---

## Skybox

Controls the procedural sky rendering.

Provides:
- Base, accent, and star colors  
- Band intensity, width, and contrast  
- Procedural star density, scale, seed, and tilt  

→ [APISkybox.md](./API/APISkybox.md)

---

## Outline

Controls all outline rendering.

Provides:
- Color-based outline parameters  
- Geometry-based depth and normal response  
- Noise and clipping controls  

→ [APIOutline.md](./API/APIOutline.md)

