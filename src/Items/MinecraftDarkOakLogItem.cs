using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Dark Oak Log (M4b starter set: conversion drop, not placeable).</summary>
    public static class MinecraftDarkOakLogItem
    {
        public const string PrefabName = "MinecraftDarkOakLogItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Dark Oak Log",
                "Dense dark wood.",
                new Color32(66, 51, 36, 255),
                new Color32(48, 36, 24, 255));
        }
    }
}
