# Performance

This document explains how **Summon Style: Hardline** uses GPU and CPU resources, what actually costs time, and how to control performance in real projects.

Hardline replaces physically based lighting with a stylized, deterministic shading model.  
This shifts performance cost away from light evaluation and toward controlled, predictable shader math.

---

## Performance Model

Hardline sits between **URP Unlit** and **URP Lit**.

- More expensive than Unlit  
- Significantly cheaper than Lit  

This is because Hardline materials perform:
- Screen-space shadow sampling  
- Environment color blending  
- Optional distance-based light masking  

But do **not** perform:
- Light loops  
- BRDF evaluation  
- Specular or reflection probe sampling  
- Per-pixel light accumulation  

The cost is stable and predictable.

Hardline shaders fully support **URP SRP Batching**, so draw calls batch the same way as standard URP shaders.

---

## Where GPU Time Is Spent

Hardline has three primary GPU cost centers.

---

### 1. Per-Material Shading (Main Cost)

Every Hardline material evaluates:

- Color ramps  
- Screen-space directional shadow  
- Environment light blending  
- Optional additional-light masks  

This happens per visible pixel.

This is:
- More expensive than URP Unlit  
- Much cheaper than URP Lit  

Because there are no real lights, no reflection probes, and no BRDFs.

Performance scales with:
- Number of visible objects  
- Screen resolution  
- Material complexity  

SRP batching ensures this cost is amortized efficiently across large numbers of meshes.

---

### 2. Outline Pass (Screen-Space)

Outlines are rendered using a full-screen post-process pass based on depth and normals.

This cost scales with:
- Screen resolution  
- Outline thickness  
- Noise and edge complexity  

Outlines are the **largest global performance lever** in Hardline.

Disabling outlines provides the biggest single performance improvement.

---

### 3. Skybox Pass (Constant Cost)

The Hardline skybox is rendered using a single full-screen pass.

It uses:
- Deterministic band math  
- Deterministic procedural stars  

Star density, scale, and band settings do **not** change shader cost.  
They only change constants in math.

Skybox cost is:

` Constant per frame `

---

## Lights Are Not Lights

Unity lights do not behave as real light sources in Hardline.

They are used only as:

- Shadow occlusion masks  
- Environment absorption volumes  

Each light contributes only:
- Position  
- Radius  

The shader evaluates a light by checking:

` distance² < radius² `

This produces a mask that modulates:
- Shadow strength  
- Environment influence  

Lights do not:
- Emit illumination  
- Cast shadows  
- Run lighting equations  

---

## Additional Light Limit

Hardline supports:

` StyleControllerEnvironment.MaxAdditionalLights = 8 `

This is a hard limit.

These lights are selected based on distance to **Camera.main** and uploaded to shaders as simple position/radius data.

Performance impact:
- CPU: selecting closest lights  
- GPU: up to 8 distance checks per pixel  

This is extremely cheap compared to real lighting.

The limit exists to keep shader cost bounded and predictable.

---

## What Scales and What Does Not

### Scales with:
- Screen resolution  
- Number of visible objects  
- Outline enabled  
- Outline thickness and noise  
- Shadow clip sharpness  

### Barely scales with:
- Number of lights (hard-capped at 8)  
- Skybox complexity  
- Preset blending  
- API calls  

Hardline’s cost is dominated by pixels, not lights.

---

## Material-Level Control

Every Hardline material exposes:

- `Environment_Light_Influence`  
- `Environment_Shadow_Influence`  

These allow you to reduce or remove global lighting on:

- FX  
- UI meshes  
- Emissive objects  
- Characters  
- Particles  

This is a powerful performance and readability tool.

Objects that do not need lighting should opt out.

---

## CPU Cost

CPU work in Hardline is limited to:

- Selecting the closest lights  
- Uploading shader globals  
- Preset blending (if used)  

There is:
- No per-renderer light assignment  
- No per-object lighting lists  
- No culling-based lighting rebuilds  

This keeps CPU overhead low and stable.

---

## Mobile and Low-End Hardware

Hardline is compatible with mobile platforms and low-end GPUs, and has been tested on some newer mobile hardware but it requires:

- Depth textures  
- Screen-space shadow sampling  

Some older mobile GPUs either:
- Do not support depth textures correctly, or  
- Have very limited depth bandwidth  

Even if the URP Render Asset allows depth textures, the hardware may not be able to provide them at usable performance.

On low-power mobile devices:
- Reduce resolution  
- Disable or soften outlines  
- Use lower shadow clip values  
- Reduce visible geometry  

Depth usage is the primary limiting factor on low-end hardware.

---

## Performance Positioning

Hardline trades real lighting for stylized, controlled shading.

Compared to standard URP:

| Pipeline | Relative Cost |
|--------|---------------|
| URP Unlit | Cheapest |
| Hardline | Medium |
| URP Lit | Most expensive |

Hardline provides dynamic directional shadows and environment lighting without paying the cost of real point-lights.

---

## Summary

Hardline performance is driven by:

- Screen resolution  
- Visible pixel count  
- Outline complexity  

Not by:
- Number of lights  
- Light types  
- Reflection probes  
- BRDFs  

This makes the system predictable, scalable, and suitable for large stylized scenes.
