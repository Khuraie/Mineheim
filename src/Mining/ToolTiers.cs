namespace Mineheim
{
    /// <summary>
    /// Minecraft tool tiers and speeds (PROTOCOL.md "Tool tiers"). Speeds are Minecraft's
    /// materials: hand 1, wood 2, stone 4, iron 6, diamond 8.
    /// TODO(spec): CurrentTier reads Valheim's m_toolTier as a stand-in for the Minecraft
    /// tier ladder (fists 0, stone 1, bronze 2, iron 3, silver/blackmetal 4+). Swap for a
    /// real Valheim-tool -> Minecraft-tier mapping if in-game gating feels off.
    /// </summary>
    public static class ToolTiers
    {
        public const int Hand = 0;
        public const int Wood = 1;
        public const int Stone = 2;
        public const int Iron = 3;
        public const int Diamond = 4;
        public const int Netherite = 5;

        /// <summary>Tier of the equipped tool; 0 is an empty hand.</summary>
        public static int CurrentTier(Player player)
        {
            var weapon = player.GetCurrentWeapon();
            return weapon == null ? Hand : weapon.m_shared.m_toolTier;
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

        /// <summary>
        /// PROTOCOL.md break condition: Progress >= 1 && CanBreak(blockType, toolTier).
        /// Below-tier on ore-class blocks the block does not break at all.
        /// </summary>
        public static bool CanBreak(BlockClass blockClass, int requiredTier, int toolTier)
        {
            return blockClass != BlockClass.Ore || toolTier >= requiredTier;
        }

        /// <summary>
        /// PROTOCOL.md: below-tier on stone-class blocks the block breaks but drops nothing.
        /// </summary>
        public static bool DropAllowed(int requiredTier, int toolTier)
        {
            return toolTier >= requiredTier;
        }
    }
}
