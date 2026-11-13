using System;
using UnityEngine;

namespace XRGames.Chapter4.TrilobiteMarch
{
    public class GaitModel
    {
        public float DistanceMm = 0f;
        public float SpeedMmS = 0f;
        public float TargetSpeedMmS = 0f;
        public float SinkLevel = 0f;
        public float IdleTimer = 0f;
        public int ConsecutiveGallops = 0;
        public float StreakTimer = 0f;
        public bool IsStreakActive => StreakTimer > 0f;

        public void ApplyGallop(float cadenceHz, float qQuality)
        {
            DistanceMm += TrilobiteMarchConfig.StrideAdvanceMm; // exactly 24 mm
            ConsecutiveGallops++;
            IdleTimer = 0f;
            SinkLevel = Mathf.Max(0f, SinkLevel - 0.35f);

            float speed = TrilobiteMarchConfig.StrideAdvanceMm * cadenceHz * qQuality;
            if (ConsecutiveGallops >= 10)
            {
                StreakTimer = 4.0f; // x1.2 streak for 4.0s
            }
            if (IsStreakActive)
            {
                speed *= 1.2f;
            }
            TargetSpeedMmS = speed;
        }

        public void Step(float dt)
        {
            IdleTimer += dt;
            if (IdleTimer >= TrilobiteMarchConfig.SinkStartDelaySec)
            {
                SinkLevel = Mathf.Min(1.0f, SinkLevel + 0.20f * dt);
            }

            if (StreakTimer > 0f)
            {
                StreakTimer -= dt;
            }

            // Speed filter with tau = 0.25s: alpha = dt / (0.25 + dt)
            float alpha = dt / (0.25f + dt);
            SpeedMmS = Mathf.Lerp(SpeedMmS, TargetSpeedMmS, alpha);
        }
    }

    public class CurlModel
    {
        public bool IsCurled = false;
        public float HoldTimer = 0f;

        public bool UpdateContact(bool pad1, bool pad2, float dt)
        {
            if (pad1 && pad2)
            {
                HoldTimer += dt;
                if (HoldTimer >= 0.120f)
                {
                    IsCurled = true;
                }
            }
            else
            {
                HoldTimer = 0f;
                IsCurled = false;
            }
            return IsCurled;
        }

        public static bool CheckProtection(float overlapSec, float sweepDurationSec = 0.90f)
        {
            // 0.60 s overlap prevents plate loss; 0.55 s does not (requires >= 2/3 = 0.60s)
            return overlapSec >= 0.60f;
        }
    }
}
