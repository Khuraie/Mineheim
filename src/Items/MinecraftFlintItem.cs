using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft Flint (M4b starter set: conversion drop, not placeable).</summary>
    public static class MinecraftFlintItem
    {
        public const string PrefabName = "MinecraftFlintItem";

        public static GameObject Create()
        {
            return BlockRegistry.BuildItemPrefab(
                PrefabName,
                "Minecraft Flint",
                "Sharp flakes for tools.",
                new Color32(58, 52, 48, 255),
                new Color32(36, 32, 30, 255));
        }
    }
}
