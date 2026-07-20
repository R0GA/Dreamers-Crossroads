# Core Shaders

This document explains the **Hardline Core Geometry Shaders**, the material layer that defines how meshes receive color, environment light, shadows, and stylized surface features inside the Hardline rendering system.

Hardline shaders do not behave like Unity’s Lit or Unlit shaders.

They do not run a traditional lighting model, but they **do** evaluate lighting and shadow data that is injected globally by the Hardline system.

Each material shader:
- Reads the global environment and shadow values written by `SSHStyleManager`
- Evaluates those values through Hardline’s lighting and shadow subgraphs
- Applies per-material controls that decide how strongly the surface reacts to the world

This is what allows every object to exist inside one shared lighting state while still retaining per-material artistic control.

---

# The Hardline Shading Pipeline

All Core shaders follow the same conceptual flow:

```
Material Colors & Masks
↓
Procedural & Texture Shaping
↓
Environment Light Subgraph
↓
Shadow Subgraph
↓
Final Stylized Surface

```


The important idea is:

**Materials do not create their own lights.**  
They evaluate a **shared world lighting model** that is authored and driven by the `SSHStyleManager`.

---

# Global Lighting Data

The Hardline system writes a set of global shader parameters when the style changes or if light data is updated.  
These are read by all Hardline shaders.

They represent the current state of:
- Environment light
- Shadow color
- Additional light volumes

They are not material properties.

---

## Global Shadow System

```
_SSH_Shadow_Color  
_SSH_Shadow_Intensity  
_SSH_Shadow_Clip  
_SSH_Shadow_Add_RangeMult  
_SSH_Shadow_Add_Count  
_SSH_Shadow_Add_Light00 … _SSH_Shadow_Add_Light07
```

These values define the global shadow layer.

The `_SSH_Shadow_Add_LightXX` entries are **distance-based occlusion volumes**:

- `xyz` → world position  
- `w` → radius  

They locally modify:
- Shadow strength  
- Environment light  

They do not emit brightness.

This is how lamps, torches, and interior spaces shape lighting without breaking the global style.

---

## Global Environment Light

```
_SSH_EnvLight_Color  
_SSH_EnvLight_Intensity  
_SSH_EnvLight_Add_Influence
```

These define the world’s ambient fill and overall mood.

Every Hardline material samples these values through the **Environment Light subgraph**.

---

# Per-Material Participation

Every Core shader includes:

```
Environment_Light_Influence  
Environment_Shadow_Influence
```

These values multiply the global environment and shadow layers **inside the shader**.

This allows materials to:
- Ignore lighting
- Ignore shadows
- Stay bright in darkness
- Fade into the environment

This is how fire, FX, UI meshes, and emissive props remain readable while still existing in the same world.

---

# Hardline Color System

Most Core shaders use the Hardline two-color model:

```
Color_Base  
Color_Accent
```

These are blended using a **continuous vertical gradient**.

---

## Accent Mask

```
AccentMask_Strength  
AccentMask_Add
```

The Accent Mask is a **Y-axis gradient** evaluated on the surface.

It blends `Color_Base → Color_Accent`.

- `AccentMask_Strength` controls how steep the blend is  
- `AccentMask_Add` slides the gradient up or down  

This creates a color horizon across the object, not discrete bands.

---

# Core Shader Families

Hardline Core shaders differ only in **how they generate base color, accent color, and masks**.

They all share the same environment and shadow system.

---

# Base & Mapped Shaders

These are the primary material types.

---

## GenericSimple

Flat color with full Hardline lighting.

```
Color_Base
```

Used for:
- FX
- UI meshes
- Simple props
- Emissive elements

---

## GenericBase

Pure procedural Hardline shading.

```
Color_Base  
Color_Accent  
AccentMask_Strength  
AccentMask_Add
```

Used for:
- Terrain
- Props
- Characters
- Stylized geometry

---

## GenericTextured

Painted color shaped by Hardline.

```
Texture_Color  
Color_Base  
Color_Accent  
Color_Line  
AccentMask_Strength  
AccentMask_Add  
Line_Sensitivity
```

Used for:
- Painted surfaces
- Patterned geometry
- UI-style art

---

## GenericEmit

Painted color textured with an emissive map to negate environmental control by Hardline.

```
Texture_Color
Texture_Emit
Color_Base
Color_Accent
AccentMask_Strength
AccentMask_Add
```

Used for:
- Neon signs
- Magical runes
- Glowing UI meshes

---

## GenericMapped

Map-driven accent and line placement.

```
Texture_Map  
Color_Base  
Color_Accent  
Color_Line  
AccentMask_Strength  
AccentMask_Add  
Line_Sensitivity
```

Used for:
- Inked surfaces
- Detailed props
- Stylized texture control

---

## GenericMappedWind

Mapped shading with motion and cutout.

```
Texture_Map  
Texture_Opacity  
Color_Base  
Color_Accent  
Color_Line  
AccentMask_Strength  
AccentMask_Add  
Line_Sensitivity  
Opacity_Clip  
Wind_Speed  
Wind_Scale  
Wind_ScaleB  
Wind_Strength
```

Used for:
- Grass
- Leaves
- Cloth
- FX meshes

---

# Procedural Shaders

These generate detail without textures.

---

## ProcGlassSpec

Stylized glass with hardline lighting.

```
Color_Specular
Color_Border
Color_Surface
Surface_Intensity
Border_Weight
Specular_Push
Specular_ViewSupress
Specular_Intensity
Specular_RimBias
Specular_BorderClip
```

---

## ProcGlassStreak

Stylized glass streaks with hardline lighting.

```
Color_Streaks
Color_Surface
Surface_Intensity
Border_Weight
Streaks_Intensity
Streaks_Scale
Streaks_NegateScale
Streaks_BaseWidth
Streaks_PreSoftness
Streaks_VariationAmount
Streaks_NoiseScale
Streaks_CameraOffsetScale
Streaks_CameraMoveScale
Streaks_Seed
```


---

## ProcBaseBandedWind

Procedural double-band foliage.

```
Color_Base  
Color_Accent  
Color_BandA  
Color_BandB  
AccentMask_Strength  
AccentMask_Add  
Band_Strength_A  
Band_Step_A  
Band_Strength_B  
Band_Step_B  
Wind_Speed  
Wind_Scale  
Wind_ScaleB  
Wind_Strength
```

Used for:
- Grass
- Stylized plants
- Organic striping

---

## ProcBaseLinedTopCover

Procedural ink + top layer.

```
Color_Base  
Color_Accent  
Color_Top  
Color_Line  
AccentMask_Strength  
AccentMask_Add  

TopMask_Strength  
TopMask_Add  
TopMask_ClipZ  

Proc_Line_Density_Hor1  
Proc_Line_Thickness_Hor1  
Proc_Line_Break_Scale_Hor1  
Proc_Line_Break_Threshold_Hor1  
Proc_Line_JitterStrength_Hor1  
Proc_Line_JitterScale_Hor1  

Proc_Line_Density_Hor2  
Proc_Line_Thickness_Hor2  
Proc_Line_Break_Scale_Hor2  
Proc_Line_Break_Threshold_Hor2  
Proc_Line_JitterStrength_Hor2  
Proc_Line_JitterScale_Hor2  

Proc_Line_Density_Ver  
Proc_Line_Thickness_Ver  
Proc_Line_Break_Scale_Ver  
Proc_Line_Break_Threshold_Ver  
Proc_Line_JitterStrength_Ver  
Proc_Line_JitterScale_Ver
```

Used for:
- Rocks
- Terrain
- Ink-style surfaces
- Graphic shading

---

# Water & FX Shaders

These are Hardline-integrated procedural effects.

---

## WaterSurface

Stylized water plane.

```
Color_Base  
Depth_Strength  
Depth_Blend  
Foam_Color  
Foam_Size  
Foam_Size_Variance  
Foam_Speed  
Outline_Size  
Outline_Color
```

Used for:
- Oceans
- Lakes
- Rivers
- Ponds

---

## Waterfall

Flowing vertical water.

```
Color_Base  
Depth_Strength  
Depth_Blend  
Wind_Speed  
Wind_Scale  
Wind_ScaleB  
Wind_Strength  
Fall_TimeScale  
Fall_PosFloorMult  
Fall_PosFloorAdd  
Fall_NoiseScale  
Fall_NoiseStepA  
Fall_NoiseStepB  
Fall_NoiseStep01  
Fall_NoiseStep02
```

Used for:
- Waterfalls
- Stylized falling water

---

## WaterSplashRing

Procedural splash rings.

```
Foam_RingColor  
Foam_RingSize  
Foam_RingRadius  
Foam_RingSpacing  
Foam_RingThickness  
Foam_RingSpeed  
Foam_RingFadeStart  
Foam_RingFadeEnd  
Foam_RingWarpStrength  
Foam_RingWarpFrequency  
Foam_RingWarpSpeed
```

Used for:
- Water impacts
- Ripples
- Splash effects

---

# Final Mental Model

Every Hardline Core Shader does the same thing:

It evaluates **material colors and masks**, then applies **global environment light and shadow** through Hardline’s subgraphs, modulated by per-material influence.

Everything else is just **how that input is authored**.

If you need to extend the core shaders, that is very doable, follow the steps in this document.

→ [UserGuideCustomShaders](./UserGuideCustomShaders.md)

