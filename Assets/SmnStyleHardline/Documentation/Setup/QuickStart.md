# Quick Start

This guide takes you from a clean install to actively authoring, controlling, and applying the Hardline style system.

---

## 1. Setup

This is covered in other documentation, but repeated for continuity.

### Activate the Hardline Renderer

After importing the package, the Hardline Wizard opens automatically.

If it does not, open it manually:

```
Window → Summon Style → Hardline Wizard
```

If the wizard shows:

> Hardline Render Pipeline Asset is available but not active

Click:

```
Apply Hardline Render Pipeline Asset
```

This switches the project to the URP configuration required for:

- Depth Checks  
- Outline rendering feature  
- Procedural skybox features

Hardline will not function fully unless this is active.

---

## 2. Demo

This section shows what Hardline actually does.

### Open the Demo Scene

Open:

```
Assets/SmnStyleHardline/Demo/Scenes/Demo_Island01
```

Press Play.

You should see a remote deserted island environment with some ruins and a camp, rendered entirely in the Hardline style.

### Move Time of Day

At the top of the screen is a Time of Day slider.

Drag it.

You will see:

- Shadow color and density change  
- Ambient fill shift  
- Skybox gradients and starmap update

This is all being dynamically applied via demo scripts.  
This is Hardline’s global style system being driven live via its interaction API.

---

## 3. Developers

This section explains how the demo is driven.

In the scene hierarchy locate:

```
SSHDemoDayNightManager
```

This script drives:

- Environment light  
- Shadow tint  
- Skybox gradients  
- Referencing preset data.

using the Hardline API `SSHStyleManager.API` at runtime.

It demonstrates how gameplay systems can control the visual style of the entire world without touching materials or lights directly.

---

## 4. Artists

This section is about authoring looks.

Select:

```
SSHStyleManager
```

This is the global style controller for the entire scene.

It has four major panels:

- Core — Presets and system state  
- Environment — Ambient light, shadow properties, etc. 
- Skybox — Gradient sky and stars  
- Outlines — Edge thickness, depth fade, noise  

Every change updates the entire scene instantly.

---

### Loading a Preset

In Core, click:

```
Load Preset
```

Navigate to:

```
Assets/SmnStyleHardline/Assets/SampleAssets/StylePresets
```

Select:

```
SSHStyle_NoOutlines.asset
```

The scene will update immediately.

---

### Creating a New Look

Go to Environment.

Under Environment Light set:

- Light Color → `#349854`  
- Light Intensity → `0.65`

The entire scene shifts into a hazy green atmosphere.

---

### Saving the Style

Return to Core and click:

```
Save Preset
```

Save it as:

```
SSHStyle_NoOutlinesGreener.asset
```

You now have a reusable style asset that can be loaded into any scene.

---

## 5. Applying Hardline to a Scene

This is how you use Hardline in your own levels.

1. Open or create a scene  
2. Create an empty GameObject  
3. Add the SSHStyleManager component  

This single object:

- Holds the active style  
- Drives all shaders  
- Controls environment lighting  
- Manages skybox and outlines  

Only one should exist per scene.

---

## 6. Creating Art Assets

This is how geometry and materials interact with the system.

---

### Assigning Hardline Shaders

Create or import a mesh.

Create a material and assign a Hardline shader, for example:

```
SSH/Geo/GenericBase
```

All Hardline geometry shaders follow the same core structure.

---

### The Two-Color System

Hardline materials are built around two colors:

- Color_Base — The main surface color  
- Color_Accent — A secondary band or ramp color  

How they blend is controlled by the Accent Mask.

---

### Accent Mask Controls

In the material inspector:

- AccentMask_Strength  
  Controls how sharp the transition is  
  - Low = smooth gradient  
  - High = graphic band  

- AccentMask_Add  
  Slides the accent band up or down across the surface  

These two values let you control the shared shadowed/unlit gradient visual language the system allows.

without using textures.

---

### Environment Interaction

Every Hardline material chooses how much it reacts to the world:

- Environment_Shadow_Influence  
- Environment_Light_Influence  

Lower these for:

- Fire  
- UI meshes  
- Glowing props  
- Magical effects  

so they stay bright and readable regardless of lighting.

