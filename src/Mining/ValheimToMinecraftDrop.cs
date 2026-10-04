using System.Collections.Generic;
using UnityEngine;

namespace Mineheim
{
    /// <summary>Minecraft drop ids carried by PROTOCOL.md's BlockBreakEvent.MinecraftDrop.</summary>
    public static class MinecraftItemId
    {
        public const int None = 0;         // explicit "no drop" (below-tier stone class)
        public const int Stone = 1;
        public const int Dirt = 2;
        public const int OakLog = 3;
        public const int OakPlanks = 4;
        public const int DarkOakLog = 5;
        public const int CopperOre = 6;
        public const int IronOre = 7;
        public const int GoldOre = 8;
        public const int DiamondOre = 9;
        public const int Obsidian = 10;
        public const int Flint = 11;
        public const int Leather = 12;

        /// <summary>Item prefab names registered by the Items/ subsystem (M4).</summary>
        public static string PrefabName(int id)
        {
            switch (id)
            {
                case Stone: return "MinecraftStoneItem";
                case Dirt: return "MinecraftDirtItem";
                case OakLog: return "MinecraftOakLogItem";
                case OakPlanks: return "MinecraftOakPlanksItem";
                case DarkOakLog: return "MinecraftDarkOakLogItem";
                case CopperOre: return "MinecraftCopperOreItem";
                case IronOre: return "MinecraftIronOreItem";
                case GoldOre: return "MinecraftGoldOreItem";
                case DiamondOre: return "MinecraftDiamondOreItem";
                case Obsidian: return "MinecraftObsidianItem";
                case Flint: return "MinecraftFlintItem";
                case Leather: return "MinecraftLeatherItem";
                default: return null;
            }
        }
    }

    /// <summary>Block classes for PROTOCOL.md "Tool tiers" gating.</summary>
    public enum BlockClass
    {
        Loose, // hand: dirt, grass, sand, snow
        Wood,  // logs and planks (hand-minable in Minecraft)
        Stone, // stone-class: below tier the block breaks but drops nothing
        Ore,   // ore-class: below tier the block does not break at all
    }

    /// <summary>
    /// M3b: the Valheim -> Minecraft drop table (PROTOCOL.md "Block mapping"). Unmapped
    /// prefabs return null and fall back to Valheim's native drops. Destructibles with
    /// custom behavior (boss altars, chests, portals) always fall back - they carry data
    /// and must not be converted. Hardmode resources (black metal, silver, mistlands) also
    /// fall back (DESIGN.md #9); the table below only maps early-game resources.
    /// TODO(spec): keys match the prefab/resource names of pinned build 25527674; widen
    /// with substring fallbacks if world testing shows renamed prefabs.
    /// </summary>
    public static class ValheimToMinecraftDrop
    {
        private static readonly Dictionary<string, int> Table = new Dictionary<string, int>
        {
            ["Rock"] = MinecraftItemId.Stone,
            ["stone"] = MinecraftItemId.Stone,
            ["Dirt"] = MinecraftItemId.Dirt,
            ["Wood"] = MinecraftItemId.OakLog,
            ["FineWood"] = MinecraftItemId.OakPlanks,
            ["CoreWood"] = MinecraftItemId.DarkOakLog,
            ["CopperOre"] = MinecraftItemId.CopperOre,
            ["TinOre"] = MinecraftItemId.IronOre,
            ["IronScrap"] = MinecraftItemId.IronOre,
            ["Obsidian"] = MinecraftItemId.Obsidian,
            ["Crystal"] = MinecraftItemId.DiamondOre,
            ["Flint"] = MinecraftItemId.Flint,
            ["LeatherScraps"] = MinecraftItemId.Leather,
        };

        // DESIGN.md #9: silver and hardmode resources drop their vanilla items.
        private static readonly HashSet<string> NeverConvert = new HashSet<string>
        {
            "SilverOre",
        };

        /// <summary>
        /// Map a broken prefab to a Minecraft drop id. Null = use Valheim's native drop
        /// (unmapped, custom destructible, or hardmode resource).
        /// </summary>
        public static int? TryGetDrop(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return null;
            }

            string name = prefabName.Replace("(Clone)", string.Empty);
            if (NeverConvert.Contains(name))
            {
                return null;
            }
            if (name.Contains("altar") || name.Contains("chest") || name.Contains("portal"))
            {
                return null; // carries data, never converted (PROTOCOL.md)
            }

            if (Table.TryGetValue(name, out int drop))
            {
                return drop;
            }

            // Prefab-name fallbacks for named destructibles (rock1, ancient tree, ...).
            string lower = name.ToLowerInvariant();
            if (lower.Contains("rock") || lower.Contains("flint"))
            {
                return lower.Contains("flint") ? MinecraftItemId.Flint : MinecraftItemId.Stone;
            }
            if (lower.Contains("copper"))
            {
                return MinecraftItemId.CopperOre;
            }
            if (lower.Contains("tin") || lower.Contains("iron"))
            {
                return MinecraftItemId.IronOre;
            }
            if (lower.Contains("obsidian"))
            {
                return MinecraftItemId.Obsidian;
            }
            return null;
        }

        /// <summary>Minecraft tier required to break (PROTOCOL.md "Tool tiers" table).</summary>
        public static int RequiredTier(string prefabName)
        {
            int? drop = TryGetDrop(prefabName);
            switch (drop)
            {
                case MinecraftItemId.Stone: return ToolTiers.Wood;
                case MinecraftItemId.CopperOre:
                case MinecraftItemId.IronOre: return ToolTiers.Stone;
                case MinecraftItemId.GoldOre:
                case MinecraftItemId.DiamondOre: return ToolTiers.Iron;
                case MinecraftItemId.Obsidian: return ToolTiers.Diamond;
                default: return ToolTiers.Hand;
            }
        }

        /// <summary>Class for the below-tier rules (break-no-drop vs no-break).</summary>
        public static BlockClass Classify(string prefabName)
        {
            int? drop = TryGetDrop(prefabName);
            switch (drop)
            {
                case MinecraftItemId.CopperOre:
                case MinecraftItemId.IronOre:
                case MinecraftItemId.GoldOre:
                case MinecraftItemId.DiamondOre: return BlockClass.Ore;
                case MinecraftItemId.Stone:
                case MinecraftItemId.Obsidian: return BlockClass.Stone;
                case MinecraftItemId.OakLog:
                case MinecraftItemId.OakPlanks:
                case MinecraftItemId.DarkOakLog: return BlockClass.Wood;
                default: return BlockClass.Loose;
            }
        }
    }
}
