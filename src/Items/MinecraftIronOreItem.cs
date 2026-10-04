using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Iron Ore (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftIronOreItem
    {
        public const string PrefabName = "MinecraftIronOreItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Iron Ore",
                "Places Iron Ore.",
                new Color32(125, 125, 125, 255),
                new Color32(216, 175, 147, 255));
        }
    }
}
