using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Leather (M4b starter set: conversion drop, not placeable).</summary>
    public static class MinecraftLeatherItem
    {
        public const string PrefabName = "MinecraftLeatherItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Leather",
                "Tanned hide.",
                new Color32(181, 130, 75, 255),
                new Color32(150, 105, 60, 255));
        }
    }
}
