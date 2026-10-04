using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// M6 hotbar (DESIGN.md #5, #20): Minecraft-style 9-slot bar over Valheim's 8 quick
    /// slots. Slot 9 stays dimmed per DESIGN.md #20 ("hide slot 9 in the UI"). Number-key
    /// selection keeps working through Valheim's own HotkeyBar; this class only renders.
    /// </summary>
    public static class Hotbar
    {
        private const int Cell = 44;
        private const int Gap = 4;
        private const int Slots = 9;
        private const int RealSlots = 8;

        private static Texture2D _slot;
        private static Texture2D _slotSel;
        private static HotkeyBar _bar;
        private static bool _barHidden;
        private static GUIStyle _countStyle;

        public static void Draw(bool active)
        {
            if (_bar == null)
            {
                Hud hud = Hud.instance;
                if (hud != null)
                {
                    _bar = hud.GetComponentInChildren<HotkeyBar>(true);
                }
            }
            if (!active)
            {
                if (_barHidden && _bar != null)
                {
                    _bar.gameObject.SetActive(true);
                    _barHidden = false;
                }
                return;
            }
            if (_bar != null && !_barHidden)
            {
                _bar.gameObject.SetActive(false);
                _barHidden = true;
            }
            EnsureAssets();
            List<ItemDrop.ItemData> items = null;
            int selected = 0;
            if (_bar != null)
            {
                Traverse traverse = Traverse.Create(_bar);
                items = traverse.Field("m_items").GetValue<List<ItemDrop.ItemData>>();
                selected = traverse.Field("m_selected").GetValue<int>();
            }
            float total = Slots * Cell + (Slots - 1) * Gap;
            float x0 = (Screen.width - total) / 2f;
            float y0 = Screen.height - Cell - 12f;
            for (int i = 0; i < Slots; i++)
            {
                float x = x0 + i * (Cell + Gap);
                bool isSel = i == selected && i < RealSlots;
                GUI.DrawTexture(new Rect(x, y0, Cell, Cell), isSel ? _slotSel : _slot);
                if (i >= RealSlots || items == null || i >= items.Count)
                {
                    continue;
                }
                ItemDrop.ItemData item = items[i];
                if (item == null)
                {
                    continue;
                }
                Sprite icon = item.GetIcon();
                if (icon == null || icon.texture == null)
                {
                    continue;
                }
                Rect tr = icon.textureRect;
                Texture tex = icon.texture;
                var uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
                GUI.DrawTextureWithTexCoords(new Rect(x + 6, y0 + 6, Cell - 12, Cell - 12), tex, uv);
                if (item.m_stack > 1)
                {
                    GUI.Label(new Rect(x + 18, y0 + 16, Cell - 22, 24), item.m_stack.ToString(), _countStyle);
                }
            }
        }

        private static void EnsureAssets()
        {
            if (_slot != null)
            {
                return;
            }
            _slot = Frame(new Color32(200, 200, 200, 255), new Color32(25, 25, 25, 190), 2);
            _slotSel = Frame(new Color32(255, 255, 255, 255), new Color32(25, 25, 25, 190), 3);
            _countStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = 15,
                normal = { textColor = Color.white },
            };
        }

        private static Texture2D Frame(Color32 border, Color32 fill, int edge)
        {
            var tex = new Texture2D(Cell, Cell, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int y = 0; y < Cell; y++)
            {
                for (int x = 0; x < Cell; x++)
                {
                    bool isBorder = x < edge || y < edge || x >= Cell - edge || y >= Cell - edge;
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
