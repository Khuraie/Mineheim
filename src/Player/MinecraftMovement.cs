using System.Reflection;
using HarmonyLib;
using UnityEngine;

// PROTOCOL.md mandates writing m_body.velocity (never transform.position). Unity 6 marks
// Rigidbody.velocity obsolete in favor of linearVelocity (same physics state), so the
// deprecation notice is suppressed in this file to keep the contract's literal API.
#pragma warning disable CS0618

namespace Mineheim
{
    /// <summary>
    /// M2 movement. PROTOCOL.md "Movement contract": compute the desired velocity from
    /// input and player state and write only m_body.velocity - Valheim's physics resolves
    /// collision and commits position. Never write transform.position.
    ///
    /// M2a: walk, jump, gravity, ground detection. M2b: sprint, air control, water,
    /// fall damage.
    /// </summary>
    public static class MinecraftMovement
    {
        /// <summary>Valheim ZInput action for sprinting (verified against assembly_valheim 1.0.16 strings).</summary>
        private const string SprintAction = "Run";
        private const string JumpAction = "Jump";

        /// <summary>HitData.m_hitType value Valheim tags fall damage with (UpdateGroundContact, 1.0.16 IL).</summary>
        internal const int FallHitType = 3;

        // assembly_valheim 1.0.16 keeps these private; they are reached through Harmony.
        // Only m_body.velocity is ever written (PROTOCOL.md rule); m_maxAirAltitude is read
        // only, to get Valheim's own fall height for the Minecraft fall damage formula.
        private static readonly AccessTools.FieldRef<Character, Rigidbody> BodyRef =
            AccessTools.FieldRefAccess<Character, Rigidbody>("m_body");

        private static readonly FieldInfo MaxAirAltitudeField =
            AccessTools.Field(typeof(Character), "m_maxAirAltitude");

        /// <summary>Desired velocity for this frame (PROTOCOL.md sample signature).</summary>
        public static Vector3 ComputeVelocity(Player player)
        {
            Character character = player;
            Rigidbody body = BodyRef(character);
            if (body == null)
            {
                return Vector3.zero;
            }

            Vector3 velocity = body.velocity;
            float dt = Time.deltaTime;
            Vector3 wish = WishDirection();
            float speed = ZInput.GetButton(SprintAction)
                ? MinecraftPhysics.SprintSpeedPerSecond
                : MinecraftPhysics.WalkSpeedPerSecond;

            if (player.IsSwimming())
            {
                // Water: slower move, jump rises, otherwise a gentle capped sink.
                velocity.x = wish.x * speed * MinecraftPhysics.WaterSpeedFactor;
                velocity.z = wish.z * speed * MinecraftPhysics.WaterSpeedFactor;
                velocity.y = ZInput.GetButton(JumpAction)
                    ? MinecraftPhysics.SwimUpSpeedPerSecond
                    : Mathf.Max(
                        velocity.y - MinecraftPhysics.WaterSinkAccelerationPerSecond * dt,
                        MinecraftPhysics.WaterTerminalSinkPerSecond);
            }
            else if (player.IsOnGround())
            {
                velocity.x = wish.x * speed;
                velocity.z = wish.z * speed;
                velocity.y = ZInput.GetButton(JumpAction) ? MinecraftPhysics.JumpVelocityPerSecond : 0f;
            }
            else
            {
                // Air: momentum is kept without input (Minecraft), and steering accelerates
                // weakly toward the wish speed. Gravity pulls, clamped at terminal velocity.
                if (wish.sqrMagnitude > 0.0001f)
                {
                    Vector3 horizontal = Vector3.MoveTowards(
                        new Vector3(velocity.x, 0f, velocity.z),
                        wish * speed,
                        MinecraftPhysics.AirControlAccelerationPerSecond * dt);
                    velocity.x = horizontal.x;
                    velocity.z = horizontal.z;
                }

                velocity.y = Mathf.Max(
                    velocity.y - MinecraftPhysics.GravityPerSecond * dt,
                    -MinecraftPhysics.TerminalVelocityPerSecond);
            }

            return velocity;
        }

        /// <summary>Commit the computed velocity. Velocity only - never transform.position.</summary>
        public static void SetVelocity(Player player, Vector3 velocity)
        {
            Character character = player;
            Rigidbody body = BodyRef(character);
            if (body != null)
            {
                body.velocity = velocity;
            }
        }

        /// <summary>
        /// Walk input via Valheim's ZInput actions, camera-relative and flattened
        /// (PROTOCOL.md "Aim contract": Minecraft systems read camera forward, no cursor).
        /// </summary>
        private static Vector3 WishDirection()
        {
            if (GameCamera.instance == null)
            {
                return Vector3.zero;
            }
            Transform cam = GameCamera.instance.transform;
            Vector3 forward = cam.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cam.right;
            right.y = 0f;
            right.Normalize();

            Vector3 wish = Vector3.zero;
            if (ZInput.GetButton("Forward"))
            {
                wish += forward;
            }
            if (ZInput.GetButton("Backward"))
            {
                wish -= forward;
            }
            if (ZInput.GetButton("Right"))
            {
                wish += right;
            }
            if (ZInput.GetButton("Left"))
            {
                wish -= right;
            }

            return Vector3.ClampMagnitude(wish, 1f);
        }

        /// <summary>
        /// Minecraft's fall damage formula: fallDistance - 3 blocks of damage. Fall height
        /// comes from Valheim's own air-altitude tracking (read-only). DESIGN.md #7: the
        /// damage lands on Valheim's HP scale - no second HP scale is introduced.
        /// </summary>
        internal static HitData BuildFallDamage(Player player, HitData originalHit)
        {
            float peak = MaxAirAltitudeField != null
                ? (float)MaxAirAltitudeField.GetValue(player)
                : player.transform.position.y;
            float fallDistance = Mathf.Max(0f, peak - player.transform.position.y);

            HitData hit = originalHit;
            hit.m_damage.m_damage = Mathf.Max(0f, fallDistance - MinecraftPhysics.SafeFallDistance);
            return hit;
        }
    }

    /// <summary>
    /// PROTOCOL.md "Movement contract" tick order steps 1-2: while Mineheim mode is on,
    /// the Player.Update postfix computes and commits the Minecraft velocity every frame.
    /// </summary>
    [HarmonyPatch(typeof(Player), "Update")]
    internal static class PlayerUpdatePatch
    {
        private static void Postfix(Player __instance)
        {
            if (!MineheimPlugin.IsMinecraftMode(__instance))
            {
                return;
            }

            MinecraftMovement.SetVelocity(__instance, MinecraftMovement.ComputeVelocity(__instance));
        }
    }

    /// <summary>
    /// M2b fall damage. Valheim applies its own formula in Character.UpdateGroundContact
    /// (percentage of height above 4 m). While Mineheim mode is on, that hit is replaced by
    /// Minecraft's formula (fallDistance - 3); landing in water cancels it, like Minecraft.
    /// Only the local Mineheim player is affected - Valheim rules stay for all other
    /// characters (DESIGN.md scope).
    /// </summary>
    [HarmonyPatch(typeof(Character), "Damage")]
    internal static class CharacterDamagePatch
    {
        private static bool Prefix(Character __instance, HitData hit)
        {
            if (MinecraftMovementState.ApplyingMinecraftFallDamage
                || (int)hit.m_hitType != MinecraftMovement.FallHitType)
            {
                return true;
            }

            var player = __instance as Player;
            if (player == null || !MineheimPlugin.IsMinecraftMode(player))
            {
                return true;
            }

            if (player.IsSwimming() || player.InWater())
            {
                return false; // Minecraft: entering water cancels fall damage.
            }

            HitData minecraftHit = MinecraftMovement.BuildFallDamage(player, hit);
            if (minecraftHit.m_damage.m_damage <= 0f)
            {
                return false;
            }

            MinecraftMovementState.ApplyingMinecraftFallDamage = true;
            try
            {
                player.Damage(minecraftHit);
            }
            finally
            {
                MinecraftMovementState.ApplyingMinecraftFallDamage = false;
            }

            return false;
        }
    }

    /// <summary>Shared re-entry flag for the fall damage swap.</summary>
    internal static class MinecraftMovementState
    {
        public static bool ApplyingMinecraftFallDamage;
    }
}
