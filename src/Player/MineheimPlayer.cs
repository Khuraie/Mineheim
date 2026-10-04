using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Mineheim
{
    /// <summary>
    /// Per-player Mineheim state. PROTOCOL.md "Subsystem boundaries": every subsystem owns
    /// its own state; everyone else reads through accessors.
    ///
    /// v0.1.0 toggle scope is world-level and single-player only (DESIGN.md #8), so exactly
    /// one MineheimPlayer exists per world. Movement, mining, and building state will live
    /// in their own subsystems in later milestones.
    /// </summary>
    public sealed class MineheimPlayer
    {
        /// <summary>
        /// DESIGN.md #4: the player must be standing still to toggle. Below this speed
        /// (m/s, from Character.GetVelocity) the player counts as standing still.
        /// </summary>
        private const float StandingStillThreshold = 0.1f;

        /// <summary>
        /// DESIGN.md #4: the player must be out of combat to toggle. Mirrors Valheim's own
        /// combat window: Player.CanSwitchPVP() allows switching only when
        /// Humanoid.m_lastCombatTimer (seconds since this humanoid last started an attack)
        /// is greater than 10 (verified against assembly_valheim 1.0.16 IL). The field is
        /// private, so it is read through Harmony's AccessTools.
        /// </summary>
        private const float CombatWindowSeconds = 10f;

        private static readonly FieldInfo LastCombatTimerField =
            AccessTools.Field(typeof(Humanoid), "m_lastCombatTimer");

        private static readonly Dictionary<Player, MineheimPlayer> Players = new Dictionary<Player, MineheimPlayer>();

        /// <summary>The Valheim player this state belongs to.</summary>
        public Player Player { get; }

        /// <summary>
        /// Whether Mineheim mode is active for this player. Owned here; read by everyone
        /// else through MineheimPlugin.IsMinecraftMode (PROTOCOL.md).
        /// </summary>
        public bool MineheimMode { get; private set; }

        private MineheimPlayer(Player player)
        {
            Player = player;
        }

        /// <summary>Get (or create) the Mineheim state of a Valheim player.</summary>
        public static MineheimPlayer Get(Player player)
        {
            if (player == null)
            {
                return null;
            }

            PruneDestroyed();

            if (!Players.TryGetValue(player, out var state))
            {
                state = new MineheimPlayer(player);
                Players.Add(player, state);
                MineheimLog.Debug("Mineheim state created for " + player.GetPlayerName());
            }

            return state;
        }

        /// <summary>PROTOCOL.md mode accessor used by subsystem hooks.</summary>
        public static bool IsMinecraftMode(Player player)
        {
            var state = Get(player);
            return state != null && state.MineheimMode;
        }

        /// <summary>
        /// DESIGN.md #4 gate: Mineheim mode may only be toggled while standing still and
        /// out of combat. Returns false plus a human-readable reason when refused.
        /// </summary>
        public bool CanToggle(out string reason)
        {
            if (!IsStandingStill(Player))
            {
                reason = "you must be standing still";
                return false;
            }

            if (IsInCombat(Player))
            {
                reason = "you are in combat";
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>Flip Mineheim mode. Only called after CanToggle passed.</summary>
        public void Toggle()
        {
            MineheimMode = !MineheimMode;
        }

        private static bool IsStandingStill(Player player)
        {
            var velocity = player.GetVelocity();
            return velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z
                <= StandingStillThreshold * StandingStillThreshold;
        }

        private static bool IsInCombat(Player player)
        {
            if (LastCombatTimerField == null)
            {
                // Without the field we cannot detect combat. Err on the side of letting the
                // toggle work rather than leaving F5 dead on a future Valheim build.
                return false;
            }

            return (float)LastCombatTimerField.GetValue(player) <= CombatWindowSeconds;
        }

        /// <summary>
        /// Drop state of destroyed players (Unity overloads == null on destroyed objects).
        /// </summary>
        private static void PruneDestroyed()
        {
            List<Player> dead = null;
            foreach (var player in Players.Keys)
            {
                if (player == null)
                {
                    if (dead == null)
                    {
                        dead = new List<Player>();
                    }

                    dead.Add(player);
                }
            }

            if (dead != null)
            {
                foreach (var player in dead)
                {
                    Players.Remove(player);
                }
            }
        }
    }
}
