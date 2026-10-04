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
    public static class MinecraftMining
    {
        private const float TicksPerSecond = 20f; // Minecraft tick rate (PROTOCOL.md)
        private const string MineButton = "Attack";

        private static Destructible _target;
        private static float _progress;

        public static void Tick(Player player, float dt)
        {
            if (!ZInput.GetButton(MineButton) || !Aim.Raycast(player, Aim.Reach, out var hit))
            {
                Reset();
                return;
            }

            var destructible = hit.collider.GetComponentInParent<Destructible>();
            if (destructible == null)
            {
                Reset();
                return;
            }

            if (destructible != _target)
            {
                _target = destructible;
                _progress = 0f;
            }

            string prefabName = destructible.name;
            int tier = ToolTiers.CurrentTier(player);
            BlockClass blockClass = ValheimToMinecraftDrop.Classify(prefabName);
            int requiredTier = ValheimToMinecraftDrop.RequiredTier(prefabName);

            if (!ToolTiers.CanBreak(blockClass, requiredTier, tier))
            {
                // Ore-class below tier: the block does not break (PROTOCOL.md). Progress
                // tops out at 1 and waits for a better tool.
                _progress = Mathf.Min(1f, _progress + SpeedPerSecond(tier, destructible) * dt);
                RaiseTick(player, hit, tier);
                return;
            }

            _progress += SpeedPerSecond(tier, destructible) * dt;
            RaiseTick(player, hit, tier);

            if (_progress >= 1f)
            {
                Break(player, destructible, hit, tier, blockClass, requiredTier);
                Reset();
            }
        }

        private static float SpeedPerSecond(int tier, Destructible destructible)
        {
            return ToolTiers.SpeedForTier(tier) / BlockHardness.For(destructible) * TicksPerSecond;
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
            Player player, Destructible destructible, RaycastHit hit,
            int tier, BlockClass blockClass, int requiredTier)
        {
            int? drop = ToolTiers.DropAllowed(requiredTier, tier)
                ? ValheimToMinecraftDrop.TryGetDrop(destructible.name)
                : (int?)MinecraftItemId.None; // stone-class below tier: breaks, no drop

            MineheimEvents.RaiseBlockBreak(new BlockBreakEvent
            {
                Player = player,
                Position = hit.point,
                ValheimPrefabName = destructible.name,
                MinecraftDrop = drop,
            });

            if (drop != null)
            {
                MinecraftDrops.SuppressVanillaDrop(destructible);
            }

            // Break through Valheim's own pipeline so the object is removed by the game.
            // The finisher hit maxes m_toolTier so m_minToolTier never blocks the break.
            var finisher = new HitData
            {
                m_point = hit.point,
                m_dir = hit.normal,
                m_toolTier = short.MaxValue,
            };
            finisher.m_damage.m_damage = destructible.m_health + 1f;
            destructible.Damage(finisher);
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
