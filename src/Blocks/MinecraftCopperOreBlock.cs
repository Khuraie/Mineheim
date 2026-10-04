using UnityEngine;

namespace Mineheim
{
    /// <summary>Copper Ore (M4b starter set).</summary>
    public static class MinecraftCopperOreBlock
    {
        public const string PrefabName = "MinecraftCopperOreBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Copper Ore", new Color32(125, 125, 125, 255), new Color32(217, 121, 65, 255));
        }
    }
}
