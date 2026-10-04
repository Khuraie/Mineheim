using UnityEngine;

namespace Mineheim
{
    /// <summary>Dirt Block (M4b starter set).</summary>
    public static class MinecraftDirtBlock
    {
        public const string PrefabName = "MinecraftDirtBlock";

        public static GameObject Create()
        {
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Dirt Block", new Color32(121, 85, 58, 255), new Color32(94, 64, 42, 255));
        }
    }
}
