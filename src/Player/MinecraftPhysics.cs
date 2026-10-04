namespace Mineheim
{
    /// <summary>
    /// Minecraft physics constants (PROTOCOL.md "Physics constants"). The five pinned
    /// constants are Minecraft's 20 Hz tick units; the *PerSecond forms convert them for
    /// Valheim's rigidbody: velocity (m/s) = m/tick * 20, acceleration (m/s^2) = m/tick^2 * 400.
    /// </summary>
    public static class MinecraftPhysics
    {
        public const float Gravity = 0.08f;            // m/tick^2 (1 tick = 1/20 s)
        public const float TerminalVelocity = 3.92f;   // m/tick
        public const float JumpVelocity = 0.42f;       // m/tick
        public const float WalkSpeed = 4.317f / 20f;   // m/tick
        public const float SprintSpeed = 5.612f / 20f; // m/tick

        private const float TicksPerSecond = 20f;

        public const float GravityPerSecond = Gravity * TicksPerSecond * TicksPerSecond;   // 32 m/s^2
        public const float TerminalVelocityPerSecond = TerminalVelocity * TicksPerSecond;  // 78.4 m/s
        public const float JumpVelocityPerSecond = JumpVelocity * TicksPerSecond;          // 8.4 m/s
        public const float WalkSpeedPerSecond = WalkSpeed * TicksPerSecond;                // 4.317 m/s
        public const float SprintSpeedPerSecond = SprintSpeed * TicksPerSecond;            // 5.612 m/s

        // --- The M2b values below are not pinned by PROTOCOL.md or DESIGN.md.
        // TODO(spec): pin or tune these against Minecraft's feel before v0.1.0. ---

        /// <summary>In-air steering acceleration toward the wish direction (Minecraft steers weakly in air).</summary>
        public const float AirControlAccelerationPerSecond = WalkSpeedPerSecond * 2f;      // ~8.6 m/s^2

        /// <summary>Water movement is slower than walking (Minecraft swimming).</summary>
        public const float WaterSpeedFactor = 0.5f;

        /// <summary>Rise speed while holding jump in water.</summary>
        public const float SwimUpSpeedPerSecond = JumpVelocityPerSecond * 0.5f;            // 4.2 m/s

        /// <summary>Gentle sink in water while not rising.</summary>
        public const float WaterSinkAccelerationPerSecond = GravityPerSecond * 0.25f;      // 8 m/s^2
        public const float WaterTerminalSinkPerSecond = -2f;

        /// <summary>Minecraft fall damage: none within 3 blocks, then fallDistance - 3.</summary>
        public const float SafeFallDistance = 3f;
    }
}
