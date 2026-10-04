namespace Mineheim
{
    /// <summary>
    /// Minecraft tool speeds (Minecraft materials: hand 1, wood 2, stone 4, iron 6,
    /// diamond 8) keyed by the equipped Valheim tool tier.
    /// TODO(spec): M3b's ToolTiers maps real Minecraft tier names and adds CanBreak
    /// gating (PROTOCOL.md "Tool tiers", including the below-tier drop/no-break rules);
    /// until then tiers only affect mining speed and never gate the break.
    /// </summary>
    public static class ToolTiers
    {
        /// <summary>Tier of the equipped tool; 0 is an empty hand.</summary>
        public static int CurrentTier(Player player)
        {
            var weapon = player.GetCurrentWeapon();
            return weapon == null ? 0 : weapon.m_shared.m_toolTier;
        }

        public static float SpeedForTier(int tier)
        {
            switch (tier)
            {
                case 0: return 1f;  // hand
                case 1: return 2f;  // wood
                case 2: return 4f;  // stone
                case 3: return 6f;  // iron
                default: return 8f; // diamond
            }
        }
    }
}
