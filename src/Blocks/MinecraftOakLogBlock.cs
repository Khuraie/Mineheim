using UnityEngine;

namespace Mineheim
{
    /// <summary>Oak Log (M4b starter set).</summary>
    public static class MinecraftOakLogBlock
    {
        public const string PrefabName = "MinecraftOakLogBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Oak Log", new Color32(104, 82, 50, 255), new Color32(78, 60, 36, 255));
        }
    }
}
