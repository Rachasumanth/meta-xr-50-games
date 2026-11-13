using System;
using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public class SledBody
    {
        public const float Gravity = 9.81f;
        public const float RimLimitM = BobsledChuteConfig.RimLimitM; // 0.024 m
        public const float FlipLimitAccel = BobsledChuteConfig.FlipThresholdAccel; // 6.5 m/s^2

        public float VelocityMPerS = BobsledChuteConfig.InitialSpeedMPerS; // 0.55 m/s
        public float LateralPosM = 0f;
        public float LateralVelocity = 0f;
        public float ArcDistanceM = 0f;
        public float ElapsedTime = 0f;

        public int CrashCount = 0;
        public bool HasFlipped = false;
        public float FlipTimer = 0f;

        public event Action OnCrash;
        public event Action OnFinish;

        public void Reset()
        {
            VelocityMPerS = BobsledChuteConfig.InitialSpeedMPerS;
            LateralPosM = 0f;
            LateralVelocity = 0f;
            ArcDistanceM = 0f;
            ElapsedTime = 0f;
            CrashCount = 0;
            HasFlipped = false;
            FlipTimer = 0f;
        }

        public float Step(float dt, float aCtrl, float bankDeg, float curvature)
        {
            ElapsedTime += dt;

            // Longitudinal dynamics: v_dot = 1.025 - 0.196 - 0.12 * v + aCtrl
            float vDot = (1.025f - 0.196f) - 0.12f * VelocityMPerS + aCtrl;
            VelocityMPerS += vDot * dt;
            VelocityMPerS = Mathf.Max(0.15f, VelocityMPerS); // Floor at 0.15 m/s (no reverse)

            ArcDistanceM += VelocityMPerS * dt;

            // Lateral dynamics: y_ddot = v^2 * kappa - g * sin(phi_b) - c_lat * y_dot
            float bankRad = bankDeg * Mathf.Deg2Rad;
            float gBank = Gravity * Mathf.Sin(bankRad);
            float aCentrifugal = VelocityMPerS * VelocityMPerS * curvature;
            float netLateralAccel = aCentrifugal - gBank;

            // Flip detection
            if (Mathf.Abs(netLateralAccel) > FlipLimitAccel)
            {
                FlipTimer += dt;
                if (FlipTimer >= 0.078f && !HasFlipped) // 80 ms
                {
                    HasFlipped = true;
                    CrashCount++;
                    OnCrash?.Invoke();
                }
            }
            else
            {
                FlipTimer = 0f;
            }

            float yDdot = netLateralAccel - BobsledChuteConfig.LateralDampingClat * LateralVelocity;
            LateralVelocity += yDdot * dt;
            LateralPosM += LateralVelocity * dt;

            // Rim collision
            if (Mathf.Abs(LateralPosM) >= RimLimitM && Mathf.Abs(netLateralAccel) <= FlipLimitAccel)
            {
                LateralPosM = Mathf.Sign(LateralPosM) * RimLimitM;
                VelocityMPerS *= 0.65f; // speed x 0.65
                ElapsedTime += 0.25f;  // +0.25s penalty
                LateralVelocity = 0f;
                CrashCount++;
                OnCrash?.Invoke();
            }

            if (ArcDistanceM >= BobsledChuteConfig.ChuteLengthM)
            {
                OnFinish?.Invoke();
            }

            return netLateralAccel;
        }
    }
}
