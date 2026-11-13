using System;
using UnityEngine;

namespace XRGames.Chapter4.DeepSpaceDish
{
    public class PropDish
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

            return Mathf.Abs(R - DeepSpaceDishConfig.OuterRadiusMm) < 0.4f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class GainEvaluator
    {
        public static float ComputeGain(Vector3 dishAxis, Vector3 satDir)
        {
            float dot = Mathf.Max(0f, Vector3.Dot(dishAxis.normalized, satDir.normalized));
            // G(psi) = cos^4(psi)
            return Mathf.Pow(dot, 4f);
        }
    }

    public class Satellite
    {
        public float LockProgressSec = 0f;
        public float BelowThresholdSec = 0f;
        public bool IsCracked = false;
        public bool IsShattered = false;
        public float EscapeTimerSec = 0f;
        public bool HasEscaped = false;
        public float CrackCount = 0;

        public void StepLock(float gain, float slewRateDegS, float dt)
        {
            if (Mathf.Abs(slewRateDegS) > DeepSpaceDishConfig.SlewGuardDegS)
            {
                LockProgressSec = Mathf.Max(0f, LockProgressSec - 0.5f * dt);
                return;
            }

            if (gain >= DeepSpaceDishConfig.LockGainThreshold)
            {
                LockProgressSec += dt;
                BelowThresholdSec = 0f;
                if (!IsCracked && LockProgressSec >= DeepSpaceDishConfig.LockRequiredSec)
                {
                    IsCracked = true;
                    CrackCount++;
                }
            }
            else if (gain >= DeepSpaceDishConfig.BreakGainThreshold)
            {
                LockProgressSec = Mathf.Max(0f, LockProgressSec - 0.6f * dt);
                BelowThresholdSec += dt;
            }
            else
            {
                BelowThresholdSec += dt;
                if (BelowThresholdSec > DeepSpaceDishConfig.BreakTimeoutSec)
                {
                    LockProgressSec = 0f;
                }
            }
        }

        public void StepEscape(bool isActivelyLocked, float dt)
        {
            if (!isActivelyLocked && !IsCracked)
            {
                EscapeTimerSec += dt;
                if (EscapeTimerSec >= DeepSpaceDishConfig.EscapeTimeoutSec)
                {
                    HasEscaped = true;
                }
            }
        }
    }

    public class AimFilter
    {
        public Quaternion CurrentRot = Quaternion.identity;

        public void Step(Quaternion targetRot, float dt)
        {
            // Slerp aim filter to smooth tracking
            CurrentRot = Quaternion.Slerp(CurrentRot, targetRot, dt / 0.060f);
        }
    }
}
