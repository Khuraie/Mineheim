using System.Reflection;
using HarmonyLib;
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
        // assembly_valheim 1.0.16 keeps Destructible.m_destroyed private.
        private static readonly FieldInfo DestroyedField =
            AccessTools.Field(typeof(Destructible), "m_destroyed");

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
            destructible.m_spawnWhenDestroyed = null;
            if (DestroyedField != null)
            {
                DestroyedField.SetValue(destructible, true);
            }
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
                // Item lands in M4; until it exists the drop is lost rather than faked.
                MineheimLog.Debug("No Minecraft item prefab yet: " + prefabName);
                return;
            }

            Object.Instantiate(prefab, e.Position + Vector3.up * 0.3f, Quaternion.identity);
        }
    }
}
