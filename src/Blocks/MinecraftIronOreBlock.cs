using UnityEngine;

namespace Mineheim
{
    /// <summary>Iron Ore (M4b starter set).</summary>
    public static class MinecraftIronOreBlock
    {
        public const string PrefabName = "MinecraftIronOreBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Iron Ore", new Color32(125, 125, 125, 255), new Color32(216, 175, 147, 255));
        }
    }
}
