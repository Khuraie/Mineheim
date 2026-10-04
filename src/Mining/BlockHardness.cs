namespace Mineheim
{
    /// <summary>
    /// Block hardness for the mining formula (PROTOCOL.md "Mining events":
    /// Progress += toolSpeed / hardness each Minecraft tick; break at Progress >= 1).
    ///
    /// Hardness is expressed in hand-speed ticks, i.e. Minecraft hardness x 30 (Minecraft's
    /// destroy constant), which keeps the formula literal and the break times Minecraft-like
    /// (stone with a wooden pick: 45/2 = 22 ticks ~= 1.1 s). Keys are matched against the
    /// Valheim prefab name.
    /// TODO(spec): PROTOCOL.md pins no hardness numbers and M3b's drop table is not landed
    /// yet; replace these fragments with the real block mapping when it is.
    /// </summary>
    public static class BlockHardness
    {
        /// <summary>One second of mining with an empty hand.</summary>
        public const float DefaultHardness = 30f;

        // Minecraft hardness x 30: stone 1.5 -> 45, ores 3.0 -> 90, wood 2.0 -> 60.
        public static float For(Destructible destructible)
        {
            string name = destructible.name.ToLowerInvariant();
            if (name.Contains("rock") || name.Contains("stone") || name.Contains("flint"))
            {
                return 45f;
            }
            if (name.Contains("ore") || name.Contains("copper") || name.Contains("tin") || name.Contains("iron"))
            {
                return 90f;
            }
            if (name.Contains("tree") || name.Contains("log") || name.Contains("pine")
                || name.Contains("beech") || name.Contains("birch") || name.Contains("fir"))
            {
                return 60f;
            }
            return DefaultHardness;
        }
    }
}
