using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Oak Log (M4b starter set: drops from mining, places the block).</summary>
    public static class MinecraftOakLogItem
    {
        public const string PrefabName = "MinecraftOakLogItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Oak Log",
                "Places an Oak Log.",
                new Color32(104, 82, 50, 255),
                new Color32(78, 60, 36, 255));
        }
    }
}
