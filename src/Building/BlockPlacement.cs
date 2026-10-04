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
    /// The 5-rule CanPlaceAt validation lands in M4b.
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

            inventory.RemoveItem(consume, 1);
            Object.Instantiate(prefab, position, Quaternion.identity);

            MineheimEvents.RaisePlaceBlock(new PlaceBlockEvent
            {
                Player = player,
                Position = position,
                Normal = normal,
                ItemType = BlockItemId(blockPrefabName),
                HotbarSlot = slot,
            });
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
