# User Guide

This is the starting point for artists, designers, and technical artists working with **Summon Style: Hardline**.

Hardline has shaders in, but it is not a collection of shaders, it acts more as a **global visual system**.  
Shadows, sky visuals, environmental light and outlines in the scene are driven by a single shared style state.

Understanding that mental model is the key to using the system smoothly.

---

## The Hardline Style Model

Hardline treats the materials as color source of truth, and add the environment lighting and shadows additively, this lets you have exact control of base colors and levers to return to that true color in compliment with environment lighting.

Instead of each object having its own lighting calculation and and phsically based lighting, the scene has:

- One environment  
- One sky  
- One outline style  
- One lighting state  

All materials react to that shared state.

This allows you to:
- Change the entire look of a world instantly  
- Animate lighting and atmosphere without touching materials  
- Keep a consistent graphic style across all assets  

The system is controlled through a single component:

```  
SSHStyleManager
```

This component owns lifecycle and access to:
- The active style  
- The environment lighting  
- The skybox  
- The outline system  

---

## Style Control

Style control is about **how the world looks**.

This includes:
- Ambient light color and strength  
- Shadow tint and cutoff  
- Sky gradients and stars  
- Outline thickness, color, and noise  

All of this lives in the **global style state** owned by the ``` SSHStyleManager ```.

You do not light objects directly.  
You shape how the environment behaves, and the world responds.

Style control can be done:
- In the inspector (artists & designers)  
- Through the API (gameplay & runtime systems)  
- By loading or blending presets  

For detailed configuration and workflows, see:

→ [UserGuideStyleConfig](./UserGuide/UserGuideStyleConfig.md)

→ [UserGuideRemoveStyleFeatures](./UserGuide/UserGuideRemoveStyleFeatures.md)

---

## Content Authoring

Content authoring is about **how assets react to the style system**.

Hardline materials are not “lit” in the traditional Unity sense.  
They are unlit surfaces that respond to:
- Environment color  
- Shadow masks  
- Additional light volumes  

Each material decides:
- How much it listens to the environment  
- How much shadow affects it  
- How graphic or soft its shading is  

This is how you make:
- Characters stay readable at night  
- Fire and magic glow through darkness  
- UI meshes ignore world lighting  
- Props feel grounded in the scene  

Hardline provides:
- A complete core shader set  
- A shared visual language (base / accent colors, ramps, masks)  
- Hooks for extending the system with custom shaders  

For authoring details, see:

→ [UserGuideCoreShaders]((./UserGuide/UserGuideCoreShaders.md))

→ [UserGuideCustomShaders](./UserGuide/UserGuideCustomShaders.md)

---

## Preset Management

Presets are how you **store and reuse looks**.

A preset (``` SSHStyleAsset ```) is a snapshot of:
- Environment lighting  
- Skybox  
- Outline configuration  

They allow you to:
- Save a look  
- Apply it to any scene  
- Blend between multiple looks at runtime  
- Drive time-of-day, weather, or mood systems  

Presets do not contain animation.  
They are designed to be blended or interpolated by gameplay systems.

The demo scene’s day/night cycle is built entirely from preset blending.

For detailed workflows, see:

→ [UserGuidePresetManagement](./UserGuide/UserGuidePresetManagement.md)

---

## How Everything Fits Together

Hardline has these interaction layers:

| Layer | What It Controls |
|------|-----------------|
| **SSHStyleManager Component** | Core operations manager and lifetime hook (Need one in the scene) |
| **SSHStyleManager.API** | API access to the style system, if you want to change things during runtime you do it here. |
| **Style** | The world’s lighting, sky, and outlines, controllable through the Component Inspector or API. |
| **Materials** | Visual definision of unlit materials, and how much influence they receive from the style's environment. |
| **Presets** | Saved snapshots of complete styles, can be applied at runtime or through the inspector. |

Once you understand this separation, the system becomes simple:

- You texture/color the scene objects using **materials**,  
- You shape the world environment influence using **styles**,  
- You store looks using **presets**,  
- You manipulated the data in those presets and apply them to the **style manager**.

Everything else flows from that.
