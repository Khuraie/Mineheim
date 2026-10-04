using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// M3b Drops subsystem (PROTOCOL.md subsystem table: owns Valheim->Minecraft item
    /// mapping, reads block break events). Subscribes to OnBlockBreak and spawns the
    /// Minecraft item when one was routed. Vanilla drops stay untouched when the event's
    /// MinecraftDrop is null.
    /// </summary>
    public static class MinecraftDrops
    {
        /// <summary>Hook from MineheimPlugin.Awake.</summary>
        public static void Init()
        {
            MineheimEvents.OnBlockBreak += Handle;
        }

        /// <summary>
        /// Suppress Valheim's drop for a converted break (harness rule: Destructible
        /// .m_destroyed handling). The spawn source is cleared too, so the vanilla drop
        /// cannot fire through either path; the object itself is still destroyed normally.
        /// </summary>
        public static void SuppressVanillaDrop(Destructible destructible)
        {
            // The spawn source is cleared before the break; Destructible.Destroy skips
            // the spawn when it is null (verified 1.0.16 IL). The private m_destroyed
            // flag is deliberately NOT set: Destructible.RPC_Damage early-returns while
            // it is set, so setting it before the routed destroy lands would veto the
            // destroy itself and leave the block standing.
            destructible.m_spawnWhenDestroyed = null;
        }

        private static void Handle(BlockBreakEvent e)
        {
            if (e.MinecraftDrop == null || e.MinecraftDrop.Value <= MinecraftItemId.None)
            {
                return; // null = vanilla drop path; None = explicit no-drop.
            }

            string prefabName = MinecraftItemId.PrefabName(e.MinecraftDrop.Value);
            if (prefabName == null || ZNetScene.instance == null)
            {
                return;
            }

            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab == null)
            {
                MineheimLog.Debug("No Minecraft item prefab: " + prefabName);
                return;
            }

            // Prefabs are built inactive so their Awake waits for the world (BlockRegistry).
            ((GameObject)Object.Instantiate(prefab, e.Position + Vector3.up * 0.3f, Quaternion.identity)).SetActive(true);
        }
    }
}
