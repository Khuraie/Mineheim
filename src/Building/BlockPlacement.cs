using HarmonyLib;
using UnityEngine;

namespace Mineheim
{
    /// <summary>PROTOCOL.md "Placement rules / OnPlaceBlock". Synchronous; do not block.</summary>
    public class PlaceBlockEvent
    {
        public Player Player;
        public Vector3 Position;
        public Vector3 Normal;
        public int ItemType;
        public int HotbarSlot; // 0..8
    }

    /// <summary>
    /// M4 placement (harness "Placing (M4)"): place input, raycast, offset by normal,
    /// spawn the Valheim prefab via ZNetScene.instance.GetPrefab + Instantiate. ZDO
    /// persistence is automatic (DESIGN.md #12).
    ///
    /// TODO(spec): the place button is Valheim's "Use" action; Minecraft players expect
    /// right mouse. ZInput actions rebind, but pick a final binding after playtesting.
    /// Placement is validated by the 5-rule CanPlaceAt below.
    /// </summary>
    public static class BlockPlacement
    {
        private const string PlaceButton = "Use";

        public static void Tick(Player player)
        {
            if (!ZInput.GetButtonDown(PlaceButton) || !Aim.Raycast(player, Aim.Reach, out var hit))
            {
                return;
            }

            Inventory inventory = player.GetInventory();
            var items = inventory.GetAllItems();
            string blockPrefabName = null;
            ItemDrop.ItemData consume = null;
            int slot = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (BlockRegistry.ItemNameToBlockPrefab.TryGetValue(items[i].m_shared.m_name, out blockPrefabName))
                {
                    consume = items[i];
                    slot = Mathf.Clamp(i, 0, 8); // TODO(spec): real quick-slot index
                    break;
                }
            }

            if (blockPrefabName == null || ZNetScene.instance == null)
            {
                return;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(blockPrefabName);
            if (prefab == null)
            {
                return;
            }

            Vector3 normal = hit.normal;
            Vector3 position = Snap(hit.point + normal * 0.5f);
            if (!CanPlaceAt(player, position))
            {
                MineheimLog.Debug("Placement refused at " + position);
                return;
            }

            inventory.RemoveItem(consume, 1);
            // Prefabs are built inactive so their Awake waits for the world (BlockRegistry).
            ((GameObject)Object.Instantiate(prefab, position, Quaternion.identity)).SetActive(true);

            MineheimEvents.RaisePlaceBlock(new PlaceBlockEvent
            {
                Player = player,
                Position = position,
                Normal = normal,
                ItemType = BlockItemId(blockPrefabName),
                HotbarSlot = slot,
            });
        }

        /// <summary>
        /// A Minecraft block may be placed at Position only if (PROTOCOL.md "Placement rules"):
        /// 1. In world bounds. 2. No existing ZDO at that position. 3. Adjacent to terrain
        /// or another placed block. 4. Does not intersect the player capsule. 5. Does not
        /// intersect any active character or creature. Rules 4 and 5 share one check since
        /// the player is a Character too. Rule 2 is approximated by colliders - ZDOs
        /// without colliders are invisible to it (TODO(spec)).
        /// </summary>
        public static bool CanPlaceAt(Player player, Vector3 position)
        {
            if (Mathf.Abs(position.x) > 10000f || Mathf.Abs(position.z) > 10000f)
            {
                return false;
            }
            // Only the cell core blocks placement: small flora and debris clipping the cell
            // edges is tolerated, while solid walls and stacked blocks still occupy the core.
            // The player/creature capsule check below stays the wide one.
            if (Physics.OverlapSphere(position, 0.15f).Length > 0)
            {
                return false;
            }
            bool supported = Physics.Raycast(position, Vector3.down, 1.0f);
            if (!supported)
            {
                Vector3[] neighbors = { Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
                foreach (var offset in neighbors)
                {
                    // 0.6 m reaches the neighboring cell's near face (0.5 m away).
                    if (Physics.CheckSphere(position + offset, 0.6f))
                    {
                        supported = true;
                        break;
                    }
                }
            }
            if (!supported)
            {
                return false;
            }
            foreach (var collider in Physics.OverlapSphere(position, 0.4f))
            {
                if (collider.GetComponentInParent<Character>() != null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Minecraft blocks snap to the 1 m grid (cell centers at .5).</summary>
        private static Vector3 Snap(Vector3 p)
        {
            return new Vector3(
                Mathf.Floor(p.x) + 0.5f,
                Mathf.Floor(p.y) + 0.5f,
                Mathf.Floor(p.z) + 0.5f);
        }

        private static int BlockItemId(string blockPrefabName)
        {
            switch (blockPrefabName)
            {
                case MinecraftStoneBlock.PrefabName: return MinecraftItemId.Stone;
                case MinecraftDirtBlock.PrefabName: return MinecraftItemId.Dirt;
                case MinecraftOakLogBlock.PrefabName: return MinecraftItemId.OakLog;
                case MinecraftOakPlanksBlock.PrefabName: return MinecraftItemId.OakPlanks;
                case MinecraftCopperOreBlock.PrefabName: return MinecraftItemId.CopperOre;
                case MinecraftIronOreBlock.PrefabName: return MinecraftItemId.IronOre;
                case MinecraftGoldOreBlock.PrefabName: return MinecraftItemId.GoldOre;
                case MinecraftDiamondOreBlock.PrefabName: return MinecraftItemId.DiamondOre;
                case MinecraftObsidianBlock.PrefabName: return MinecraftItemId.Obsidian;
                default: return MinecraftItemId.None;
            }
        }
    }

    /// <summary>M4 placement tick, beside the mining tick on Player.Update.</summary>
    [HarmonyPatch(typeof(Player), "Update")]
    internal static class PlayerPlacementPatch
    {
        private static void Postfix(Player __instance)
        {
            if (MineheimPlugin.IsMinecraftMode(__instance))
            {
                BlockPlacement.Tick(__instance);
            }
        }
    }
}
