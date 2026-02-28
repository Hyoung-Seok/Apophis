# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Apophis** — 3D top-down action/strategy survival game. "Survive by exploring darkness and escape — light is precious."
- **Engine:** Unity 6 (6000.3.10f1)
- **Render Pipeline:** URP 17.3.0 with dual quality tiers (PC and Mobile)
- **Platforms:** PC and Mobile (Android)
- **Language:** Korean-primary development

## Key Packages

Cinemachine 3.1.6, Character Controller 1.4.2, New Input System 1.18.0, AI Navigation 2.0.11, Animation Rigging 1.4.1, Addressables 2.9.1, Timeline 1.8.11, Unity MCP (AI editor bridge)

## Folder Convention

```
Assets/
  1_Scenes/       — Scene files
  2_Scripts/       — Game C# scripts
  3_Prefabs/       — Prefab assets
  4_Model/         — 3D models
  5_Animations/    — Animation clips/controllers
  6_Image/         — Textures, sprites, UI images
  7_Data/          — ScriptableObjects, configs, data assets
  Settings/        — URP render pipeline assets and volume profiles
  Thirdparty/      — Third-party assets (Gridbox Prototype Materials)
  Plugins/Editor/  — Editor plugins (JetBrains RiderFlow)
```

## Render Pipeline

- **PC:** `Settings/PC_RPAsset` — full quality, RenderScale 1.0, depth/opaque textures ON
- **Mobile:** `Settings/Mobile_RPAsset` — optimized, RenderScale 0.8, depth/opaque textures OFF
- **Post-processing:** ACES tonemapping, bloom, vignette via `SampleSceneProfile`
- Color space: Linear

## Editor Settings

- **Enter Play Mode Options:** Enabled with no domain/scene reload (fast iteration)
- **Serialization:** Force Text
- **Physics:** Standard gravity, 50Hz fixed timestep

## Build & Test

- Test framework (`com.unity.test-framework` 1.6.0) is installed; tests go in standard `Tests/` assembly folders
- No CI/CD pipeline configured yet
- No custom build scripts; use Unity's standard build pipeline
- Build scenes: `Assets/1_Scenes/EmptyScene.unity` (index 0)
