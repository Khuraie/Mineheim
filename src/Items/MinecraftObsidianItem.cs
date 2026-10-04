using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Obsidian (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftObsidianItem
    {
        public const string PrefabName = "MinecraftObsidianItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Obsidian",
                "Places Obsidian.",
                new Color32(20, 16, 24, 255),
                new Color32(70, 40, 110, 255));
        }
    }
}
