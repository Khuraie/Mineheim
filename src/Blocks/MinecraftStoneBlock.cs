using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft stone block (M4a: the first registered Mineheim block).</summary>
    public static class MinecraftStoneBlock
    {
        public const string PrefabName = "MinecraftStoneBlock";

        public static GameObject Create()
        {
            // Our own 16x16 palette: stone grey with darker flecks.
            return BlockRegistry.BuildBlockPrefab(PrefabName, "Stone Block", new Color32(125, 125, 125, 255), new Color32(90, 90, 90, 255));
        }
    }
}
