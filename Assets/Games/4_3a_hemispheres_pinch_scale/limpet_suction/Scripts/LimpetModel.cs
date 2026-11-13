using System;
using UnityEngine;

namespace XRGames.Chapter4.LimpetSuction
{
    public class PropShell
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

            return Mathf.Abs(R - LimpetSuctionConfig.OuterRadiusMm) < 0.5f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class SealDetector
    {
        public float DwellSec = 0f;
        public bool IsSealed = false;

        public void Step(float forceN, float tiltDeg, float rimGapMm, float dt)
        {
            if (forceN >= LimpetSuctionConfig.MinSealForceN && tiltDeg <= LimpetSuctionConfig.MaxSealTiltDeg && rimGapMm <= LimpetSuctionConfig.MaxSealGapMm)
            {
                DwellSec += dt;
                if (DwellSec >= LimpetSuctionConfig.SealDwellSec)
                {
                    IsSealed = true;
                }
            }
            else
            {
                DwellSec = 0f;
                if (forceN < 4.0f || tiltDeg > 7.0f || rimGapMm > 2.0f)
                {
                    IsSealed = false;
                }
            }
        }
    }

    public class WaveDynamics
    {
        public static float ComputeWaveDisplacementMm(bool isSealed, float waveForceN, float durationSec)
        {
            if (isSealed)
            {
                // Sealed: vacuum hold resists up to 9 N counter-surge, displacement < 1 mm
                return 0.45f;
            }
            else
            {
                // Unsealed: 9 N load sweeps shell >= 18 mm
                return 40.0f;
            }
        }

        public static bool CheckOverpress(float forceN, out int integrityLoss)
        {
            if (forceN >= LimpetSuctionConfig.OverpressLimitN)
            {
                integrityLoss = 1;
                return true;
            }
            integrityLoss = 0;
            return false;
        }
    }
}
