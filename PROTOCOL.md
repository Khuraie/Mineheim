# Mineheim Internal Protocol

Mineheim is a single BepInEx plugin. There is no bridge, no external process,
no second game. All communication happens in-process between Mineheim
subsystems and Valheim's APIs.

This file documents the internal event contracts so contributors know who
owns what.

---

## Subsystem boundaries

| Subsystem | Owns | Reads |
|-----------|------|-------|
| Movement | Player velocity, gravity, jump state | Input, ground flag |
| Mining | Block damage progress, tool tier checks | Aim raycast |
| Building | Block placement, hotbar selection | Aim raycast, inventory |
| Drops | Valheim→Minecraft item mapping | Block break events |
| Steve | Player model, animation, facing | Player state |
| HUD | Rendering only | All subsystems |

No subsystem may directly modify another's state. Use the events below.

---

## Movement contract

Valheim's physics and character controller are authoritative. Mineheim never
writes `Character.transform.position`. It only writes velocity.

Tick order:

```
1. Harmony patch fires on Player.Update or Character.Update
2. ComputeVelocity reads input + player state, writes desired velocity
3. Valheim's physics runs, resolves collision, commits position
4. PostUpdate reads resolved state, updates Steve + HUD
```

```csharp
[HarmonyPatch(typeof(Player), nameof(Player.Update))]
static void Postfix(Player __instance)
{
    if (!MineheimPlugin.IsMinecraftMode(__instance)) return;
    var vel = MinecraftMovement.ComputeVelocity(__instance);
    __instance.m_body.velocity = vel;
}
```

**Rule:** never write `transform.position`. Only `m_body.velocity`.

---

## Physics constants

Valheim uses meters, same as Minecraft. No unit conversion needed.

```csharp
public static class MinecraftPhysics
{
    public const float Gravity          = 0.08f;  // m/tick²  (1 tick = 1/20 s)
    public const float TerminalVelocity = 3.92f;  // m/tick
    public const float JumpVelocity     = 0.42f;  // m/tick
    public const float WalkSpeed        = 4.317f / 20f;
    public const float SprintSpeed      = 5.612f / 20f;
}
```

Valheim ticks at 50 Hz (Unity fixed update). Minecraft ticks at 20 Hz.
Scale accordingly — either tick-lock Mineheim to 20 Hz or convert constants.

---

## Aim contract

Valheim is a third-person 3D game. Aim is camera-driven, not mouse-cursor.

```csharp
public static class Aim
{
    public static Vector3 GetAim(Player player)
    {
        var cam = GameCamera.instance.transform;
        return cam.forward;
    }

    public static bool Raycast(Player player, float reach, out RaycastHit hit)
    {
        var origin = player.GetEyePoint();
        var dir = GetAim(player);
        return Physics.Raycast(origin, dir, out hit, reach);
    }
}
```

This is different from MineTerra. Valheim has a 3D camera, so aim is the
camera forward vector. Reach is 4.5 meters (Minecraft default).

Every Minecraft system reads the camera forward vector. No mouse cursor.

---

## Mining events

### OnMineTick

Fires every tick the player holds the mine button.

```csharp
public class MineTickEvent
{
    public Player Player;
    public Vector3 HitPoint;
    public int ToolTier;
    public float Progress; // 0..1
}
```

Break condition: `Progress >= 1f && CanBreak(blockType, toolTier)`.

### OnBlockBreak

Fires when a block is destroyed by player mining.

```csharp
public class BlockBreakEvent
{
    public Player Player;
    public Vector3 Position;
    public string ValheimPrefabName;
    public int? MinecraftDrop; // null = use Valheim drop
}
```

If `MinecraftDrop` is non-null, suppress the Valheim drop and spawn the
Minecraft item instead.

### OnPlaceBlock

Fires on place input.

```csharp
public class PlaceBlockEvent
{
    public Player Player;
    public Vector3 Position;
    public Vector3 Normal;
    public int ItemType;
    public int HotbarSlot; // 0..8
}
```

---

## Tool tiers

Mapped to Minecraft pickaxe tiers.

| Minecraft tier | Required for |
|---------------|-------------|
| Hand          | dirt, grass, sand, snow |
| Wood          | stone, cobblestone, coal ore |
| Stone         | iron ore, lapis ore |
| Iron          | diamond ore, gold ore, redstone ore |
| Diamond       | obsidian |
| Netherite     | ancient debris |

Below-tier on stone-class blocks: block breaks, no drop.
Below-tier on ore-class blocks: block doesn't break.

---

## Block mapping

Valheim prefabs and resources map to Minecraft items.

| Valheim resource | Minecraft drop |
|------------------|---------------|
| `Rock` / `stone` | `MinecraftStoneItem` |
| `Dirt` (terrain) | `MinecraftDirtItem` |
| `Wood` | `MinecraftOakLogItem` |
| `FineWood` | `MinecraftOakPlanksItem` |
| `CoreWood` | `MinecraftDarkOakLogItem` |
| `CopperOre` | `MinecraftCopperOreItem` |
| `TinOre` | `MinecraftIronOreItem` |
| `IronScrap` | `MinecraftIronOreItem` |
| `SilverOre` | `MinecraftGoldOreItem` |
| `Obsidian` | `MinecraftObsidianItem` |
| `Crystal` | `MinecraftDiamondOreItem` |
| `Flint` | `MinecraftFlintItem` |
| `LeatherScraps` | `MinecraftLeatherItem` |

Unmapped prefabs fall back to Valheim's native drops. Destructibles with
custom behavior (boss altars, chests, portals) always fall back — they
carry data and must not be converted.

---

## Placement rules

A Minecraft block may be placed at `Position` only if:

1. In world bounds.
2. No existing ZDO at that position.
3. Adjacent to terrain or another placed block.
4. Does not intersect player capsule.
5. Does not intersect any active character or creature.

Valheim's ZDO system handles persistence automatically once spawned.

---

## Steve contract

Steve is a Unity GameObject with a skinned mesh or 6-box rig. He replaces
the default player model.

```csharp
public class SteveController : MonoBehaviour
{
    public Transform head;
    public Transform body;
    public Transform leftArm, rightArm;
    public Transform leftLeg, rightLeg;

    public void SetFacing(float yaw) { /* rotate root */ }
    public void SetAnimation(SteveState state) { /* swap anim */ }
}
```

States: Idle, Walk, Sprint, Jump, Fall, Mine, Attack, Sneak.

Facing follows the camera yaw. No left/right flip — this is 3D, Steve turns
freely.

---

## Rules

- No subsystem talks to another directly. Use events.
- Events are synchronous. Do not block.
- Never write `transform.position`. Only `m_body.velocity`.
- Patch via Harmony. Do not modify Valheim DLLs.
- Config changes take effect on next world load unless marked live-reloadable.