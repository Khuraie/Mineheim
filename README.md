# Mineheim

Play Valheim as a Minecraft character.

Mineheim layers Minecraft movement, mining, block placing, inventory feel,
and HUD on top of Valheim. Valheim's world, physics, ZDO persistence,
inventory, crafting, and progression stay intact underneath - Mineheim is
**Valheim with Minecraft flavor**, not a Minecraft port (see `DESIGN.md`).

Inspired by [SkyCraft](https://github.com/chasmlol/SkyCraft/releases/tag/v0.1.0).

## Status

Early development. Current milestone: **M1 (plugin loads)** - see
`docs/MILESTONES.md`. v0.1.0 ships after M6.

## Requirements

- Valheim (current stable Steam build, pinned in `build.txt`)
- BepInEx 5.4.x (x64)
- Jotunn
- .NET 6 SDK (building only)

## Install

1. Install BepInEx 5.4.x into the Valheim folder.
2. Install Jotunn into `BepInEx/plugins`.
3. Drop `Mineheim.dll` into `BepInEx/plugins`.
4. Launch Valheim. Press **F5** to toggle Mineheim mode.

## Controls (M1)

- **F5** - toggle Mineheim mode. DESIGN.md #4: only while standing still and
  out of combat.

## Build

```bash
dotnet build -c Release
# Output: bin/Release/net472/Mineheim.dll
```

See `docs/BUILD.md` for details.

## Documents

- `PROJECT.md` - goals, requirements, layout, milestones
- `PROTOCOL.md` - internal contracts and physics rules (non-negotiable)
- `DESIGN.md` - scope decisions (non-negotiable)
- `docs/MILESTONES.md` - milestone status and acceptance checklists

## Legal

Mineheim bundles no Iron Gate or Mojang assets. You must own both games.

Not affiliated with or endorsed by Iron Gate Studio or Mojang.

## License

MIT.
