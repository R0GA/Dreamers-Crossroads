# Requirements

This document defines the minimum technical requirements needed for Summon Style: Hardline to function correctly inside a Unity project.

---

## Unity Versions

Summon Style: Hardline requires a modern Unity editor with current Scriptable Render Pipeline support.

| Unity Version | Release Date | Support Status |
|--------------|-------------|----------------|
| 6000.3.6f1 (LTS) | 29 Jan 2026 | Supported |
| 6000.3.5f2 (LTS) | 26 Jan 2026 | Supported |
| 6000.3.5f1 (LTS) | 21 Jan 2026 | Supported |
| 6000.3.4f1 (LTS) | 14 Jan 2026 | Supported |
| 6000.3.3f1 (LTS) | 08 Jan 2026 | Supported |
| 6000.3.2f1 (LTS) | 16 Dec 2025 | Supported |
| 6000.3.1f1 (LTS) | 09 Dec 2025 | Supported |
| 6000.3.0f1 (LTS) | 03 Dec 2025 | Supported|
| 6000.2.15f1 | 03 Dec 2025 | Supported |
| 6000.2.14f1 | 26 Nov 2025 | Supported |
| 6000.2.13f1 | 19 Nov 2025 | Supported |
| 6000.2.12f1 | 12 Nov 2025 | Supported |
| 6000.2.11f1 | 05 Nov 2025 | Supported |
| 6000.2.10f1 | 29 Oct 2025 | Supported |
| 6000.2.9f1 | 22 Oct 2025 | Supported |
| 6000.2.8f1 | 15 Oct 2025 | Core Supported (*) |
| 6000.2.7f2 | 08 Oct 2025 | Core Supported (*) |
| 6000.2.6f2 | 03 Oct 2025 | Core Supported (*) |
| 6000.1.17f1 | 03 Oct 2025 | Core Supported (*) |
| 6000.0.66f2 (LTS) | 26 Jan 2026 | Core Supported (*) |
| 6000.0.66f1 (LTS) | 21 Jan 2026 | Untested |
| 6000.0.65f1 (LTS) | 07 Jan 2026 | Core Supported (*) |
| 6000.0.64f1 (LTS) | 10 Dec 2025 | Core Supported (*) |
| 6000.0.63f1 (LTS) | 28 Nov 2025 | Untested |
| 6000.0.62f1 (LTS) | 05 Nov 2025 | Untested |
| 6000.0.61f1 (LTS) | 30 Oct 2025 | Core Supported (*) |
| 6000.0.60f1 (LTS) | 16 Oct 2025 | Core Supported (*) |
| 6000.0.59f2 (LTS) | 08 Oct 2025 | Untested |
| 6000.0.58f2 (LTS) | 03 Oct 2025 | Core Supported (*) | 

> `"*"` Versions with this symbol in the status column have Demo UI icon visibility issues for UIToolKit. This does NOT effect the core style system or sample assets, only effects demo UI. 

All `Supported` Unity 6 versions below **6000.2.6f2** are excluded due to known [security vulnerabilities](https://unity.com/security/sept-2025-01).

All `LTS` Unity 6 versions below **6000.0.58f2** are excluded due to known [security vulnerabilities](https://unity.com/security/sept-2025-01).

Due to above mentioned security vulnerabilities and drastic changed between the remaining version's URP Package before Unity 6, we recommend using this system with versions of Unity 6 that Unity has tagged `Supported`and versions above `6000.2.10f1` for a clean experience. That being said, with some tweaks the system can be used in any version above `6000.0.58f2` safely.

---

## Render Pipeline (URP)

Hardline only supports the Universal Render Pipeline.

**Supported Render Pipelines:**

| Pipeline | Support |
|--------|--------|
| Universal Render Pipeline (URP) | Required |
| Built-in Render Pipeline | Not supported |
| HDRP | Not supported |

Hardline relies on URP render features, render passes, and shader hooks that do not exist in the Built-in pipeline and are not shared verbatim with the HDRP.

**Supported URP Versions**

| URP Package | Status |
|-------------|--------|
| URP 14+ | Required |
| Below URP 14 | Not supported |

Hardline uses:
- Scriptable Render Features  
- Depth and opaque texture injection  
- Custom outline and skybox passes  

These are only reliable in modern URP versions.

---

## Color Space

| Setting | Requirement |
|--------|-------------|
| Color Space | Linear |

Gamma color space is not supported.  
Hardline’s gradient ramps, ambient light blending, and shadow tinting are authored and evaluated in linear space.

---

## Required URP Settings

The active URP Renderer must have:

- Depth Texture enabled  
- Opaque Texture enabled  

These are required for:
- Outline depth testing  
- Shadow color sampling  
- Environment and skybox lighting

---

## Shader Compatibility

It is highly recommended to use the provided Hardline shaders or expand based off of them for materials used with this system.

 It is technically possible to use Unity’s standard shaders (Lit, URP Unlit, Shader Graph PBR, etc.) but they may produce artifacts and will interfere with outline rendering. Standard shaders will also not get the same environment lighting and shadowing effects as the Hardline shaders. 

---

## Rendering Architecture Assumptions

Hardline assumes:

- A URP camera rendering the scene
- A URP Renderer with the Hardline Render Features installed
- A single active style system (via `SSHStyleManager`) driving all materials

Do not worry about assigning URP Renderers manually unless you have a specific need. It's safe to use the Wizard to set this up the system automatically in the `Installation.md` document.

Camera stacking and multi-renderer setups will currently omit the style system or render in unpredictable ways.
Note, UI is purposefully not changed by the hardline so that designers can decorate UI however they like.

---

## Platform Support

Hardline supports any platform supported by URP that provides:

- Depth textures
- Scriptable Render Features  

WebGL is not supported.

---

## Scripting & Runtime

Hardline requires:

- C# scripting backend  
- Scriptable Render Pipeline assemblies  
- Standard Unity runtime (no stripped render builds)

---

## Validation Checklist

Your project must meet all of the following before installation:

- [ ] Using Listed Unity version   
- [ ] Universal Render Pipeline active  
- [ ] Listed URP Version installed  
- [ ] Linear color space enabled  
- [ ] Depth Texture enabled  
- [ ] Opaque Texture enabled  

If any item is missing, Hardline is not guaranteed to render correctly.

---

If all requirements are met, continue to:

→ [Installation](./Installation.md)
