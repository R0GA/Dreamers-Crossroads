# FAQ

This document answers common questions about **Summon Style: Hardline** and clarifies how the system is intended to be used.

---

## Is Hardline a lighting system?

No.

Hardline replaces Unity’s physically based lighting with a **stylized environment and shadow system**.

Objects do not receive light from lamps, point lights, or spot lights.  
They receive color and shadow from the **global environment state** driven by `SSHStyleManager`.

Unity lights are used only to locally modify how the environment and shadow layers apply.

---

## Why don’t my lights brighten objects?

Because Hardline does not use lights as illumination sources.

Lights only act as:
- Shadow occlusion volumes  
- Environment absorption zones  

They negate environmental influence and break up the global lighting, but they do not emit brightness.

This keeps the visual style stable and predictable.

---

## Can I use baked lighting, lightmaps, or probes?

No.

Hardline does not use:
- Lightmaps  
- Light probes  
- Reflection probes  

All lighting is evaluated from:
- The global environment  
- Screen-space shadows  
- Additional light masks  

Materials that need to ignore lighting (UI meshes, emissive objects, decals, effects, etc.) can opt out using their environment and shadow influence controls.

---

## Can different parts of the scene have different lighting?

Not at the same time.

There is:
- One environment  
- One skybox  
- One outline state  

Regional variation must be achieved by:
- Blending style presets  
- Driving style values over time or distance  
- Controlling material influence values  

Multiple simultaneous lighting environments are not supported.

---

## Can I use multiple cameras?

Hardline is designed around a single main camera.

The environment system selects lights relative to `Camera.main`.  
Secondary cameras may render without correct environment occlusion or light influence.

Split-screen, minimaps, and picture-in-picture are not supported.

---

## Does Hardline support HDRP or Built-in?

No.

Hardline only supports:
- Universal Render Pipeline (URP)  

It relies on URP render passes, depth textures, and shader hooks that do not exist in other pipelines.

---

## Can I mix Hardline with standard URP shaders?

You can, but they will not behave the same.

Standard shaders:
- Do not receive Hardline environment lighting  
- Do not receive Hardline shadows  
- May interfere with outlines  

For best results, geometry that should appear in the Hardline style should use Hardline shaders.

---

## Why are there only 8 lights?

Because lights in Hardline are not lights.

They are just **distance masks** uploaded to shaders.

The system is hard-limited to:

! StyleControllerEnvironment.MaxAdditionalLights = 8 !

This keeps shader cost stable and predictable.

---

## Is Hardline suitable for realistic games?

No.

Hardline does not simulate:
- Real light falloff  
- Specular or metallic response  
- Energy conservation  

It is designed for:
- Stylized  
- Illustrated  
- Graphic  
- Cel-shaded  

visual styles.

---

## Are the sample assets production ready?

Yes, as of 1.1.0, the sample assets are designed to be usable in production projects.

They are:
- Visual references
- Shader usage examples
- Demo building blocks
- Optimized for real-time use
- Include LODs for all static meshes

They are not just demo content, but they are also not a full asset pack.
---

## Can I animate or blend styles at runtime?

Yes.

Style presets (`SSHStyleAsset`) are snapshots.  
They are designed to be:
- Blended  
- Interpolated  
- Driven by gameplay systems  

The demo’s day/night system is a reference implementation of this.

---

## Can I extend or customize Hardline?

Yes.

You can:
- Write scripts that drive the API  
- Create new style presets  
- Modify materials and shaders  
- Extend shader graphs based on Hardline’s includes  

You may not redistribute modified versions of the core system.

---

## What if I need something Hardline does not support?

The current limitations describe the shipped system, not the ceiling of what could be offered if there is demand.

If you need something outside the documented constraints, contact support with:
- Your use case  
- The behavior you need  
- Why the current system is insufficient  

If enough demand exists, new features or extensions can be built to relax or remove specific limitations.
