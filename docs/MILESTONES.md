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
| M2a | Movement core | Walk, jump, gravity, ground detection, no clipping | code complete (PR #2), in-game checklist pending |
| M2b | Movement polish | Sprint, air control, water, fall damage | code complete (PR #3), in-game checklist pending |
| M3a | Mining core | Raycast, hardness, tool speed, vanilla drops | code complete (PR #4), in-game checklist pending |
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

## M2a - Movement core

Branch: `m2a-movement-core` (PR #2). Code complete.

- `src/Player/MinecraftPhysics.cs` - PROTOCOL.md constants, scaled from 20 Hz tick units to per-second rigidbody units
- `src/Player/MinecraftMovement.cs` - `Player.Update` postfix writes `m_body.velocity` only (Harmony field ref; the field is private in assembly_valheim 1.0.16). Never `transform.position`
- Ground detection via `Character.IsOnGround()`; walk 4.317 m/s, jump 8.4 m/s, gravity 32 m/s^2 clamped at 78.4 m/s terminal

Acceptance checklist (fresh world):

- [ ] Walk at Minecraft speed; diagonals capped to walk speed
- [ ] Jump reaches about 1.2 m (Minecraft 1.25 blocks)
- [ ] Gravity feels Minecraft-fast; terminal velocity on long falls
- [ ] No clipping through terrain or pieces

## M2b - Movement polish

Branch: `m2b-movement-polish` (PR #3). Code complete.

- Sprint: Valheim's "Run" action at 5.612 m/s (action name verified in assembly_valheim 1.0.16)
- Air control: weak steering toward wish speed; momentum kept without input
- Water: `Character.IsSwimming()` gate - slower move, jump rises, gentle capped sink; entering water cancels fall damage
- Fall damage: Minecraft's formula (fallDistance - 3) replaces Valheim's percentage formula; suppressed via a `Character.Damage` prefix on `m_hitType == 3` fall hits and re-applied through Valheim's own pipeline. Lands on Valheim HP per DESIGN.md #7

Acceptance checklist (fresh world):

- [ ] Sprint is visibly faster than walk
- [ ] Air steering is weak but present; falling keeps momentum
- [ ] Water: rise with jump, sink slowly otherwise, slower horizontal speed
- [ ] Fall 5 m: no damage; fall 10 m: 7 damage; land in water: no damage

## M3a - Mining core

Branch: `m3a-mining-core` (PR #4). Code complete.

- `src/Building/Aim.cs` - PROTOCOL.md Aim contract: camera-forward raycast from `Character.GetEyePoint()`, reach 4.5 m
- `src/Mining/BlockHardness.cs` - hardness in hand-speed ticks (Minecraft hardness x 30, the destroy constant)
- `src/Mining/ToolTiers.cs` - tool speed by equipped tier (hand 1 ... diamond 8); tier gating arrives in M3b
- `src/Mining/MinecraftMining.cs` - hold Valheim's "Attack" to mine; `Progress += toolSpeed / hardness` per Minecraft tick, break at 1 through Valheim's Destructible pipeline (vanilla drops); fires `OnMineTick` / `OnBlockBreak` per PROTOCOL.md with `MinecraftDrop = null`

Acceptance checklist (fresh world):

- [ ] Hold attack on a rock within 4.5 m: progress builds and it breaks (~2 s by hand, faster with tools)
- [ ] Breaking drops the usual Valheim loot (vanilla drops, no conversion yet)
- [ ] Trees and ore take measurably longer than loose rock (hardness)
- [ ] Looking away or releasing attack resets progress
