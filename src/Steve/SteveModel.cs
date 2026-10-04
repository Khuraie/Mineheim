using HarmonyLib;
using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// Steve's rig controller (PROTOCOL.md "Steve contract"): the fields are the limb
    /// pivots. Facing follows the camera yaw (DESIGN.md #16: full 360, no flip).
    /// </summary>
    public class SteveController : MonoBehaviour
    {
        public Transform head;
        public Transform body;
        public Transform leftArm, rightArm;
        public Transform leftLeg, rightLeg;

        public SteveState State = SteveState.Idle;
        internal float WalkPhase;

        public void SetFacing(float yaw)
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public void SetAnimation(SteveState state)
        {
            State = state;
        }
    }

    /// <summary>
    /// Builds the 6-box Steve rig at Minecraft proportions (DESIGN.md #14).
    /// Layout in meters (1 px = 1/16 m): legs pivot at hip 0.75, body pivot at hip with
    /// arms and head riding it, head pivot at neck 1.5.
    /// </summary>
    public static class SteveModel
    {
        // Character.m_visual is private in assembly_valheim 1.0.16.
        private static readonly AccessTools.FieldRef<Character, GameObject> VisualRef =
            AccessTools.FieldRefAccess<Character, GameObject>("m_visual");

        public static SteveController Attach(Player player)
        {
            GameObject root = new GameObject("Steve");
            root.transform.SetParent(player.transform, false);
            root.transform.localPosition = Vector3.zero;
            var c = root.AddComponent<SteveController>();
            ResolveBaseShader(player);

            PlaceLimb(root.transform, SteveRenderer.Limb("LeftLeg", 0.25f, 0.75f, 0.25f, SteveRenderer.Pants), new Vector3(0.125f, 0.75f, 0f), -0.375f);
            PlaceLimb(root.transform, SteveRenderer.Limb("RightLeg", 0.25f, 0.75f, 0.25f, SteveRenderer.Pants), new Vector3(-0.125f, 0.75f, 0f), -0.375f);

            GameObject bodyPivot = new GameObject("BodyPivot");
            bodyPivot.transform.SetParent(root.transform, false);
            bodyPivot.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            GameObject bodyMesh = SteveRenderer.Limb("Body", 0.5f, 0.75f, 0.25f, SteveRenderer.Shirt);
            bodyMesh.transform.SetParent(bodyPivot.transform, false);
            bodyMesh.transform.localPosition = new Vector3(0f, 0.375f, 0f);
            c.body = bodyPivot.transform;

            c.leftArm = PlaceLimb(bodyPivot.transform, SteveRenderer.Limb("LeftArm", 0.25f, 0.75f, 0.25f, SteveRenderer.Skin), new Vector3(0.375f, 0.625f, 0f), -0.375f);
            c.rightArm = PlaceLimb(bodyPivot.transform, SteveRenderer.Limb("RightArm", 0.25f, 0.75f, 0.25f, SteveRenderer.Skin), new Vector3(-0.375f, 0.625f, 0f), -0.375f);

            GameObject headPivot = new GameObject("HeadPivot");
            headPivot.transform.SetParent(bodyPivot.transform, false);
            headPivot.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            GameObject headMesh = SteveRenderer.Limb("Head", 0.5f, 0.5f, 0.5f, SteveRenderer.Skin);
            headMesh.transform.SetParent(headPivot.transform, false);
            headMesh.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            GameObject face = SteveRenderer.FacePlate();
            face.transform.SetParent(headPivot.transform, false);
            face.transform.localPosition = new Vector3(0f, 0.31f, 0.255f);
            c.head = headPivot.transform;

            c.leftLeg = root.transform.Find("LeftLeg");
            c.rightLeg = root.transform.Find("RightLeg");
            return c;
        }

        private static Transform PlaceLimb(Transform parent, GameObject pivot, Vector3 pivotPos, float meshOffsetY)
        {
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = pivotPos;
            pivot.transform.GetChild(0).localPosition = new Vector3(0f, meshOffsetY, 0f);
            return pivot.transform;
        }

        private static void ResolveBaseShader(Player player)
        {
            if (SteveRenderer.HasBaseShader)
            {
                return;
            }
            Material template = null;
            var skinned = player.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (skinned != null)
            {
                template = skinned.sharedMaterial;
            }
            if (template == null)
            {
                var flat = player.GetComponentInChildren<MeshRenderer>(true);
                if (flat != null)
                {
                    template = flat.sharedMaterial;
                }
            }
            SteveRenderer.SetBaseShader(template != null ? template.shader : null);
            MineheimLog.Debug("Steve base shader: " + (template != null ? template.shader.name : "none (primitive fallback)"));
        }

        public static void HideVisual(Player player)
        {
            GameObject visual = VisualRef(player);
            if (visual != null)
            {
                visual.SetActive(false);
            }
        }

        public static void RestoreVisual(Player player)
        {
            GameObject visual = VisualRef(player);
            if (visual != null)
            {
                visual.SetActive(true);
            }
        }
    }

    /// <summary>
    /// M5 Steve wiring: while Mineheim mode is on for the local player, Steve replaces
    /// the Valheim model, follows the camera yaw, and animates from player state.
    /// DESIGN.md #17: Valheim armor visuals stay hidden while Steve is active.
    /// </summary>
    public static class SteveManager
    {
        private static GameObject _root;
        private static SteveController _controller;
        private static float _mineTimer;

        /// <summary>Hook from MineheimPlugin.Awake.</summary>
        public static void Init()
        {
            MineheimEvents.OnMineTick += _ => { _mineTimer = 0.4f; };
        }

        public static void Tick(Player player)
        {
            if (player == null || player != Player.m_localPlayer)
            {
                return;
            }

            if (!MineheimPlugin.IsMinecraftMode(player))
            {
                Detach(player);
                return;
            }

            if (_controller == null)
            {
                _controller = SteveModel.Attach(player);
                _root = _controller.gameObject;
                SteveModel.HideVisual(player);
            }

            _mineTimer -= Time.deltaTime;
            _controller.SetFacing(GameCamera.instance.transform.eulerAngles.y);
            SteveState state = DetectState(player);
            _controller.SetAnimation(state);
            SteveAnimator.Animate(_controller, state, player.GetVelocity().magnitude, Time.deltaTime);
        }

        private static SteveState DetectState(Player player)
        {
            if (player.InAttack())
            {
                return SteveState.Attack;
            }
            if (_mineTimer > 0f)
            {
                return SteveState.Mine;
            }
            if (player.IsSwimming())
            {
                return SteveState.Walk; // no swim pose in v0.1.0; avoids the arms-up Fall pose
            }
            if (!player.IsOnGround())
            {
                return player.GetVelocity().y > 1f ? SteveState.Jump : SteveState.Fall;
            }
            if (player.IsSneaking() || player.IsCrouching())
            {
                return SteveState.Sneak;
            }
            float speed = new Vector3(player.GetVelocity().x, 0f, player.GetVelocity().z).magnitude;
            if (speed > (MinecraftPhysics.WalkSpeedPerSecond + MinecraftPhysics.SprintSpeedPerSecond) * 0.5f)
            {
                return SteveState.Sprint;
            }
            return speed > 0.5f ? SteveState.Walk : SteveState.Idle;
        }

        private static void Detach(Player player)
        {
            if (_root == null)
            {
                return;
            }
            Object.Destroy(_root);
            _root = null;
            _controller = null;
            SteveModel.RestoreVisual(player);
        }
    }

    /// <summary>M5 tick, beside movement/mining/placement on Player.Update.</summary>
    [HarmonyPatch(typeof(Player), "Update")]
    internal static class PlayerStevePatch
    {
        private static void Postfix(Player __instance)
        {
            SteveManager.Tick(__instance);
        }
    }
}
