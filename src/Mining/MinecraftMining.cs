using HarmonyLib;
using UnityEngine;

namespace Mineheim
{
    /// <summary>PROTOCOL.md "Mining events / OnMineTick". Synchronous; do not block.</summary>
    public class MineTickEvent
    {
        public Player Player;
        public Vector3 HitPoint;
        public int ToolTier;
        public float Progress; // 0..1
    }

    /// <summary>PROTOCOL.md "Mining events / OnBlockBreak". Null MinecraftDrop = use Valheim's drop.</summary>
    public class BlockBreakEvent
    {
        public Player Player;
        public Vector3 Position;
        public string ValheimPrefabName;
        public int? MinecraftDrop; // null = use Valheim drop, MinecraftItemId.None = no drop
    }

    /// <summary>
    /// Event hub (PROTOCOL.md "Rules": subsystems communicate via events only, events are
    /// synchronous and must not block). Mining raises; the Drops subsystem subscribes.
    /// </summary>
    public static class MineheimEvents
    {
        public static event System.Action<MineTickEvent> OnMineTick;
        public static event System.Action<BlockBreakEvent> OnBlockBreak;

        internal static void RaiseMineTick(MineTickEvent e) => OnMineTick?.Invoke(e);

        internal static void RaiseBlockBreak(BlockBreakEvent e) => OnBlockBreak?.Invoke(e);

        public static event System.Action<PlaceBlockEvent> OnPlaceBlock;

        internal static void RaisePlaceBlock(PlaceBlockEvent e) => OnPlaceBlock?.Invoke(e);
    }

    /// <summary>
    /// Mining core (PROTOCOL.md "Mining events"): hold the mine button (Valheim's "Attack"
    /// action) while looking at a Destructible within Aim.Reach. Progress grows at
    /// toolSpeed / hardness per Minecraft tick; the block breaks at Progress >= 1 when
    /// CanBreak(blockType, toolTier) passes.
    ///
    /// M3b drop rules (PROTOCOL.md "Tool tiers"): drops route through
    /// ValheimToMinecraftDrop; below-tier stone-class breaks with no drop; below-tier
    /// ore-class never breaks; unmapped destructibles keep their vanilla drops.
    /// TODO(spec): Valheim melee swings also damage destructibles (Valheim systems stay,
    /// DESIGN.md). Decide whether to suppress that in Mineheim mode so hardness timing
    /// is pure.
    /// </summary>
    /// <summary>
    /// Anything M3 mining can break: Destructible props, MineRock/MineRock5 deposits,
    /// and TreeLog trunks. Rocks and trees are NOT Destructibles (verified against
    /// assembly_valheim 1.0.16: MineRock, MineRock5 and TreeLog derive straight from
    /// MonoBehaviour), so a mine target limited to Destructible misses most of the world.
    /// </summary>
    internal sealed class MineTarget
    {
        private readonly Component _component;
        private readonly Destructible _destructible;
        private readonly string _dropField; // rock/tree DropTable field, null for Destructible

        public string Name => _component.name;

        private MineTarget(Component component, Destructible destructible, string dropField)
        {
            _component = component;
            _destructible = destructible;
            _dropField = dropField;
        }

        public static MineTarget FromHit(RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return null;
            }
            var destructible = hit.collider.GetComponentInParent<Destructible>();
            if (destructible != null)
            {
                return new MineTarget(destructible, destructible, null);
            }
            Component rock = hit.collider.GetComponentInParent<MineRock>() as Component
                ?? hit.collider.GetComponentInParent<MineRock5>() as Component
                ?? hit.collider.GetComponentInParent<TreeLog>() as Component
                ?? hit.collider.GetComponentInParent<TreeBase>() as Component;
            if (rock == null)
            {
                return null;
            }
            string typeName = rock.GetType().Name;
            string dropField = typeName == "TreeLog" || typeName == "TreeBase" ? "m_dropWhenDestroyed" : "m_dropItems";
            return new MineTarget(rock, null, dropField);
        }

        public bool SameAs(MineTarget other)
        {
            return other != null && other._component == _component;
        }

        public float Health =>
            Traverse.Create(_component).Field("m_health").GetValue<float>();

        public void Damage(HitData hit)
        {
            if (_destructible != null)
            {
                _destructible.Damage(hit);
                return;
            }
            // MineRock/MineRock5/TreeLog all expose public Damage(HitData).
            _component.SendMessage("Damage", hit, SendMessageOptions.DontRequireReceiver);
        }

        public void SuppressDrop()
        {
            if (_destructible != null)
            {
                MinecraftDrops.SuppressVanillaDrop(_destructible);
                return;
            }
            // Rocks and trees spawn their Destroy loot from a DropTable field; swap in an
            // empty table so nothing spawns. The object is destroyed right after, so the
            // swap is never restored (restoring before a deferred destroy RPC lands would
            // re-enable the vanilla drop).
            Traverse.Create(_component).Field(_dropField).SetValue(new DropTable());
        }
    }

    public static class MinecraftMining
    {
        private const float TicksPerSecond = 20f; // Minecraft tick rate (PROTOCOL.md)
        private const string MineButton = "Attack";

        private static MineTarget _target;
        private static float _progress;

        public static void Tick(Player player, float dt)
        {
            if (!ZInput.GetButton(MineButton) || !Aim.Raycast(player, Aim.Reach, out var hit))
            {
                Reset();
                return;
            }

            var target = MineTarget.FromHit(hit);
            if (target == null)
            {
                Reset();
                return;
            }

            if (!target.SameAs(_target))
            {
                _target = target;
                _progress = 0f;
            }

            string prefabName = target.Name;
            int tier = ToolTiers.CurrentTier(player);
            BlockClass blockClass = ValheimToMinecraftDrop.Classify(prefabName);
            int requiredTier = ValheimToMinecraftDrop.RequiredTier(prefabName);

            if (!ToolTiers.CanBreak(blockClass, requiredTier, tier))
            {
                // Ore-class below tier: the block does not break (PROTOCOL.md). Progress
                // tops out at 1 and waits for a better tool.
                _progress = Mathf.Min(1f, _progress + SpeedPerSecond(tier, prefabName) * dt);
                RaiseTick(player, hit, tier);
                return;
            }

            _progress += SpeedPerSecond(tier, prefabName) * dt;
            RaiseTick(player, hit, tier);

            if (_progress >= 1f)
            {
                Break(player, target, hit, tier, blockClass, requiredTier);
                Reset();
            }
        }

        private static float SpeedPerSecond(int tier, string prefabName)
        {
            return ToolTiers.SpeedForTier(tier) / BlockHardness.For(prefabName) * TicksPerSecond;
        }

        private static void RaiseTick(Player player, RaycastHit hit, int tier)
        {
            MineheimEvents.RaiseMineTick(new MineTickEvent
            {
                Player = player,
                HitPoint = hit.point,
                ToolTier = tier,
                Progress = Mathf.Clamp01(_progress),
            });
        }

        private static void Break(
            Player player, MineTarget target, RaycastHit hit,
            int tier, BlockClass blockClass, int requiredTier)
        {
            int? drop = ToolTiers.DropAllowed(requiredTier, tier)
                ? ValheimToMinecraftDrop.TryGetDrop(target.Name)
                : (int?)MinecraftItemId.None; // stone-class below tier: breaks, no drop

            MineheimEvents.RaiseBlockBreak(new BlockBreakEvent
            {
                Player = player,
                Position = hit.point,
                ValheimPrefabName = target.Name,
                MinecraftDrop = drop,
            });

            if (drop != null)
            {
                target.SuppressDrop();
            }

            // Break through Valheim's own pipeline so the object is removed by the game.
            // The finisher hit maxes m_toolTier so m_minToolTier never blocks the break.
            // MineRock.Damage returns immediately when m_hitCollider is null, and
            // MineRock5 needs a positive radius for its area query: both are set.
            var finisher = new HitData
            {
                m_point = hit.point,
                m_dir = hit.normal,
                m_toolTier = short.MaxValue,
                m_hitCollider = hit.collider,
                m_radius = 1f,
            };
            finisher.m_damage.m_damage = target.Health + 1f;
            target.Damage(finisher);
        }

        private static void Reset()
        {
            _target = null;
            _progress = 0f;
        }
    }

    /// <summary>
    /// Mining input tick (PROTOCOL.md tick order), running beside the movement postfix on
    /// Player.Update while Mineheim mode is on.
    /// </summary>
    [HarmonyPatch(typeof(Player), "Update")]
    internal static class PlayerMiningPatch
    {
        private static void Postfix(Player __instance)
        {
            if (MineheimPlugin.IsMinecraftMode(__instance))
            {
                MinecraftMining.Tick(__instance, Time.deltaTime);
            }
        }
    }
}
