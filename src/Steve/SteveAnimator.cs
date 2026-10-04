using UnityEngine;

namespace Mineheim
{
    /// <summary>PROTOCOL.md "Steve contract" animation states.</summary>
    public enum SteveState
    {
        Idle,
        Walk,
        Sprint,
        Jump,
        Fall,
        Mine,
        Attack,
        Sneak,
    }

    /// <summary>
    /// Limb keyframes per state (DESIGN.md #15: eight small rotation sets on the limb
    /// transforms - Minecraft's animations are simple, don't overthink them).
    /// </summary>
    public static class SteveAnimator
    {
        public static void Animate(SteveController c, SteveState state, float speed, float dt)
        {
            float swing = 0f;
            if (state == SteveState.Walk || state == SteveState.Sprint)
            {
                c.WalkPhase += dt * (3f + speed * 1.4f);
                swing = Mathf.Sin(c.WalkPhase) * (state == SteveState.Sprint ? 0.9f : 0.7f);
            }

            switch (state)
            {
                case SteveState.Idle:
                    Pose(c, 0f, 0f, 0f, 0f, 0f);
                    break;
                case SteveState.Walk:
                    Pose(c, swing, -swing, -swing, swing, 0f);
                    break;
                case SteveState.Sprint:
                    Pose(c, swing, -swing, -swing, swing, 0.15f);
                    break;
                case SteveState.Jump:
                    Pose(c, -2.4f, -2.4f, 0.5f, -0.3f, 0f);
                    break;
                case SteveState.Fall:
                    Pose(c, 0f, 0f, 0.2f, -0.2f, 0f);
                    c.leftArm.localRotation = Quaternion.Euler(0f, 0f, 1.2f);
                    c.rightArm.localRotation = Quaternion.Euler(0f, 0f, -1.2f);
                    break;
                case SteveState.Mine:
                    Pose(c, -0.3f, 0f, 0f, 0f, 0.1f);
                    c.rightArm.localRotation = Quaternion.Euler(-1.2f + Mathf.Sin(Time.time * 10f) * 0.6f, 0f, 0f);
                    break;
                case SteveState.Attack:
                    Pose(c, -0.3f, 0f, 0f, 0f, 0.1f);
                    c.rightArm.localRotation = Quaternion.Euler(-2.0f + Mathf.Sin(Time.time * 14f) * 0.9f, 0f, 0f);
                    break;
                case SteveState.Sneak:
                    Pose(c, 0.3f, 0.3f, 0.3f, 0.3f, 0.4f);
                    break;
            }
        }

        private static void Pose(SteveController c, float armL, float armR, float legL, float legR, float lean)
        {
            c.leftArm.localRotation = Quaternion.Euler(armL, 0f, 0f);
            c.rightArm.localRotation = Quaternion.Euler(armR, 0f, 0f);
            c.leftLeg.localRotation = Quaternion.Euler(legL, 0f, 0f);
            c.rightLeg.localRotation = Quaternion.Euler(legR, 0f, 0f);
            c.body.localRotation = Quaternion.Euler(lean, 0f, 0f);
        }
    }
}
