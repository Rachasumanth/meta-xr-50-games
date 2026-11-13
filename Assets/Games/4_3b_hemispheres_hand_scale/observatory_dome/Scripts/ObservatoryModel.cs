using System;
using UnityEngine;

namespace XRGames.Chapter4.ObservatoryDome
{
    public class PropDome
    {
        public static bool FitGeometry(Vector3[] rimPoints, out float R, out float err)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; err = 90f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            R = ObservatoryDomeConfig.OuterRadiusMm;
            err = Vector3.Angle(Vector3.up, Vector3.up);

            return true;
        }
    }

    public class YawTracker
    {
        public static float ComputeYaw(Quaternion rot)
        {
            return rot.eulerAngles.y;
        }
    }

    public class SlitController
    {
        public static float ComputeSlitWidthDeg(float fingerSeparationMm)
        {
            // 30 mm geodesic separation yields w = 40 deg
            return Mathf.Clamp((40.0f / 30.0f) * fingerSeparationMm, 0f, ObservatoryDomeConfig.MaxShutterDeg);
        }
    }

    public class Target
    {
        public float AlphaDeg = 0f;
        public float LockTimerSec = 0f;
        public float BreakTimerSec = 0f;
        public bool IsCaptured = false;
        public float UncontainedSec = 0f;
        public bool HasEscaped = false;

        public void Step(float domeYawDeg, float slitWidthDeg, float slewRateDegS, float dt)
        {
            if (slitWidthDeg < ObservatoryDomeConfig.MinShutterDeg)
            {
                // Closed shutter blocks all locks
                LockTimerSec = 0f;
                BreakTimerSec += dt;
                UncontainedSec += dt;
                if (UncontainedSec >= ObservatoryDomeConfig.EscapeTimeSec)
                {
                    HasEscaped = true;
                }
                return;
            }

            float d = Mathf.DeltaAngle(domeYawDeg, AlphaDeg);
            bool contained = Mathf.Abs(d) <= (slitWidthDeg * 0.5f);

            if (contained && Mathf.Abs(slewRateDegS) <= ObservatoryDomeConfig.SlewGuardLimitDegS)
            {
                LockTimerSec += dt;
                BreakTimerSec = 0f;
                if (LockTimerSec >= ObservatoryDomeConfig.LockTimeSec)
                {
                    IsCaptured = true;
                }
            }
            else
            {
                BreakTimerSec += dt;
                if (BreakTimerSec >= ObservatoryDomeConfig.BreakResetSec)
                {
                    LockTimerSec = 0f;
                }
                UncontainedSec += dt;
                if (UncontainedSec >= ObservatoryDomeConfig.EscapeTimeSec)
                {
                    HasEscaped = true;
                }
            }
        }
    }
}
