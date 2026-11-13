using System;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public class ThumbKinematics
    {
        public const float Kf = 60.0f; // 60 N/m
        public const float MaxPressure = WorryStoneSpiritConfig.MaxPressure; // 0.55 N

        public float PressureN { get; private set; }
        public float HardnessH { get; private set; } = 1.0f;
        public float SFactor { get; private set; } = 1.0f;
        public float SExpirationTimer { get; private set; } = 0f;

        public float ComputePressure(float penetrationMm)
        {
            float deltaM = Mathf.Clamp(penetrationMm * 0.001f, 0.0013f, 0.0092f);
            PressureN = Mathf.Min(Kf * deltaM, MaxPressure);
            return PressureN;
        }

        public void ClassifyStroke(bool isOval, bool isLinearScrub, float durationSec)
        {
            if (isOval && durationSec >= 0.8f && durationSec <= 2.5f)
            {
                SFactor = 1.25f;
                SExpirationTimer = 2.0f;
            }
            else if (isLinearScrub)
            {
                SFactor = 0.85f;
                SExpirationTimer = 1.0f;
            }
        }

        public void Step(float dt, float speedMmS)
        {
            // Hardness evolution
            if (PressureN > 0.50f || speedMmS > 200.0f)
            {
                HardnessH = Mathf.Min(2.0f, HardnessH + 0.25f * dt);
            }
            else
            {
                HardnessH = Mathf.Max(1.0f, HardnessH - 0.10f * dt);
            }

            // SFactor expiration
            if (SExpirationTimer > 0f)
            {
                SExpirationTimer -= dt;
                if (SExpirationTimer <= 0f)
                {
                    SFactor = 1.0f;
                }
            }
        }
    }
}
