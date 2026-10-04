using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft stone item (M4a: drops from mined stone, places the stone block).</summary>
    public static class MinecraftStoneItem
    {
        public const string PrefabName = "MinecraftStoneItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Stone",
                "Places a Stone Block.",
                new Color32(125, 125, 125, 255),
                new Color32(90, 90, 90, 255));
        }
    }
}
