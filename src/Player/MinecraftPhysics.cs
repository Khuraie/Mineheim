namespace Mineheim
{
    /// <summary>
    /// Minecraft physics constants (PROTOCOL.md "Physics constants"). The five constants
    /// are Minecraft's 20 Hz tick units; the *PerSecond forms convert them for Valheim's
    /// rigidbody: velocity (m/s) = m/tick * 20, acceleration (m/s^2) = m/tick^2 * 400.
    /// </summary>
    public static class MinecraftPhysics
    {
        public const float Gravity = 0.08f;            // m/tick^2 (1 tick = 1/20 s)
        public const float TerminalVelocity = 3.92f;   // m/tick
        public const float JumpVelocity = 0.42f;       // m/tick
        public const float WalkSpeed = 4.317f / 20f;   // m/tick
        public const float SprintSpeed = 5.612f / 20f; // m/tick (M2b)

        private const float TicksPerSecond = 20f;

        public const float GravityPerSecond = Gravity * TicksPerSecond * TicksPerSecond;   // 32 m/s^2
        public const float TerminalVelocityPerSecond = TerminalVelocity * TicksPerSecond;  // 78.4 m/s
        public const float JumpVelocityPerSecond = JumpVelocity * TicksPerSecond;          // 8.4 m/s
        public const float WalkSpeedPerSecond = WalkSpeed * TicksPerSecond;                // 4.317 m/s
        public const float SprintSpeedPerSecond = SprintSpeed * TicksPerSecond;            // 5.612 m/s
    }
}
