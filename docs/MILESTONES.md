# Milestones

M1-M6 ship as **v0.1.0** (PROJECT.md "Milestones").

Process (harness ground rules): work one milestone at a time - M(n+1) does not
start until M(n) builds and is verified in-game. Each milestone gets its own
branch and PR titled `M<n>: <milestone name>`. Update this file when a
milestone is done.

Testing baseline (DESIGN.md #24): one fresh Valheim world, Meadows biome, day 1.
Fresh world every time physics change.

| # | Milestone | Outcome | Status |
|---|-----------|---------|--------|
| M1 | Plugin loads | BepInEx loads Mineheim, F5 toggles mode, log works | code complete, in-game checklist pending |
| M2a | Movement core | Walk, jump, gravity, ground detection, no clipping | not started |
| M2b | Movement polish | Sprint, air control, water, fall damage | not started |
| M3a | Mining core | Raycast, hardness, tool speed, vanilla drops | not started |
| M3b | Drop conversion | Valheim-Minecraft drop table, tier gating | not started |
| M4a | Block registry | One block type, place works, persists as ZDO | not started |
| M4b | Block set | 9 starter blocks, placement validation, drops | not started |
| M5 | Steve model | 3D Steve replaces player model, facing + animation | not started |
| M6 | HUD | Crosshair, hearts, 9-slot hotbar | not started |

---

## M1 - Plugin loads

Branch: `m1-plugin-loads`. Target: v0.1.0-alpha M1.

Deliverables:

- `src/MineheimPlugin.cs` - `BaseUnityPlugin` subclass, `Awake()`, Harmony init, config binding
- `src/Config.cs` - toggle key, log level
- `src/Player/MineheimPlayer.cs` - per-player Mineheim state
- `build.txt` - mod metadata with pinned Valheim build

Acceptance checklist (fresh world, Meadows, day 1):

- [x] `dotnet build` succeeds (0 warnings, 0 errors, output `bin/Release/net472/Mineheim.dll`)
- [x] BepInEx 5.4.23.5 loads the plugin on Valheim 1.0.16 build 25527674 (verified by launching the game and reading `BepInEx/LogOutput.log`)
- [x] Log shows `Mineheim v0.1.0 loaded`
- [x] No crash on plugin load (game reached `Chainloader startup complete`, `com.mineheim.plugin.cfg` generated with the F5 toggle key and log level)
- [ ] F5 flips Mineheim mode and the log prints the new state (needs a human in a world)
- [ ] F5 does nothing while moving (DESIGN.md #4, "standing still")
- [ ] F5 does nothing while in combat (DESIGN.md #4, 10 s Valheim combat window)
- [ ] No crash on world entry or world exit

Notes:

- Target framework is `net472`, not the `net6.0` output PROJECT.md's Build
  section shows: BepInEx 5.4 loads plugins into Unity Mono (CLR 4.0.30319.42000),
  which cannot resolve `System.Runtime 6.0.0.0` - a net6.0 plugin is found by
  the chainloader but its `Awake()` never runs (verified empirically on build
  25527674). Jotunn 2.30.2 targets .NETFramework 4.6.2; Mineheim follows that
  convention per ground rule 8. TODO(spec): align PROJECT.md's Build section.
- Toggle gating follows DESIGN.md #4. "In combat" mirrors Valheim's own window:
  `Player.CanSwitchPVP()` treats `Humanoid.m_lastCombatTimer > 10` as out of
  combat (verified in the assembly_valheim 1.0.16 IL), so Mineheim uses the same
  10-second window since the player last started an attack.
- Standing still is `Character.GetVelocity()` magnitude below 0.1 m/s.
- M1 ships no Harmony patches; the Harmony instance is initialized in
  `Awake()` and the first patch (the `Player.Update` movement postfix from
  PROTOCOL.md "Movement contract") lands in M2a.
- The four unchecked items need a human at the keyboard; record results here
  before the PR is merged.

## M2a - Movement core (not started)

Outcome: walk, jump, gravity, ground detection, no clipping. Acceptance
checklist is defined when the milestone starts.

Planned hooks (PROTOCOL.md "Movement contract", "Physics constants"):

- `Player.Update` Harmony postfix writes `m_body.velocity` only - never
  `transform.position`
- `MinecraftPhysics` constants scaled from Minecraft's 20 Hz ticks to
  Valheim's 50 Hz
- Ground detection via `Character.IsOnGround()`
