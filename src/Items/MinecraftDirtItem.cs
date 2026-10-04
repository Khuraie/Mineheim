using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Dirt (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftDirtItem
    {
        public const string PrefabName = "MinecraftDirtItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Dirt",
                "Places a Dirt Block.",
                new Color32(121, 85, 58, 255),
                new Color32(94, 64, 42, 255));
        }
    }
}
