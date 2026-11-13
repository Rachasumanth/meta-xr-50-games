using System;
using UnityEngine;

namespace XRGames.Chapter4.EllipticalOrbit
{
    public class RollModel
    {
        public const float Mass = EllipticalOrbitConfig.MassKg; // 0.075 kg
        public const float A = EllipticalOrbitConfig.SemiAxisA * 1000f; // 22.5 mm
        public const float C = EllipticalOrbitConfig.SemiAxisC * 1000f; // 31.0 mm

        public float SpeedMmS = 0f;
        public float AlphaRad = 0f;
        public float TotalTravelMm = 0f;
        public float StallTimer = 0f;
        public bool IsStalled = false;

        public static float ComputeEffectiveRadiusMm(float psiRad)
        {
            float cosPsi = Mathf.Cos(psiRad);
            float sinPsi = Mathf.Sin(psiRad);
            float c2OverA = (C * C) / A; // 31^2 / 22.5 = 42.711 mm
            float denom = (cosPsi * cosPsi) / c2OverA + (sinPsi * sinPsi) / A;
            return 1.0f / denom;
        }

        public static float ComputeCurvatureMmInv(float alphaRad)
        {
            // kappa = 0.008 + 0.012 * sin(2*alpha)
            return 0.008f + 0.012f * Mathf.Sin(2.0f * alphaRad);
        }

        public void ApplyProgradeNudge(float impulseMNs)
        {
            // dv = J / m
            float impulseNs = impulseMNs * 0.001f;
            float dvMmS = (impulseNs / Mass) * 1000f;
            SpeedMmS += dvMmS;
        }

        public void ApplyFlankNudge(float impulseMNs, float leverMm)
        {
            // Changes alpha by 0.27 rad for 3 mN*s at 20mm
            float deltaAlpha = (impulseMNs / 3.0f) * (leverMm / 20.0f) * 0.27f;
            AlphaRad += deltaAlpha;
        }

        public void Step(float dt)
        {
            if (SpeedMmS > 0.1f)
            {
                // Deceleration giving 295 mm stop from 160 mm/s
                // a = v0^2 / (2*d) = 160^2 / (2 * 295) = 25600 / 590 = 43.39 mm/s^2
                float decel = 43.39f;
                float newSpeed = Mathf.Max(0f, SpeedMmS - decel * dt);
                TotalTravelMm += (SpeedMmS + newSpeed) * 0.5f * dt;
                SpeedMmS = newSpeed;
            }

            if (SpeedMmS < 25.0f)
            {
                StallTimer += dt;
                if (StallTimer >= 1.2f)
                {
                    IsStalled = true;
                }
            }
            else
            {
                StallTimer = 0f;
            }
        }
    }
}
