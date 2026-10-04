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
        public int? MinecraftDrop; // null = use Valheim drop
    }

    /// <summary>
    /// Event hub (PROTOCOL.md "Rules": subsystems communicate via events only, events are
    /// synchronous and must not block). Mining raises; the Drops subsystem subscribes from
    /// M3b on.
    /// </summary>
    public static class MineheimEvents
    {
        public static event System.Action<MineTickEvent> OnMineTick;
        public static event System.Action<BlockBreakEvent> OnBlockBreak;

        internal static void RaiseMineTick(MineTickEvent e) => OnMineTick?.Invoke(e);

        internal static void RaiseBlockBreak(BlockBreakEvent e) => OnBlockBreak?.Invoke(e);
    }

    /// <summary>
    /// M3a mining core (PROTOCOL.md "Mining events"): hold the mine button (Valheim's
    /// "Attack" action) while looking at a Destructible within Aim.Reach. Progress grows
    /// at toolSpeed / hardness per Minecraft tick and the block breaks at 1.
    ///
    /// The break goes through Valheim's own Destructible pipeline so drops stay vanilla
    /// (M3a). M3b routes drops through ValheimToMinecraftDrop, adds CanBreak tier gating,
    /// and suppresses the Valheim drop via Destructible.m_destroyed handling.
    /// TODO(spec): Valheim melee swings also damage destructibles (Valheim systems stay,
    /// DESIGN.md). Decide in M3b whether to suppress that in Mineheim mode so hardness
    /// timing is pure.
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

            int tier = ToolTiers.CurrentTier(player);
            _progress += ToolTiers.SpeedForTier(tier) / BlockHardness.For(destructible) * TicksPerSecond * dt;

            MineheimEvents.RaiseMineTick(new MineTickEvent
            {
                Player = player,
                HitPoint = hit.point,
                ToolTier = tier,
                Progress = Mathf.Clamp01(_progress),
            });

            if (_progress >= 1f)
            {
                Break(player, destructible, hit);
                Reset();
            }
        }

        private static void Break(Player player, Destructible destructible, RaycastHit hit)
        {
            MineheimEvents.RaiseBlockBreak(new BlockBreakEvent
            {
                Player = player,
                Position = hit.point,
                ValheimPrefabName = destructible.name,
                MinecraftDrop = null, // M3a: vanilla Valheim drop.
            });

            // Break through Valheim's own pipeline so the vanilla spawn/drop
            // (m_spawnWhenDestroyed) happens. Tier gating is M3b's CanBreak, so the
            // finisher hit maxes the tool tier and m_minToolTier never blocks M3a.
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
