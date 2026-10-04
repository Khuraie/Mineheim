using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Copper Ore (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftCopperOreItem
    {
        public const string PrefabName = "MinecraftCopperOreItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Copper Ore",
                "Places Copper Ore.",
                new Color32(125, 125, 125, 255),
                new Color32(217, 121, 65, 255));
        }
    }
}
