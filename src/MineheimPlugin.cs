using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace Mineheim
{
    /// <summary>
    /// Mineheim plugin entry point. Mineheim is "Valheim with Minecraft flavor"
    /// (DESIGN.md): Valheim's physics, ZDO persistence, inventory, crafting, and
    /// progression stay authoritative and Mineheim layers Minecraft rules on top.
    ///
    /// PROTOCOL.md rules that hold for the whole plugin:
    /// - subsystems communicate via events only;
    /// - never write transform.position, only m_body.velocity (from M2a on);
    /// - patch via Harmony, never modify Valheim DLLs.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class MineheimPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.mineheim.plugin";
        public const string PluginName = "Mineheim";
        public const string PluginVersion = "0.1.0"; // keep in sync with build.txt

        private Harmony _harmony;

        private void Awake()
        {
            MineheimLog.Init(Logger);
            MineheimConfig.Bind(Config);

            // Harmony init. M1 ships no patches yet; M2a adds the Player.Update movement
            // patch described in PROTOCOL.md "Movement contract".
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Subsystem wiring (PROTOCOL.md: subsystems meet at events). Drops routes break
            // events; the block registry registers M4 content through Jotunn.
            MinecraftDrops.Init();
            BlockRegistry.Init();

            // M1 acceptance line. Printed unfiltered so the milestone checklist always
            // sees it regardless of the configured log level.
            Logger.LogInfo(PluginName + " v" + PluginVersion + " loaded");
        }

        /// <summary>
        /// Input polling. M1 scope: the mode toggle only. Kept on the plugin object so no
        /// gameplay subsystem owns input (PROTOCOL.md "Subsystem boundaries").
        /// </summary>
        private void Update()
        {
            if (MineheimConfig.ToggleKey == null || !MineheimConfig.ToggleKey.Value.IsDown())
            {
                return;
            }

            HandleToggle();
        }

        private void HandleToggle()
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                MineheimLog.Debug("Toggle ignored: no local player (not in a world yet).");
                return;
            }

            var state = MineheimPlayer.Get(player);
            if (!state.CanToggle(out var reason))
            {
                MineheimLog.Info(
                    "Mineheim toggle refused: " + reason + " (DESIGN.md #4). Mode stays "
                    + (state.MineheimMode ? "ON" : "OFF") + ".");
                return;
            }

            state.Toggle();
            MineheimLog.Info("Mineheim mode: " + (state.MineheimMode ? "ON" : "OFF"));
        }

        /// <summary>
        /// PROTOCOL.md mode accessor. Every Harmony patch gates on this before applying
        /// Minecraft rules to a player.
        /// </summary>
        public static bool IsMinecraftMode(Player player)
        {
            return MineheimPlayer.IsMinecraftMode(player);
        }

        private void OnDestroy()
        {
            if (_harmony == null)
            {
                return;
            }

            _harmony.UnpatchSelf();
            _harmony = null;
        }
    }
}
