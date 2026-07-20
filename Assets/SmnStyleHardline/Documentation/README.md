# Summon Style: Hardline

> An illustrative rendering system for expressive, gradient-blocked worlds. Create hand-drawn, cel-shaded scenes in real 3D space—without the limits of Unity’s physically-based lighting.

Hardline replaces physically-based shading and material hacks with a visual language built from controlled color gradients, shaped light, and inked form. Instead of smooth lighting falloff or PBR roughness, surfaces are defined by intentional color ramps that behave like painted fills and comic shading. 

Hardline's shaders are unlit, offering performant visuals, while still allowing dynamic shadows and stylized ambient lighting to be applied to them to bring scenes to life.

---

## Overview

The `Summon Style Hardline` package provides a complete system for authoring, controlling, and rendering stylized, illustrative, cell-shaded, materials in Unity. We can break the project down into a few key modules:
- **Core**: The core system files you need to run the system, this includes shaders, URP integration assets and the core runtime system and component.
- **Assets**: Sample assets set up for the style system, including materials, meshes, and textures. This will also be where any official asset packs are placed on install.
- **Documentation**: All the local documentation files for the system. Documentation is also available online [here](https://summonboxstudio.com/docs/smnstylehardline/README). 
- **Demo**: A demo scene showcasing the system in action, includes demo only component scripts to help illustrate usage with an interactive day/night cycle.
- **Wizard**: An editor wizard to help set up the system in your project, remove unused modules, and link to online documentation.

The only required module to run the system is the Core module. All other modules can safely be removed if not used in your project. The Assets module is only required if you want to use the demo scene.

You interact with the system through a single hook component, this `SSHStyleManager` component manages all system state, style application, and rendering control. You can change the style properties from this component either straight in the inspector, or through the API at runtime. This hook lets us easily manage style state across scenes, and lets other systems interact with the system. We will cover this in more detail in the `Usage` section of the documentation.

---

## Style Features

- **Dynamic Unlit Shaders**: A complete set of unlit shaders built for the style system, with support for a range of stylized usage scenarios (Gradient Base-Accent Colored, Mapped/Textured, Wind Vertex Animation, Water, etc)
- **Outlines**: Configurable color and geometry-based outlines with depth response and stylized noise, hidable if wanted.
- **Environment Lighting**: Stylized ambient lighting with color control, dynamic tinted shadow sampling, and dynamic pointlight based shadow/environment absorption.
- **Skybox**: Procedural skybox with gradient bands, dual-tone colors, and a configurable starfield.
- **URP Integration**: Full support for the Universal Render Pipeline, including a Render Features to handle outline rendering and skybox.
- **Centralized Style Management**: A single component to manage all style state, making it easy to control and switch styles at runtime or in the editor.

---

## Getting Started

If this is your first time using Summon Style: Hardline, follow this path:

1. Visit `Requirements.md` to check if your project is compatible with the system.  
   
   → [Requirements](./Setup/Requirements.md)

2. See `Installation.md` to see how to automatically set up the URP renderer using the Wizard.  
   
   → [Installation](./Setup/Installation.md)

3. See `QuickStart.md` to get up and running with the demo and authoring simple content.
   
   → [Quick Start](./Setup/QuickStart.md)

4. Once you are ready to create your own styles and scenes with the system, see `UserGuide.md`.
   
   → [User Guide](./Usage/UserGuide.md)


This will take you from a clean Unity project to a working system you are ready to create with.

---

## Core Concepts

Summon Style: Hardline is built so users only need to interact with a few core elements to control the system. These are:

| Layer | Purpose |
|------|--------|
| SSHStyleManager Component | The core lifecycle manager and system interaction layer. |
| SSHSystemAsset Asset | The system-wide configuration and current style configuration container. |
| SSHStyleAsset Asset | Style preset asset, used to store and easily apply style configurations. |
| SSHStyleManager.API Class | The exposed API for interacting with the system programmatically. |
| Style Shaders | System-ready shader set used by materials to render in the system's style. |

The system internals are intentionally abstracted away to keep user interaction simple and focused, and to avoid conflicting shader property manipulation. 

##### Developers:
If you are a developer developing gameplay systems that need to interact with the style system, see the `API Reference` section below.

##### Artists & Designers:
If you are an artist or designer looking to create and author styles, or assets for those styles, see the `User Guide` section below.

---

## API Reference

The API is separated into several sub-APIs, each responsible for a specific part of the style system:

- **API.Core**  
  Controls global system state and runtime behavior. This includes assigning the active system asset, controlling environment update frequency, and forcing immediate lighting or environment refreshes.

- **API.Preset**  
  Applies complete `SSHStyleAsset` presets to the current system state. This is used when you want to apply a full style configuration to the current system.

- **API.Environment**  
  Controls all scene lighting that affects the style system, including ambient environment light, shadow color and strength, and synchronization of additional Unity lights into the shader system.

- **API.Skybox**  
  Controls the procedural skybox, including base and accent colors, gradient banding, and the procedural starmap (stars, density, scale, and tilt).

- **API.Outline**  
  Controls all outline rendering, including color-based outlines and geometry-based edge detection. This API lets you tune outline color, thickness, depth response, and stylized noise.

→ [API](./Usage/API.md)


---

## User Guide

For artists and designers looking to create and author styles or content:

For an overview of how to create and use styles in your project, or how to use the system's shaders effectively for materials in your scenes, see the `User Guide` section.

→ [User Guide](./Usage/UserGuide.md)

---

## Compatibility & Performance

Before shipping, review the constraints of the system:

- Performance Characteristics  
  → [Compatibility/Performance.md](./Compatibility/Performance.md)

- Known Limitations  
  → [Compatibility/Limitations.md](./Compatibility/Limitations.md)

---

## Supporting Assets & Examples

This package includes example assets and demonstrations to help you understand how the system is intended to be used:

- → [SupportingAssets/DemoDoc.md](./SupportingAssets/DemoDoc.md)  
- → [SupportingAssets/SampleAssetDoc.md](./SupportingAssets/SampleAssetDoc.md)

---

## FAQ

Common questions and clarifications:

→ [FAQ.md](./FAQ.md)

---

## Support

If you need help, encounter issues, or require clarification:

→ [SUPPORT.md](./SUPPORT.md)

---

## Versioning & Changes

Full change history:

→ [CHANGELOG.md](./CHANGELOG.md)

---

## License

Usage terms and legal information:

→ [LICENCE.md](./LICENCE.md)
