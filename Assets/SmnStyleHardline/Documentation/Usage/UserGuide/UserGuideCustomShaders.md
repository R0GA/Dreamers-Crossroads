# Custom Shaders

This document explains how to **create new Hardline-compatible shaders** by extending the existing Core Shader Graphs.

This is an advanced workflow intended for technical artists and graphics programmers.

> **Prerequisite**  
> This document requires knowledge of the Hardline Core shaders.  
> Read this first:  
> → [UserGuideCoreShaders](./UserGuideCoreShaders.md)

Hardline shaders are not normal Unity shaders.  
They must participate in the **global environment and shadow system** driven by `SSHStyleManager`.

The correct way to extend Hardline is to **start from an existing Core shader** and modify its surface logic — not to build from scratch.

---

## Why You Must Start From a Core Shader

All Hardline Core shaders already contain:

- The global environment light bindings  
- The global shadow bindings  
- Additional light volume sampling  
- The correct Shader Graph include files  
- The correct render pass configuration  
- All required global shader properties  

These are not optional.

If you build a Shader Graph from scratch, it will:
- Not receive environment lighting  
- Not receive shadows  
- Not participate in outlines  
- Not behave correctly inside the Hardline pipeline  

So the rule is simple:

**Always copy a Core shader and extend it.**

---

## Choosing a Base Shader

Pick the shader closest to what you want to build.

| Base Shader | When to Use |
|-------------|-------------|
| **GenericSimple** | Rawest possible base. Flat color, no textures. Best for building custom effects. |
| **GenericBase** | Two-color procedural Hardline shading. |
| **GenericTextured** | Painted or masked surfaces. |
| **GenericMapped** | Texture-driven ink and accent control. |
| **GenericMappedWind** | Vegetation, cloth, foliage. |
| **Procedural shaders** | Rocks, grass, inked terrain. |

For this guide we will use:

**GenericTextured**

We will extend it to support an **emissive mask** that cancels environment and shadow influence so parts of the surface glow and remain readable.

---

## Goal

We want:

- A normal Hardline textured material  
- Plus an emissive mask texture  
- That removes environment lighting and shadows where the mask is white  

This allows:
- Windows  
- Neon  
- Magical runes  
- Glowing UI meshes  
- Effects  

to appear bright while still living inside the Hardline world.

---

## Step 1 — Duplicate the Shader

Locate:

```
Assets/SmnStyleHardline/Core/Runtime/Graphics/Shaders/Object/Generic/GenericTextured
```

Duplicate it into a clean directory in your project, for example:

```
Assets/MyGame/Shaders/
```

Rename the new shader:

```
GenericTexturedEmissive
```

Open it in **Shader Graph**.

---

## Step 2 — Understand the Graph Structure

Hardline shaders are arranged in **layers**.

You will see blocks similar to:

```
Material Colors & Masks
↓
Procedural & Texture Shaping
↓
Sub Sample Environment Light
↓
Sub Sample Shadow Map
↓
Final Surface
```

We are going to modify **how the environment and shadows are applied** — not how colors are generated.

Find these two nodes:

- `Sub Sample Environment Light`
- `Sub Sample Shadow Map`

These are where the global Hardline lighting enters the material.

---

## Step 3 — Locate the Environment Influence Input

Select **Sub Sample Environment Light**.

You will see an input called:

```
Environment_Light_Influence
```

This is the per-material multiplier that controls how strongly the global environment affects this surface.

Normally it is connected directly to the material property:

```
Environment_Light_Influence (float)
```

We are going to **modulate this value using a texture mask**.

---

## Step 4 — Add the Emissive Mask

Create a new Shader Graph property:

```
Texture_Emissive
```

Type:

```
Texture2D
```

This texture will be a **mask**:
- White = emissive (ignore environment & shadows)  
- Black = normal Hardline lighting  

---

## Step 5 — Sample the Mask

Add a:

```
Sample Texture 2D
```

Connect:
- `Texture` → `Texture_Emissive`  
- `UV` → the same UVs used by the color texture  

This gives us a grayscale value representing where emission should exist.

---

## Step 6 — Invert the Mask

We want:
- White = cancel lighting  
- Black = allow lighting  

But the environment system works by **multiplying influence**.

So we need:

```
1 − mask
```

Add a:

```
One Minus
```

Connect:

```
Sampled Emissive → One Minus
```

This creates a **cancellation mask**:
- Emissive area → 0  
- Normal area → 1  

---

## Step 7 — Cancel Environment Light

Add a:

```
Multiply
```

Inputs:
- A → `Environment_Light_Influence`  
- B → Output of `One Minus`  

This produces:

```
Final_EnvInfluence = Environment_Light_Influence × (1 − EmissiveMask)
```

Now connect this result to:

```
Sub Sample Environment Light → Environment_Light_Influence
```

instead of the original float property.

The emissive areas now ignore environment light.

---

## Step 8 — Cancel Shadow Influence

Now do the same thing for shadows.

Find:

```
Sub Sample Shadow Map
```

Locate its input:

```
Environment_Multiplier
```

Add another:

```
Multiply
```

Inputs:
- A → the original shadow influence value  
- B → the same **One Minus** emissive mask  

Connect this to:

```
Sub Sample Shadow Map → Environment_Multiplier
```

Now emissive areas also ignore shadows.

---

## Final Result

Your shader now has:

- Normal Hardline shading everywhere  
- But where `Texture_Emissive` is white:
  - No environment darkening  
  - No shadow tint  
  - Pure base color remains  

This creates true **Hardline-style emission** that integrates perfectly with:
- Presets  
- Day/night  
- Weather  
- Style blending  
- Outline rendering  

without breaking the global system.

---

## Why This Works

We did not bypass Hardline.

We simply **modulated its influence**.

The global environment system still runs.  
The material simply chooses where to ignore it.

This is the correct pattern for extending Hardline.

---

## Key Rule

When adding new surface behavior:

**Always operate between the material and the Sub Sample nodes.**

Never remove:
- Environment sampling  
- Shadow sampling  
- Global parameters  

You only reshape how they apply.

That is how you build powerful custom looks that remain fully compatible with the Hardline pipeline.
