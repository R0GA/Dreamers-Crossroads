# Installation

This document covers installing **Summon Style: Hardline** into a compatible Unity project and setting up the required rendering environment using the included setup wizard.

Before proceeding, ensure your project satisfies all technical requirements listed in [Requirements](./Requirements.md).

---

## 1. Install the Package

Summon Style: Hardline is distributed as a Unity package.

### Asset Store Installation

1. Open the Unity Editor.  
2. Open **Window → Package Manager**.  
3. Select **My Assets**.  
4. Locate **Summon Style: Hardline**.  
5. Click **Download**, then **Import**.  
6. Import all files.

After import completes, the Hardline modules and documentation will appear in:

```
Assets/SummonStyleHardline/
```

This includes:
- Core system files  
- Shaders  
- Demo content  
- Wizard  
- Documentation  

---

## 2. Automatic Setup Wizard

After the package finishes importing, the **Hardline Setup Wizard** will automatically open.

This wizard handles all required URP and renderer configuration for you.

### What the Wizard Does

The wizard will:

- Detect your current URP configuration.  
- Create or install the **Hardline URP Renderer Data**.
- Assign the correct renderer to your active URP Asset.

You will be asked to switch to the Hardline Wizard if you are not using the provided URP Asset.

This step is required for the system to function.

---

## 3. Environment Configuration

When prompted by the wizard:

- Accept the renderer switch.  
- Allow it to update project's URP Asset (Note, this will NOT delete your original asset, just swap it out).

Once complete, your project will be configured with:

- A URP Renderer that supports Hardline outlines and skybox.  
- A system-wide style configuration asset.  
- All required render passes and buffers.  

No manual URP configuration is required unless you have a custom rendering setup.

---

## 4. Installation Complete

At this point:

- The rendering pipeline is configured.
- The Hardline runtime system is active (Just waiting for a `SSHStyleManager` component to exist).
- The demo and style assets are ready to use.  

You can now continue with:

→ [QuickStart](./QuickStart.md) – to load the demo and see the system in action.


→ [UserGuide](../Usage/UserGuide.md) – to start creating and authoring your own styles and art content.

---

If you encounter issues during installation or setup:

→ [SUPPORT](../SUPPORT.md)