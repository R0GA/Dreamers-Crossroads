# Removing or Disabling Style Features

This document explains how to **disable or remove specific Hardline visual systems** when they are not needed for a project.

Hardline is built from several independent full-screen and material-driven systems:

- Outlines  
- Skybox  
- Environment lighting and shadows  

Some of these are controlled by **URP render features**, and others are controlled by **style values**.

They are disabled in different ways.

---

# Full-Screen Style Features

Hardline uses URP **Renderer Features** for its full-screen passes.

These features live on the Hardline render pipeline asset:

```Assets/SmnStyleHardline/Core/Runtime/Graphics/URPSettings/SSH_RPAsset```

Select this asset in the Project window to see the renderer configuration.

In the Inspector you will see a list of **Renderer Features**.

The important ones are:

- Screen Space Shadows  
- SSH Outline Color  
- SSH Outline Geo  
- SSH Skybox  

Each of these corresponds to a full-screen render pass.

---

## Screen Space Shadows

This feature is required for:

- Hardline shadow tinting  
- Environment shadow masking  
- Additional light shadow interaction  

It provides the depth-based shadow buffer used by Hardline materials.

Do not disable this unless you are removing **all** environment shadow behavior.

---

## SSH Outline Color

This is the color-based outline pass.

It generates outlines from:
- Color contrast  
- Screen-space edges  

If this feature is disabled:
- All color outlines disappear  
- The Color Outline section in the style has no effect  

---

## SSH Outline Geo

This is the geometry-based outline pass.

It generates outlines from:
- Surface normals  
- Depth discontinuities  

If this feature is disabled:
- All geometry outlines disappear  
- The Geo Outline section in the style has no effect  

---

## SSH Skybox

This is the procedural Hardline skybox.

It renders:
- Gradient bands  
- Dual-tone sky colors  
- Procedural starfield  

If this feature is disabled:
- The Hardline skybox will not render  
- The scene will fall back to Unity’s normal skybox or camera clear color  
- All Skybox style values become inactive  

---

# Disabling Outlines

To remove outlines completely:

1. Select  `SSH_RPAsset`


2. In the Renderer Features list, disable or remove:
- `SSH Outline Color`
- `SSH Outline Geo`

No changes to materials or style values are required.

The outline API and inspector will still exist, but they will no longer render anything.

---

# Disabling the Hardline Skybox

To remove the procedural skybox:

1. Select   `SSH_RPAsset`


2. Disable or remove:
- `SSH Skybox`

The scene will then use:
- The Camera background color, or  
- A standard Unity skybox  

The Hardline skybox style controls will no longer affect rendering.

---

# Environment Lighting and Shadows

Environment lighting and shadows are **not** a full-screen feature.

They are evaluated **inside every Hardline material**.

This means they cannot be disabled by removing a renderer feature.

Instead, they are controlled through style values.

---

## Disabling Environment Light

To remove ambient environment lighting:

In the `SSHStyleManager` inspector:

```Environment → Environment Light → Light Intensity```

Set it to: `0`


This removes all global ambient lighting from the scene.

Materials will render using only their base and accent colors.

---

## Disabling Environment Shadows

To remove global shadowing:

In the `SSHStyleManager` inspector:

```Environment → Shadows → Shadow Intensity```

Set it to: `0`


This disables the shadow layer entirely.

---

## Disabling All Environment Influence

To make the scene ignore the environment system:

Set:

```Environment → Environment Light → Light Intensity = 0```

```Environment → Shadows → Shadow Intensity = 0```


This leaves:
- Skybox (if enabled)
- Outlines (if enabled)

but removes all lighting and shadow modulation from materials.

---

# Material-Level Overrides

Even when environment lighting and shadows are enabled globally, individual materials can opt out.

Hardline shaders expose:

- `Environment_Light_Influence`  
- `Environment_Shadow_Influence`  

Setting these to zero on a material makes that object ignore:

- Environment lighting  
- Shadow tinting  

This is used for:
- Fire  
- UI meshes  
- Particles  
- Glowing objects  
- Effects  

---

# Summary

| Feature | How It Is Disabled |
|-------|-------------------|
| Color Outlines | Disable `SSH Outline Color` in `SSH_RPAsset` |
| Geometry Outlines | Disable `SSH Outline Geo` in `SSH_RPAsset` |
| Skybox | Disable `SSH Skybox` in `SSH_RPAsset` |
| Environment Light | Set `Environment Light Intensity = 0` |
| Environment Shadows | Set `Shadow Intensity = 0` |
| Per-object environment | Set material influence sliders to 0 |

Full-screen systems live in the **URP Renderer**.  
Lighting systems live in the **Style** and **Materials**.

They are intentionally separated so each part can be removed cleanly without breaking the rest of the pipeline.
