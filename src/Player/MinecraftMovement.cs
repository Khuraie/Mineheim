using HarmonyLib;
using UnityEngine;

// PROTOCOL.md mandates writing m_body.velocity (never transform.position). Unity 6 marks
// Rigidbody.velocity obsolete in favor of linearVelocity (same physics state), so the
// deprecation notice is suppressed in this file to keep the contract's literal API.
#pragma warning disable CS0618

namespace Mineheim
{
    /// <summary>
    /// M2a movement core. PROTOCOL.md "Movement contract": compute the desired velocity
    /// from input and player state and write only m_body.velocity - Valheim's physics
    /// resolves collision and commits position. Never write transform.position.
    ///
    /// In scope (M2a): walk, jump, gravity, ground detection. Sprint, air control,
    /// water, and fall damage arrive in M2b.
    /// </summary>
    public static class MinecraftMovement
    {
        // assembly_valheim 1.0.16 keeps Character.m_body private, so it is reached through
        // Harmony's field ref. Only m_body.velocity is ever written (PROTOCOL.md rule).
        private static readonly AccessTools.FieldRef<Character, Rigidbody> BodyRef =
            AccessTools.FieldRefAccess<Character, Rigidbody>("m_body");

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
            if (player.IsOnGround())
            {
                Vector3 wish = WishDirection();
                velocity.x = wish.x * MinecraftPhysics.WalkSpeedPerSecond;
                velocity.z = wish.z * MinecraftPhysics.WalkSpeedPerSecond;
                velocity.y = ZInput.GetButton("Jump") ? MinecraftPhysics.JumpVelocityPerSecond : 0f;
            }
            else
            {
                // Air: Minecraft keeps horizontal momentum while gravity pulls, clamped at
                // terminal velocity. Air control is M2b.
                velocity.y = Mathf.Max(
                    velocity.y - MinecraftPhysics.GravityPerSecond * Time.deltaTime,
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
}
