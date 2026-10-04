using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Oak Planks (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftOakPlanksItem
    {
        public const string PrefabName = "MinecraftOakPlanksItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Oak Planks",
                "Places Oak Planks.",
                new Color32(178, 143, 90, 255),
                new Color32(150, 118, 70, 255));
        }
    }
}
