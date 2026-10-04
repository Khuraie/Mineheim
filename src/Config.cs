using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// Log verbosity for Mineheim's own messages (config entry "Logging.LogLevel").
    /// </summary>
    public enum MineheimLogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Off = 4,
    }

    /// <summary>
    /// Mineheim configuration. M1 scope: toggle key and log level.
    /// PROTOCOL.md rules: config changes take effect on next world load unless marked
    /// live-reloadable. Both entries below are live-reloadable.
    /// </summary>
    public static class MineheimConfig
    {
        /// <summary>
        /// Key that toggles Mineheim mode. DESIGN.md #4: toggling only works while
        /// standing still and out of combat.
        /// </summary>
        public static ConfigEntry<KeyboardShortcut> ToggleKey;

        /// <summary>
        /// Minimum level a Mineheim message needs to reach the BepInEx console.
        /// </summary>
        public static ConfigEntry<MineheimLogLevel> LogLevel;

        public static void Bind(ConfigFile config)
        {
            ToggleKey = config.Bind(
                "Input",
                "ToggleKey",
                new KeyboardShortcut(KeyCode.F5),
                "Live-reloadable. Toggles Mineheim mode. DESIGN.md #4: only works while standing still and out of combat.");

            LogLevel = config.Bind(
                "Logging",
                "LogLevel",
                MineheimLogLevel.Info,
                "Live-reloadable. Minimum Mineheim log level printed to the BepInEx console.");
        }
    }

    /// <summary>
    /// Tiny log facade so every subsystem prints through the configured Mineheim log level.
    /// </summary>
    public static class MineheimLog
    {
        private static ManualLogSource _source;

        internal static void Init(ManualLogSource source)
        {
            _source = source;
        }

        public static void Debug(string message) => Write(MineheimLogLevel.Debug, message);

        public static void Info(string message) => Write(MineheimLogLevel.Info, message);

        public static void Warning(string message) => Write(MineheimLogLevel.Warning, message);

        public static void Error(string message) => Write(MineheimLogLevel.Error, message);

        private static void Write(MineheimLogLevel level, string message)
        {
            if (_source == null)
            {
                return;
            }

            if (MineheimConfig.LogLevel != null && (int)level < (int)MineheimConfig.LogLevel.Value)
            {
                return;
            }

            switch (level)
            {
                case MineheimLogLevel.Debug:
                    _source.LogDebug(message);
                    break;
                case MineheimLogLevel.Info:
                    _source.LogInfo(message);
                    break;
                case MineheimLogLevel.Warning:
                    _source.LogWarning(message);
                    break;
                case MineheimLogLevel.Error:
                    _source.LogError(message);
                    break;
            }
        }
    }
}
