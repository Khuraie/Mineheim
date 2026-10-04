using UnityEngine;

namespace Mineheim
{
    /// <summary>Diamond Ore (M4b starter set).</summary>
    public static class MinecraftDiamondOreBlock
    {
        public const string PrefabName = "MinecraftDiamondOreBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Diamond Ore", new Color32(125, 125, 125, 255), new Color32(93, 236, 231, 255));
        }
    }
}
