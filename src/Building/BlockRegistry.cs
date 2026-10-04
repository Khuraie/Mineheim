using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// M4 block registry (DESIGN.md #13: blocks are Valheim prefabs registered via Jotunn,
    /// custom 16x16-style textures on 3D cubes; DESIGN.md #12: every placed block is a ZDO
    /// and Valheim's save system handles persistence). Block prefabs carry ZNetView
    /// (m_persistent) and Destructible so M3 mining breaks them and routes drops.
    ///
    /// Textures are drawn procedurally in Minecraft's 16x16 style - no Iron Gate or Mojang
    /// assets (PROJECT.md "Legal").
    /// </summary>
    public static class BlockRegistry
    {
        /// <summary>Item display name -> block prefab name, used by BlockPlacement.</summary>
        public static readonly Dictionary<string, string> ItemNameToBlockPrefab = new Dictionary<string, string>();

        /// <summary>Hook from MineheimPlugin.Awake (M4a: one block; M4b: the starter set).</summary>
        public static void Init()
        {
            RegisterBlock(MinecraftStoneBlock.Create());
            RegisterItem(MinecraftStoneItem.Create(), MinecraftStoneBlock.PrefabName);
        }

        public static void RegisterBlock(GameObject prefab)
        {
            PrefabManager.Instance.AddPrefab(prefab);
            PrefabManager.Instance.RegisterToZNetScene(prefab);
        }

        public static void RegisterItem(GameObject prefab, string blockPrefabName)
        {
            ItemManager.Instance.AddItem(new CustomItem(prefab, false));
            PrefabManager.Instance.RegisterToZNetScene(prefab);
            if (blockPrefabName != null)
            {
                ItemNameToBlockPrefab[prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name] = blockPrefabName;
            }
        }

        /// <summary>1 m cube prefab with our texture, persistent ZDO, and minable health.</summary>
        public static GameObject BuildBlockPrefab(string prefabName, string display, Color32 baseColor, Color32 fleck)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = prefabName;
            go.GetComponent<MeshRenderer>().sharedMaterial = BuildMaterial(baseColor, fleck);

            ZNetView view = go.AddComponent<ZNetView>();
            view.m_persistent = true; // DESIGN.md #12: placed blocks persist as ZDOs

            Destructible destructible = go.AddComponent<Destructible>();
            destructible.m_health = 50f;
            destructible.m_minToolTier = 0;

            var piece = go.AddComponent<Piece>();
            piece.m_name = display;

            return go;
        }

        /// <summary>Item prefab carrying the block; its icon is the block's face texture.</summary>
        public static GameObject BuildItemPrefab(string prefabName, string display, string description, Color32 baseColor, Color32 fleck)
        {
            GameObject go = new GameObject(prefabName);
            var itemDrop = go.AddComponent<ItemDrop>();
            itemDrop.m_itemData = new ItemDrop.ItemData();
            itemDrop.m_itemData.m_shared.m_name = display;
            itemDrop.m_itemData.m_shared.m_description = description;
            itemDrop.m_itemData.m_shared.m_maxStackSize = 64; // Minecraft stack size
            itemDrop.m_itemData.m_shared.m_icons = new[] { BuildIcon(baseColor, fleck) };
            itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Material;

            go.AddComponent<Rigidbody>().useGravity = true;
            go.AddComponent<BoxCollider>().size = new Vector3(0.25f, 0.25f, 0.25f);

            return go;
        }

        public static Sprite BuildIcon(Color32 baseColor, Color32 fleck)
        {
            Texture2D tex = BuildTexture(baseColor, fleck);
            return Sprite.Create(tex, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
        }

        private static Material BuildMaterial(Color32 baseColor, Color32 fleck)
        {
            // TODO(spec): confirm which shader survives Valheim's pipeline across GPUs.
            Shader shader =
                Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse")
                ?? Shader.Find("Unlit/Texture");
            var mat = new Material(shader);
            mat.mainTexture = BuildTexture(baseColor, fleck);
            return mat;
        }

        /// <summary>Our own 16x16 pixel art: base with deterministic flecks (Minecraft style).</summary>
        private static Texture2D BuildTexture(Color32 baseColor, Color32 fleck)
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool flecked = ((x * 31 + y * 17 + x * y * 7) % 11) < 3;
                    tex.SetPixel(x, y, flecked ? fleck : baseColor);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
