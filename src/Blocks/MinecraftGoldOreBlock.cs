using UnityEngine;

namespace Mineheim
{
    /// <summary>Gold Ore (M4b starter set).</summary>
    public static class MinecraftGoldOreBlock
    {
        public const string PrefabName = "MinecraftGoldOreBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Gold Ore", new Color32(125, 125, 125, 255), new Color32(252, 238, 75, 255));
        }
    }
}
