# Limitations

This document lists known design constraints and technical limitations of **Summon Style: Hardline**.

These are intentional trade-offs made to support performance, determinism, and stylized rendering.

---

## Camera-Dependent Light Selection

The environment lighting system only samples point lights relative to **Camera.main**.

The logic controller (`LogicControllerEnvironment`) selects the closest point lights to the main camera and uploads them to shaders.  
Lights that are far from the main camera or only visible to a secondary camera may not contribute.

This can cause issues when:

- Using multiple active cameras  
- Using shadow-casting point lights for occlusion on secondary cameras  
- Running split-screen or picture-in-picture setups  

Lights near the main camera will always take priority, even if another camera is rendering a different area.

---

## Lights Are Not Illumination Sources

Unity lights do not function as traditional lighting sources.

Point lights are used only for:

- Shadow occlusion  
- Stylized additive environment shading  

They do not emit physically based light into the scene.

This is a design constraint that allows:

- Stable visual style  
- Predictable color control  
- Performance scaling  

Designers can remove or soften environment lighting by placing lights, such as:

- Lamps  
- Torches  
- Fireplaces  
- Interior fixtures  

These do not brighten objects directly; they modify how the environment lighting and shadow layers behave.

Projects that require physically accurate local lighting are not a match for this system.

---

## Limited Number of Active Lights

Only a fixed number of point lights are sent to shaders at any time.

The maximum is:


`StyleControllerEnvironment.MaxAdditionalLights = 8`


Only the closest lights to the main camera are used.

Scenes with many lights may experience:

- Popping as lights enter and leave the active set  
- Non-intuitive occlusion if many lights are clustered  

This is intentional to keep shader cost bounded.

---

## Global Style State

All style values are global shader parameters.

There is:

- One environment  
- One skybox  
- One outline configuration  

It is not possible to have:

- Per-camera styles  
- Per-area skyboxes  
- Different outline settings per object  

Regional variation must be implemented by blending styles over time or distance.

---

## Per-Object Lighting and Probes

Hardline does not support traditional per-object or probe-based lighting.

This means:

- Light probes  
- Reflection probes  
- Per-object light lists  
- Per-object light attenuation  

are not used by the shading system.

All lighting is evaluated from global environment and shadow data.

However, materials can opt in or out of this global influence.  
Shader graphs include controls that allow materials to:

- Reduce or remove environment light influence  
- Reduce or remove shadow contribution  

This allows specific materials (UI meshes, emissive objects, decals, effects, etc.) to ignore or override the global environment while still using the Hardline pipeline.

---

## Presets Are Snapshots

`SSHStyleAsset` stores snapshots of style data.

They do not:

- Store animation  
- Store curves  
- Store procedural behavior  

They are intended for:

- Instant switching  
- Manual blending  
- Runtime interpolation systems (as in the demo)

---

## Shader Graph and URP Dependency

Hardline relies on:

- Unity URP  
- Custom Shader Graph assets  
- Custom HLSL include files  

It does not support:

- Built-in render pipeline  
- HDRP  
- Third-party lighting frameworks  

---

## Post-Processing Integration

Outlines and skybox are implemented using custom full-screen passes.

Other post-processing effects may:

- Render before Hardline  
- Render after Hardline  
- Interfere with depth or normal buffers  

Compatibility with external post-processing stacks is not guaranteed.

---

## Not Physically Based

The system does not simulate:

- Energy conservation  
- Real light falloff  
- Surface roughness  
- Metallic response  

Materials are driven by:

- Color ramps  
- Masks  
- Shadow bands  
- Style parameters  

Projects that require realistic lighting should not use this pipeline.

---

## Asset-Driven State

All live values come from a single `SSHSystemAsset`.

This means:

- Changes affect the entire scene  
- Undo/redo operates on one shared asset  
- Runtime and edit-mode share the same state  

Multiple simultaneous style states are not supported without multiple managers.

---

## Requests and Extensions

These limitations describe the current system design, not hard restrictions on what is possible.

If a project requires functionality outside these boundaries, support can be contacted with:

- A description of the use case  
- What behavior is needed  
- Why the current system is insufficient  

If enough demand exists for a feature or workflow, additional systems or extensions can be built to relax or remove specific limitations.
