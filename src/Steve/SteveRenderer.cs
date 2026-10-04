using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// Builds Steve's boxes. A 6-box rig (DESIGN.md #14): head, body, 2 arms, 2 legs at
    /// Minecraft proportions (1 px = 1/16 m). Every texture is our own 16x16 pixel art -
    /// CC0 by construction (PROJECT.md "Legal").
    /// </summary>
    public static class SteveRenderer
    {
        public static readonly Color32 Skin = new Color32(176, 134, 106, 255);
        public static readonly Color32 Shirt = new Color32(0, 170, 170, 255);
        public static readonly Color32 Pants = new Color32(60, 60, 200, 255);

        /// <summary>
        /// A limb pivot with a box mesh child. The child is offset so the pivot sits at
        /// the joint and rotation swings the limb correctly.
        /// </summary>
        public static GameObject Limb(string name, float w, float h, float d, Color32 color)
        {
            GameObject pivot = new GameObject(name);
            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(mesh.GetComponent<Collider>());
            mesh.name = name + "Mesh";
            mesh.transform.SetParent(pivot.transform, false);
            mesh.transform.localScale = new Vector3(w, h, d);
            mesh.GetComponent<MeshRenderer>().material = Flat(color);
            return pivot;
        }

        public static Material Flat(Color32 color)
        {
            // TODO(spec): confirm which shader survives Valheim's pipeline across GPUs.
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse");
            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        /// <summary>
        /// A thin face plate for the front of the head: our own 16x16 eyes-and-mouth on
        /// Steve's skin tone. The plate rides as a child of the head pivot.
        /// </summary>
        public static GameObject FacePlate()
        {
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(plate.GetComponent<Collider>());
            plate.name = "FacePlate";
            plate.transform.localScale = new Vector3(0.4f, 0.25f, 0.02f);
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    tex.SetPixel(x, y, Skin);
                }
            }
            // Eyes: white 4x2 blocks with blue pupils.
            for (int x = 2; x <= 5; x++) { tex.SetPixel(x, 7, Color.white); tex.SetPixel(x, 8, Color.white); }
            for (int x = 10; x <= 13; x++) { tex.SetPixel(x, 7, Color.white); tex.SetPixel(x, 8, Color.white); }
            for (int x = 3; x <= 4; x++) { tex.SetPixel(x, 7, new Color32(60, 60, 180, 255)); tex.SetPixel(x, 8, new Color32(60, 60, 180, 255)); }
            for (int x = 11; x <= 12; x++) { tex.SetPixel(x, 7, new Color32(60, 60, 180, 255)); tex.SetPixel(x, 8, new Color32(60, 60, 180, 255)); }
            // Mouth.
            for (int x = 6; x <= 9; x++) { tex.SetPixel(x, 4, new Color32(120, 70, 60, 255)); }
            tex.Apply();
            plate.GetComponent<MeshRenderer>().material = new Material(plate.GetComponent<MeshRenderer>().material.shader);
            plate.GetComponent<MeshRenderer>().material.mainTexture = tex;
            return plate;
        }
    }
}
