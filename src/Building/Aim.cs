using UnityEngine;

namespace Mineheim
{
    /// <summary>
    /// PROTOCOL.md "Aim contract": Valheim is third-person 3D, so aim is camera-driven.
    /// Every Minecraft system reads the camera forward vector; no mouse cursor. Reach is
    /// 4.5 meters (Minecraft default).
    /// </summary>
    public static class Aim
    {
        public const float Reach = 4.5f;

        public static Vector3 GetAim(Player player)
        {
            return GameCamera.instance.transform.forward;
        }

        public static bool Raycast(Player player, float reach, out RaycastHit hit)
        {
            var origin = player.GetEyePoint();
            var dir = GetAim(player);
            return Physics.Raycast(origin, dir, out hit, reach);
        }
    }
}
