# Mineheim

Play Valheim as a Minecraft character.

Mineheim layers Minecraft movement, mining, block placing, inventory, crafting,
and combat on top of Valheim. Valheim's world, biomes, bosses, and progression
still run underneath.

This is a single BepInEx plugin. No bridge, no second process, no Fabric mod.
Minecraft's rules are reimplemented in C# and applied to Valheim's systems.

Inspired by [SkyCraft](https://github.com/chasmlol/SkyCraft/releases/tag/v0.1.0).

---

## Status

Early development. v0.1.0 not released. Current target: M1.

## Requirements

- Valheim (current stable Steam build)
- BepInEx 5.4.x (x64)
- Harmony (bundled with BepInEx)
- Jotunn (Valheim modding library)
- .NET 6 SDK for building
- Windows, Linux (Proton), or macOS (untested)

## Download

Not yet. Watch the Releases page.

Planned v0.1.0 assets:

- `Mineheim-0.1.0.zip` (BepInEx plugin + Jotunn dependency notes)
- `Mineheim-0.1.0-symbols.zip`

## Install

1. Install Valheim via Steam.
2. Install BepInEx 5.4.x into the Valheim folder.
3. Install Jotunn into `BepInEx/plugins`.
4. Drop `Mineheim.dll` into `BepInEx/plugins`.
5. Launch Valheim. Press **F5** to toggle Mineheim mode.

## Features (v0.1.0)

- Minecraft movement: walk, sprint, jump, sneak, air control, fall damage
- Minecraft mining: tool tiers, block hardness, mining speed, correct drops
- Minecraft block placing: player-placed blocks are real Valheim ZDOs
- Valheim resources convert to Minecraft items when mined in Mineheim mode
- Minecraft hearts HUD and 9-slot hotbar
- 3D Steve as the player character (Unity makes this free)
- Single-player only

## Not in v0.1.0

- Hunger, XP, enchanting
- Minecraft crafting grid (Valheim's crafting UI stays)
- Minecraft mobs (Valheim's enemies stay)
- Multiplayer
- Boss rebalance

See `DESIGN.md` for reasoning.

## Architecture

Single BepInEx plugin. C#. Harmony patches. Jotunn for content registration.

Valheim owns the world, terrain, persistence, and networking. Mineheim
supplies:
- Input → velocity curve (fed to Valheim's character controller)
- Minecraft block registry (as Valheim pieces and ZDOs)
- Tool tier gating
- HUD rendering
- 3D Steve as the player model

Valheim's physics and ZDO persistence are authoritative. Mineheim never
writes `Character.transform.position` directly — only `Character.m_velocity`
or the equivalent rigidbody force. See `PROTOCOL.md`.

## Why this is easier than SkyCraft

SkyCraft is a C++ SKSE plugin talking to a Java Fabric mod over shared memory.
Two runtimes, two languages, two build systems.

Mineheim is one C# plugin. No bridge. No second process. Minecraft's rules
are reimplemented directly in C#, the same way MineTerra would have done it
for Terraria — except Valheim is 3D, so Steve works for free.

## Layout

```
Mineheim/
├─ README.md
├─ PROJECT.md
├─ PROTOCOL.md
├─ DESIGN.md
├─ LICENSE
├─ CONTRIBUTING.md
├─ .github/workflows/
│  └─ build.yml
├─ src/
│  ├─ MineheimPlugin.cs
│  ├─ Config.cs
│  ├─ Player/
│  │  ├─ MineheimPlayer.cs
│  │  ├─ MinecraftMovement.cs
│  │  └─ MinecraftPhysics.cs
│  ├─ Mining/
│  │  ├─ ToolTiers.cs
│  │  ├─ BlockHardness.cs
│  │  └─ ValheimToMinecraftDrop.cs
│  ├─ Building/
│  │  ├─ BlockRegistry.cs
│  │  ├─ BlockPlacement.cs
│  │  └─ Aim.cs
│  ├─ Blocks/
│  │  ├─ MinecraftStoneBlock.cs
│  │  ├─ MinecraftDirtBlock.cs
│  │  └─ ...
│  ├─ Items/
│  │  ├─ MinecraftStoneItem.cs
│  │  └─ ...
│  ├─ Steve/
│  │  ├─ SteveModel.cs
│  │  ├─ SteveAnimator.cs
│  │  └─ SteveRenderer.cs
│  └─ HUD/
│     ├─ Crosshair.cs
│     ├─ Hearts.cs
│     └─ Hotbar.cs
├─ assets/
│  ├─ steve.fbx
│  └─ textures/
└─ docs/
   ├─ ARCHITECTURE.md
   ├─ MILESTONES.md
   ├─ KNOWN_ISSUES.md
   └─ BUILD.md
```

## Milestones

| # | Milestone | Outcome |
|---|-----------|---------|
| M1 | Plugin loads | BepInEx loads Mineheim, F5 toggles mode, log works |
| M2a | Movement core | Walk, jump, gravity, ground detection, no clipping |
| M2b | Movement polish | Sprint, air control, water, fall damage |
| M3a | Mining core | Raycast, hardness, tool speed, vanilla drops |
| M3b | Drop conversion | Valheim→Minecraft drop table, tier gating |
| M4a | Block registry | One block type, place works, persists as ZDO |
| M4b | Block set | 9 starter blocks, placement validation, drops |
| M5 | Steve model | 3D Steve replaces player model, facing + animation |
| M6 | HUD | Crosshair, hearts, 9-slot hotbar |

M1–M6 ship as **v0.1.0**.

## Build

```bash
git clone https://github.com/<you>/Mineheim
cd Mineheim
dotnet build
# Output: bin/Release/net6.0/Mineheim.dll
```

Copy the DLL to `BepInEx/plugins` and launch Valheim.

## Known issues

- Valheim's terrain is a heightmap, not a voxel grid. Placed blocks sit on
  top of the terrain, they don't replace it. This is by design.
- Bosses take Minecraft weapon damage but aren't rebalanced. Some fights
  will be long.
- Valheim updates will break patches. Pin to a known version.

## Legal

Mineheim bundles no Iron Gate or Mojang assets. Draw your own 16x16 block
textures in Minecraft's style. Build your own Steve model or use a CC0 one.
You must own both games.

Not affiliated with or endorsed by Iron Gate Studio or Mojang.

## License

MIT.