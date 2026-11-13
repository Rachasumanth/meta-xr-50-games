using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SnappingTurtleBunker
{
    public class PropShell
    {
        public static bool FitHemisphere(Vector3[] rimPoints, out float R, out float rimPlaneTiltDeg)
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

            return Mathf.Abs(R - SnappingTurtleConfig.OuterRadiusMm) < 0.5f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class TiltDynamics
    {
        public float CurrentTiltDeg = 0f;
        public float ContactAzimuthDeg = 0f;

        public void Step(float forceN, float azimuthDeg, float dt)
        {
            ContactAzimuthDeg = azimuthDeg;
            float targetTilt = 0f;
            if (forceN > 0f)
            {
                targetTilt = Mathf.Min(SnappingTurtleConfig.MaxTiltDeg, (forceN / 0.8f) * SnappingTurtleConfig.Calibrated08NTiltDeg);
            }

            // 1st order lag with tau = 180 ms
            float alpha = 1.0f - Mathf.Exp(-dt / SnappingTurtleConfig.TiltTauSec);
            CurrentTiltDeg += (targetTilt - CurrentTiltDeg) * alpha;
            CurrentTiltDeg = Mathf.Clamp(CurrentTiltDeg, 0f, SnappingTurtleConfig.MaxTiltDeg);
        }
    }

    public class SectorClassifier
    {
        public static int ClassifySector(float azimuthDeg)
        {
            // Normalize to [0, 360)
            float norm = (azimuthDeg % 360f + 360f) % 360f;
            if (norm >= 315f || norm < 45f) return 0;   // North / Front
            if (norm >= 45f && norm < 135f) return 1;   // East / Right
            if (norm >= 135f && norm < 225f) return 2;  // South / Back
            return 3;                                   // West / Left
        }

        public static int GetOppositeSector(int sector)
        {
            return (sector + 2) % 4;
        }
    }

    public class ApexSealDetector
    {
        public float DwellTimeSec = 0f;
        public bool IsSealed = false;

        public void Step(float forceN, float dt)
        {
            if (forceN >= SnappingTurtleConfig.ApexSealMinForceN)
            {
                DwellTimeSec += dt;
                if (DwellTimeSec >= SnappingTurtleConfig.ApexSealMinDwellSec)
                {
                    IsSealed = true;
                }
            }
            else
            {
                DwellTimeSec = 0f;
                IsSealed = false;
            }
        }
    }

    public class BiteController
    {
        public static bool EvaluateBite(float tiltDeg, int raisedSector, int beetleSector, float beetleDistMm)
        {
            if (tiltDeg < 3.0f) return false;
            if (raisedSector != beetleSector) return false;
            return beetleDistMm <= 30.0f;
        }
    }

    public class ChainTracker
    {
        private List<float> biteTimes = new List<float>();
        public bool IsFrenzy = false;
        public float FrenzyTimer = 0f;

        public void RegisterBite(float timeNow)
        {
            biteTimes.Add(timeNow);
            // Check for 3 bites within 4s window
            biteTimes.RemoveAll(t => timeNow - t > SnappingTurtleConfig.SnapFrenzyWindowSec);
            if (biteTimes.Count >= 3)
            {
                IsFrenzy = true;
                FrenzyTimer = SnappingTurtleConfig.SnapFrenzyDurationSec;
            }
        }

        public void Step(float dt)
        {
            if (IsFrenzy)
            {
                FrenzyTimer -= dt;
                if (FrenzyTimer <= 0f)
                {
                    IsFrenzy = false;
                }
            }
        }
    }
}
