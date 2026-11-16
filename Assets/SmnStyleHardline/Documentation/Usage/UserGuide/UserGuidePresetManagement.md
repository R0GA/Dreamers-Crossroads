# Preset Management

This document explains how **Hardline style presets** are created, stored, and used by both artists and gameplay systems.

A preset is a single asset type:

``` SSHStyleAsset ```

It represents a **complete snapshot of the visual world**:
- Environment lighting  
- Skybox  
- Outlines  

Presets are how looks are authored, saved, shared, and driven at runtime.

They are the bridge between **art direction** and **gameplay control**.

---

# Conceptual Flow

The intended workflow is:

1. **An artist or designer builds a look** using the `SSHStyleManager` inspector  
2. That look is **saved into an SSHStyleAsset**  
3. Gameplay systems **load or blend those presets** through the API  

Presets do not animate.  
They are static snapshots designed to be blended, interpolated, or switched by runtime systems.

---

# What a Preset Contains

An ``` SSHStyleAsset ``` stores:

- All Environment values  
- All Skybox values  
- All Outline values  

It does not store:
- Animation  
- Curves  
- Time-of-day logic  
- Light objects  
- Scene data  

It is purely **style state**.

When applied, it overwrites the active style inside the `SSHStyleManager`.

---

# Designer Workflow (Inspector)

This is how artists and designers work with presets.

---

## Building a Look

In the scene:

1. Select the `SSHStyleManager`
2. Use the following panels to tune the look:
   - Environment  
   - Skybox  
   - Outlines  

Every change updates the entire scene immediately.

This is where you art-direct:
- Time of day  
- Mood  
- Weather  
- Palette  
- Graphic sharpness  

---

## Saving a Preset

When the look is correct:

In the `SSHStyleManager` Inspector:

```Core → Save Preset```


You will be prompted to create a new ``` SSHStyleAsset ```.

This captures:
- Current Environment  
- Current Skybox  
- Current Outlines  

as a reusable style snapshot.

Presets are just assets.  
They can be versioned, duplicated, renamed, and stored like any other Unity asset.

---

## Loading a Preset

In the `SSHStyleManager` Inspector:

```Core → Load Preset```


Select any ``` SSHStyleAsset ```.

The scene will immediately update to that look.

This is identical to calling the Preset API at runtime.

---

# Developer Workflow (API)

Presets are applied through:

``` SSHStyleManager.API.Preset ```

---

## Applying a Preset at Runtime

The core function is:

``` API.Preset.Apply(SSHStyleAsset) ```

This replaces the entire style state of the scene.

It updates:
- Environment  
- Skybox  
- Outlines  
- All shaders  

in one operation.

This is how:
- Time of day  
- Weather  
- Biomes  
- Interior / exterior  
- Mood systems  

are implemented.

---

## Accessing a Preset in Code

An ``` SSHStyleAsset ``` is a normal Unity ScriptableObject.

You reference it like any other asset.

Example pattern:

``` public SSHStyleAsset dayStyle; ```
``` public SSHStyleAsset nightStyle; ```

These fields can be assigned in the inspector.

---

## Applying It Through the API

At runtime:

``` var api = SSHStyleManager.API; ```
``` if (api == null) return; ```

``` api.Preset.Apply(dayStyle); ```

This pushes the preset into the active style system.

Everything in the scene updates instantly.

---

# Blending Presets

Presets are not meant to be switched only.

They are designed to be:
- Read  
- Interpolated  
- Reapplied  

The demo’s day/night system:
- Reads multiple ``` SSHStyleAsset ``` values  
- Interpolates between them  
- Writes the blended result into the style system through the API  

This allows:
- Smooth sunrise and sunset  
- Weather transitions  
- Biome fades  

Presets are the data.  
Your scripts provide the animation.

---

# Multiple Presets in a Project

A typical project will have presets like:

- Day  
- Night  
- Sunset  
- Foggy  
- Interior  
- Cave  
- BossFight  

These are all just ``` SSHStyleAsset ``` files.

They can be:
- Loaded manually in the editor  
- Driven by timeline  
- Blended by gameplay systems  
- Assigned per scene  

There is no limit to how many presets you create.

---

# Summary

Presets are the **style currency** of Hardline.

- Artists create looks in the inspector  
- Those looks are saved as ``` SSHStyleAsset ```  
- Developers apply and blend them using ``` API.Preset ``` or seperate API calls.