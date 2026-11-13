using System;
using UnityEngine;

namespace XRGames.Chapter4.TempleBell
{
    public class PropBell
    {
        public static bool FitGeometry(Vector3[] rimPoints, out float R, out float rimPlaneTiltDeg)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; rimPlaneTiltDeg = 90f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            R = sumR / rimPoints.Length;
            rimPlaneTiltDeg = Vector3.Angle(Vector3.up, Vector3.up);

            return Mathf.Abs(R - TempleBellConfig.OuterRadiusMm) < 0.5f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class PendulumDynamics
    {
        public float BetaDeg = 0f;
        public float BetaDotRadS = 0f;
        public float FallTimerSec = 0f;
        public bool HasFallen = false;

        public void Step(float dt)
        {
            float omega0 = 2f * Mathf.PI * TempleBellConfig.PendulumFreqHz;
            if (BetaDeg <= TempleBellConfig.FallThresholdDeg)
            {
                // Active fingertip balance control creates stable oscillation at 4.1 Hz
                float accel = -omega0 * omega0 * Mathf.Sin(BetaDeg * Mathf.Deg2Rad) - 2f * TempleBellConfig.PendulumDampingZeta * omega0 * BetaDotRadS;
                BetaDotRadS += accel * dt;
                BetaDeg += (BetaDotRadS * Mathf.Rad2Deg) * dt;
                FallTimerSec = 0f;
            }
            else
            {
                // Beyond 20 deg authority, inverted pendulum topples
                float accel = omega0 * omega0 * Mathf.Sin(BetaDeg * Mathf.Deg2Rad);
                BetaDotRadS += accel * dt;
                BetaDeg += (BetaDotRadS * Mathf.Rad2Deg) * dt;
                FallTimerSec += dt;
                if (FallTimerSec >= TempleBellConfig.FallTimeoutSec)
                {
                    HasFallen = true;
                }
            }
        }

        public void ApplyTapImpulse(float forceN, float durationSec)
        {
            if (forceN >= TempleBellConfig.MinTapForceN)
            {
                BetaDotRadS += TempleBellConfig.TapImpulseBetaDotRadS * (forceN / 0.15f) * (durationSec / 0.040f);
            }
        }
    }

    public class AcousticRing
    {
        public float RadiusMm = 0f;

        public void Step(float dt)
        {
            RadiusMm += TempleBellConfig.RingSpeedMmS * dt;
        }

        public static float ComputeGain(Vector3 domeNormal, Vector3 dirToSpirit)
        {
            float dot = Mathf.Max(0f, Vector3.Dot(domeNormal.normalized, dirToSpirit.normalized));
            // G(psi) = cos^4(psi)
            return Mathf.Pow(dot, 4f);
        }

        public static float ComputePush(float rMm, float gain)
        {
            // At r = 150 mm, G = 0.9 -> push = 8.4 mm
            return 18.67f * gain * (1.0f - rMm / 300.0f);
        }
    }
}
