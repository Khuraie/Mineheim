using UnityEngine;

namespace Mineheim
{
    /// <summary>Oak Planks (M4b starter set).</summary>
    public static class MinecraftOakPlanksBlock
    {
        public const string PrefabName = "MinecraftOakPlanksBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Oak Planks", new Color32(178, 143, 90, 255), new Color32(150, 118, 70, 255));
        }
    }
}
