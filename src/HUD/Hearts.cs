using UnityEngine;

namespace Mineheim
{
    /// <summary>M6 hearts (DESIGN.md #7, #19): 10 Minecraft-style hearts, filled by Valheim HP percentage.</summary>
    public static class Hearts
    {
        private static Texture2D _full;
        private static Texture2D _half;
        private static Texture2D _empty;

        private static readonly string[] Shape = new[]
        {
            "RRR...RRR",
            "RRRRRRRRR",
            "RRRRRRRRR",
            ".RRRRRRR.",
            "..RRRRR..",
            "...RRR...",
            "....R....",
            ".........",
            ".........",
        };

        private static readonly Color32 Red = new Color32(220, 20, 20, 255);
        private static readonly Color32 Dark = new Color32(45, 20, 20, 255);

        public static void Draw(Player player)
        {
            if (_full == null)
            {
                _full = Build(null);
                _half = Build(4);
                _empty = Build(-1);
            }
            float hearts = player.GetHealthPercentage() * 10f;
            const float size = 20f;
            const float gap = 2f;
            float y = Screen.height - 96f;
            for (int i = 0; i < 10; i++)
            {
                Texture2D tex = hearts >= i + 0.75f ? _full : (hearts >= i + 0.25f ? _half : _empty);
                GUI.DrawTexture(new Rect(16f + i * (size + gap), y, size, size), tex);
            }
        }

        private static Texture2D Build(int? redUntilCol)
        {
            int s = Shape[0].Length;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var clear = new Color32(0, 0, 0, 0);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    bool on = Shape[y][x] == 'R';
                    bool red = on && (!redUntilCol.HasValue || x <= redUntilCol.Value);
                    tex.SetPixel(x, s - 1 - y, !on ? clear : (red ? Red : Dark));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
