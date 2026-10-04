using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Gold Ore (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftGoldOreItem
    {
        public const string PrefabName = "MinecraftGoldOreItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Gold Ore",
                "Places Gold Ore.",
                new Color32(125, 125, 125, 255),
                new Color32(252, 238, 75, 255));
        }
    }
}
