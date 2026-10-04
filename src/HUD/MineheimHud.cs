using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// M6 HUD hub (PROTOCOL.md subsystem table: HUD renders only, reads all subsystems).
    /// Coordinates vanilla-HUD visibility so the Minecraft layer replaces it while active.
    /// </summary>
    public static class MineheimHud
    {
        private static bool _vanillaHealthHidden;

        /// <summary>Hook from MineheimPlugin.Awake.</summary>
        public static void Init()
        {
        }

        /// <summary>
        /// Hide Valheim's own health bar while the Minecraft hearts are shown, restore it
        /// when Mineheim mode is off (DESIGN.md #19: hearts are the Valheim HP display).
        /// </summary>
        public static void Tick(bool active)
        {
            Hud hud = Hud.instance;
            if (hud == null || hud.m_healthBarRoot == null)
            {
                return;
            }
            if (active && !_vanillaHealthHidden)
            {
                hud.m_healthBarRoot.gameObject.SetActive(false);
                _vanillaHealthHidden = true;
            }
            else if (!active && _vanillaHealthHidden)
            {
                hud.m_healthBarRoot.gameObject.SetActive(true);
                _vanillaHealthHidden = false;
            }
        }
    }
}
