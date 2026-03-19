# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Apophis** — a 3D top-view action/tactical-strategy game. Theme: survive by exploring darkness and escape; light is precious above all. Targets PC and Mobile platforms.

## Engine & Stack

- **Unity 6000.3.10f1** (Unity 6 LTS)
- **Render Pipeline**: URP 17.3.0 — dual renderer setup with `PC_Renderer`/`PC_RPAsset` and `Mobile_Renderer`/`Mobile_RPAsset` under `Assets/Settings/`
- **Input**: New Input System (`com.unity.inputsystem` 1.18.0)
- **AI Navigation**: `com.unity.ai.navigation` 2.0.10
- **C#**: Language version 9.0, .NET Framework 4.7.1
- **IDE**: Rider and Visual Studio integrations installed
- **MCP**: `com.coplaydev.unity-mcp` (beta) for Claude AI ↔ Unity Editor integration

## Key Packages

| Package | Purpose |
|---|---|
| `com.unity.render-pipelines.universal` | URP rendering |
| `com.unity.inputsystem` | New Input System |
| `com.unity.ai.navigation` | NavMesh pathfinding |
| `com.unity.timeline` | Cinematic sequences |
| `com.unity.test-framework` | Unit/integration tests |
| `com.unity.visualscripting` | Visual scripting |

## Project Structure

- `Assets/Scenes/` — Game scenes (currently `SampleScene.unity`)
- `Assets/Settings/` — URP settings, volume profiles, per-platform render assets
- `Packages/manifest.json` — Package dependencies
- `ProjectSettings/` — Unity project configuration

## Testing

Unity Test Framework 1.6.0 is installed. Run tests via Unity Editor (Window > General > Test Runner) or use the `run_tests` MCP tool. No test assemblies exist yet — when creating tests, place them under `Assets/Tests/` with appropriate `.asmdef` files (EditMode and PlayMode).

## Unity MCP Integration

This project has the Unity MCP server connected. Use MCP tools to:
- Query and modify scene hierarchy, GameObjects, components
- Create/edit C# scripts and check compilation via `read_console`
- Manage assets, materials, prefabs, animations
- Control play mode via `manage_editor`

After creating or modifying scripts, always check `read_console` for compilation errors before proceeding.

## Platform Considerations

When writing rendering or performance-sensitive code, account for the dual PC/Mobile renderer configuration. Quality settings and render pipeline assets differ per platform.

## Language

The project README and commit messages are in Korean (한국어). Follow the same convention unless instructed otherwise.
