using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Diamond Ore (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftDiamondOreItem
    {
        public const string PrefabName = "MinecraftDiamondOreItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Diamond Ore",
                "Places Diamond Ore.",
                new Color32(125, 125, 125, 255),
                new Color32(93, 236, 231, 255));
        }
    }
}
