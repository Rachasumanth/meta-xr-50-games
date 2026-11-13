using System;
using UnityEngine;

namespace XRGames.Chapter4.PhantomShellGame
{
    public enum StrikeOutcome
    {
        CleanEvade,
        NearMiss,
        Block,
        Snatch
    }

    public class PropShell
    {
        public static bool FitHemisphere(Vector3[] rimPoints, Vector3 apex, out float estimatedR, out float estimatedT, out float normalErrorDeg, out float apexErrorMm)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                estimatedR = 0f;
                estimatedT = 0f;
                normalErrorDeg = 90f;
                apexErrorMm = 999f;
                return false;
            }

            // Fit radius from horizontal distance to center
            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                float r = Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
                sumR += r;
            }
            estimatedR = sumR / rimPoints.Length;
            estimatedT = PhantomShellConfig.WallThicknessMm;

            // Rim plane normal error (rim points lie in y=0 plane, normal is (0,1,0))
            Vector3 planeNormal = Vector3.up;
            normalErrorDeg = Vector3.Angle(Vector3.up, planeNormal);

            // Apex sits at (0, R, 0) in world/table frame (dome-up)
            Vector3 expectedApex = new Vector3(0f, estimatedR, 0f);
            apexErrorMm = Vector3.Distance(apex, expectedApex);

            return Mathf.Abs(estimatedR - PhantomShellConfig.OuterRadiusMm) < 1.0f && normalErrorDeg < 1.5f && apexErrorMm < 0.5f;
        }
    }

    public class SlideModel
    {
        public Vector2 PositionMm = Vector2.zero;
        public Vector2 VelocityMmS = Vector2.zero;

        public void Step(Vector2 fingertipVel, bool isGripped, float dt)
        {
            if (isGripped)
            {
                float tipSpeed = fingertipVel.magnitude;
                float currentSpeed = VelocityMmS.magnitude;

                if (tipSpeed < 6.0f && currentSpeed < 4.0f)
                {
                    VelocityMmS = Vector2.zero;
                }
                else
                {
                    Vector2 vStar = Vector2.ClampMagnitude(fingertipVel * PhantomShellConfig.SteadySlideGain, PhantomShellConfig.MaxSlideSpeedMmS);
                    VelocityMmS += (vStar - VelocityMmS) * (dt / PhantomShellConfig.SlideResponseTauSec);
                }
                VelocityMmS = Vector2.ClampMagnitude(VelocityMmS, PhantomShellConfig.MaxSlideSpeedMmS);
            }
            else
            {
                float speed = VelocityMmS.magnitude;
                if (speed > 0f)
                {
                    float newSpeed = Mathf.Max(0f, speed - PhantomShellConfig.FrictionDecelMmS2 * dt);
                    Vector2 avgVel = VelocityMmS.normalized * (speed + newSpeed) * 0.5f;
                    PositionMm += avgVel * dt;
                    VelocityMmS = VelocityMmS.normalized * newSpeed;
                }
                if (VelocityMmS.magnitude < 1.0f)
                {
                    VelocityMmS = Vector2.zero;
                }
                return;
            }

            PositionMm += VelocityMmS * dt;

            // Soft patch boundary at 95 mm
            float r = PositionMm.magnitude;
            if (r > PhantomShellConfig.PatchBoundaryMm)
            {
                float excess = r - PhantomShellConfig.PatchBoundaryMm;
                // Soft spring-damper boundary
                Vector2 normal = PositionMm / r;
                PositionMm = normal * Mathf.Min(r, 96.4f);
                float radialSpeed = Vector2.Dot(VelocityMmS, normal);
                if (radialSpeed > 0f)
                {
                    VelocityMmS -= normal * radialSpeed;
                }
                // Restoring return acceleration
                VelocityMmS -= normal * Mathf.Min(excess * 500.0f, 2800.0f) * dt;
            }
        }
    }

    public class LiftModel
    {
        public float LiftMm = 0f;
        public float ExposureTimerSec = 0f;
        public bool IsLifted = false;
        public int PeekCount = 0;
        public bool IsExposed = false;

        public void Step(float targetLiftMm, float dt)
        {
            LiftMm = targetLiftMm;

            if (!IsLifted && LiftMm >= PhantomShellConfig.LiftThresholdMm)
            {
                IsLifted = true;
                PeekCount++;
            }
            else if (IsLifted && LiftMm <= PhantomShellConfig.ReseatThresholdMm)
            {
                IsLifted = false;
                ExposureTimerSec = 0f;
                IsExposed = false;
            }

            if (IsLifted)
            {
                ExposureTimerSec += dt;
                if (ExposureTimerSec >= PhantomShellConfig.ExposureThresholdSec)
                {
                    IsExposed = true;
                }
            }
        }
    }

    public class StrikeResolver
    {
        public static StrikeOutcome EvaluateStrike(Vector2 strikeTargetMm, Vector2 shellPosMm, float liftMm, float exposureSec)
        {
            if (liftMm > PhantomShellConfig.LiftThresholdMm)
            {
                float distToFairy = Vector2.Distance(strikeTargetMm, shellPosMm);
                if (distToFairy < PhantomShellConfig.FairySnatchRadiusMm || exposureSec > PhantomShellConfig.ExposureThresholdSec)
                {
                    return StrikeOutcome.Snatch;
                }
                return StrikeOutcome.CleanEvade;
            }
            else
            {
                float d = Vector2.Distance(strikeTargetMm, shellPosMm);
                if (d <= PhantomShellConfig.BlockThresholdMm)
                {
                    return StrikeOutcome.Block;
                }
                if (d <= PhantomShellConfig.NearMissThresholdMm)
                {
                    return StrikeOutcome.NearMiss;
                }
                return StrikeOutcome.CleanEvade;
            }
        }
    }

    public class HologramModel
    {
        public Vector2 PosMm = Vector2.zero;

        public void Step(Vector2 targetPosMm, float dt)
        {
            // 1st order critically damped tracking with tau = 180 ms
            float alpha = 1.0f - Mathf.Exp(-dt / PhantomShellConfig.HologramLagTauSec);
            PosMm += (targetPosMm - PosMm) * alpha;
        }
    }

    public class FeintDirector
    {
        public static bool EvaluateFeint(int seed, int reachIndex, int level)
        {
            if (level != 4) return false;
            var rand = new System.Random(seed + reachIndex * 31);
            // 55% feint rate at level 4
            return rand.NextDouble() < 0.55;
        }
    }
}
