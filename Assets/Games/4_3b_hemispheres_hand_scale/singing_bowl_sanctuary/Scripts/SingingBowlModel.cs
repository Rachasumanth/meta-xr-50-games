using System;
using UnityEngine;

namespace XRGames.Chapter4.SingingBowlSanctuary
{
    public class PropBowl
    {
        public static bool FitGeometry(Vector3[] rimPoints, out float R, out float a, out float rimRMS)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; a = 0f; rimRMS = 999f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            a = sumR / rimPoints.Length;
            R = SingingBowlConfig.InnerRadiusMm;
            rimRMS = Mathf.Abs(a - SingingBowlConfig.InnerRimRadiusMm);

            return Mathf.Abs(R - SingingBowlConfig.InnerRadiusMm) < 0.5f && Mathf.Abs(a - SingingBowlConfig.InnerRimRadiusMm) < 0.5f && rimRMS < 0.5f;
        }

        public static void ProjectTrace(Vector3 point, out float phiDeg)
        {
            phiDeg = Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
            if (phiDeg < 0f) phiDeg += 360f;
        }
    }

    public class ResonanceModel
    {
        public float AmplitudeA = 0f;
        public float SagTimerSec = 0f;
        public bool IsCollapsed = false;

        public static float ComputeGaussianGain(float speedMmS)
        {
            float e = (speedMmS - SingingBowlConfig.ResonantSpeedMmS) / SingingBowlConfig.ResonantSpeedMmS;
            return Mathf.Exp(-Mathf.Pow(e / 0.223f, 2f));
        }

        public void Step(float speedMmS, float dt)
        {
            float G = ComputeGaussianGain(speedMmS);
            AmplitudeA = Mathf.Clamp01(AmplitudeA + dt * (G - AmplitudeA) / SingingBowlConfig.ResonanceTauSec);

            if (AmplitudeA < 0.15f)
            {
                SagTimerSec += dt;
                if (SagTimerSec >= 1.20f)
                {
                    IsCollapsed = true;
                }
            }
            else
            {
                SagTimerSec = 0f;
            }
        }
    }

    public class Locust
    {
        public float RadiusMm = 0f;
        public bool IsAlive = true;

        public bool CheckRingHit(float ringRadiusMm)
        {
            if (!IsAlive) return false;
            if (Mathf.Abs(RadiusMm - ringRadiusMm) <= 6.0f)
            {
                IsAlive = false;
                return true;
            }
            return false;
        }
    }
}
