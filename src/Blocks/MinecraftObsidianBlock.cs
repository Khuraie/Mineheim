using UnityEngine;

namespace Mineheim
{
    /// <summary>Obsidian (M4b starter set).</summary>
    public static class MinecraftObsidianBlock
    {
        public const string PrefabName = "MinecraftObsidianBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Obsidian", new Color32(20, 16, 24, 255), new Color32(70, 40, 110, 255));
        }
    }
}
