using UnityEngine;

namespace Mineheim
{
    /// <summary>M6 crosshair (DESIGN.md #18): Minecraft-style plus replacing Valheim's while active.</summary>
    public static class Crosshair
    {
        private static Texture2D _cross;
        private static bool _vanillaHidden;

        private static readonly string[] Glyph = new[]
        {
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "WWWWWWWWWWWWWWW",
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "......WWW......",
            "......WWW......",
        };

        public static void Draw(bool active)
        {
            Hud hud = Hud.instance;
            if (!active)
            {
                if (_vanillaHidden && hud != null)
                {
                    hud.m_crosshair.enabled = true;
                    hud.m_crosshairBow.enabled = true;
                    _vanillaHidden = false;
                }
                return;
            }
            if (hud != null && !_vanillaHidden)
            {
                hud.m_crosshair.enabled = false;
                hud.m_crosshairBow.enabled = false;
                _vanillaHidden = true;
            }
            if (_cross == null)
            {
                _cross = PixelArt(Glyph, new Color32(235, 235, 235, 255));
            }
            const float scale = 2f;
            float w = Glyph[0].Length * scale;
            float h = Glyph.Length * scale;
            GUI.DrawTexture(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), _cross);
        }

        internal static Texture2D PixelArt(string[] glyph, Color32 fg)
        {
            int w = glyph[0].Length;
            int h = glyph.Length;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, h - 1 - y, glyph[y][x] == '.' ? clear : fg);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
