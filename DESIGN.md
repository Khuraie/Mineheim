# Mineheim Design Decisions

Decisions that define scope. Change these only after v0.1.0 ships.

## Scope

**This is "Valheim with Minecraft flavor," not "Minecraft in Valheim."**

Mineheim is a layer. Valheim's world, physics, persistence, inventory,
crafting, and progression stay intact. Minecraft supplies movement, block
appearance, placement rules, tool tiers, HUD, and the player model.

Anything that would require rewriting a Valheim subsystem is out of scope.

---

## Architecture

**1. Single BepInEx plugin, not a bridge.**
SkyCraft runs a hidden Minecraft process next to Skyrim and pipes data
between them. That's because Skyrim is a closed C++ engine and Minecraft is
Java — you can't merge them.

Valheim is Unity/C#, and Minecraft's rules are small enough to reimplement
in C#. No second process, no shared memory, no Fabric mod. One plugin.

**2. Reimplement Minecraft's rules, don't port Minecraft.**
We don't need real Minecraft running. We need Minecraft's *feel*: the
movement curve, the mining rules, the placement rules, the HUD. Those are
constants and logic, not a game engine.

**3. Valheim is authoritative.**
Valheim's physics, ZDO persistence, and networking are correct and tested.
Mineheim feeds them input. It does not replace them.

---

## Before M1

**4. Mode switching.** F5 toggles only when standing still and not in combat.

**5. Inventory.** Use Valheim's inventory. It already has a hotbar and a grid.
Re-skin the hotbar to 9 slots and Minecraft-style icons. Don't rebuild it.

**6. Win condition.** Mineheim mode is a sandbox layer. You can still beat
Valheim normally. Bosses take damage from Minecraft weapons but aren't
rebalanced in v0.1.0.

**7. HP scale.** Use Valheim HP. Do not convert to Minecraft's 20. Valheim's
HP values are tuned for its enemies, and converting breaks the game. Draw
hearts as a *display* of Valheim HP percentage.

**8. Toggle scope.** World-level for v0.1.0. Single-player only.

---

## Before M3

**9. Hardmode resources.** Leave them Valheim. Black metal, silver, and
mistlands-tier resources drop their vanilla items. Only map the early-game
resources.

**10. Smelting.** Use Valheim's smelter. Don't reimplement Minecraft's
fuel-and-timer system.

---

## Before M4

**11. Terrain vs. blocks.** Valheim's terrain is a heightmap. Placed blocks
sit *on top* of the terrain as discrete objects, they don't replace it.
This is different from Minecraft's voxel grid. Accept it — it's how Valheim
building already works.

**12. Persistence.** Every placed block is a Valheim ZDO. Valheim's save
system handles it automatically. No custom save format.

**13. Block registry.** Blocks are Valheim prefabs registered via Jotunn.
Textures are custom 16x16-style but rendered on 3D cubes, not flat sprites.

---

## Before M5

**14. Steve model.** Build Steve as 6 boxes (head, body, 2 arms, 2 legs) in
Unity. No external FBX needed. ~200 lines of C#. This gives you full control
over animation and is CC0 by construction.

**15. Animation.** Eight states: Idle, Walk, Sprint, Jump, Fall, Mine, Attack,
Sneak. Each is a small set of keyframed rotations on the limb transforms.
Minecraft's animations are simple — don't overthink them.

**16. Facing.** Steve rotates to face the camera yaw. Full 360°, not left/right
flip. This is 3D — Steve turns freely.

**17. Armor.** Hide Valheim armor rendering while Steve is active. Otherwise
you'd need to model every armor piece. v0.2.0 could add 3D armor, but not
v0.1.0.

---

## Before M6

**18. Crosshair.** Valheim already has a crosshair. Replace with a Minecraft
crosshair sprite.

**19. Hearts.** Draw 10 hearts in Minecraft style. Fill based on
`player.GetHealthPercentage()`.

**20. Hotbar.** Re-skin Valheim's 8-slot hotbar to Minecraft's 9. Hide slot 9
in the UI or repurpose it.

---

## Meta

**21. MVP.** Movement, mining, placing, Steve, HUD. Nothing else.

**22. Audience.** Valheim players who want Minecraft feel. Lean into Valheim
systems. Don't apologize for keeping Valheim's crafting, inventory, and
progression.

**23. Cool moment.** Mining copper in the Black Forest while a Minecraft
crosshair sits in the middle of the screen and Steve swings a pickaxe.
Design around it.

**24. Testing.** One fresh Valheim world, Meadows biome, day 1. Checklist per
milestone. Fresh world every time physics change.

**25. Versioning.**
- **v0.1.0** — movement, mining, placing, Steve, HUD, single-player.
- **v0.2.0** — hunger, XP, enchanting, crafting grid, multiplayer.
- **v1.0.0** — Minecraft mobs, boss rebalance, full block set, armor.

---

## Anti-goals

If a feature requires any of these, it's out of scope:

- Rewriting Valheim's inventory UI
- Rewriting Valheim's crafting system
- Rewriting Valheim's networking
- Replacing Valheim's physics
- Reimplementing Valheim's mob AI
- Adding a second HP scale
- Shipping copyrighted textures or models

## North star

> If it makes Valheim feel more like Minecraft without replacing Valheim,
> ship it. If it replaces Valheim, cut it.