using System;
using UnityEngine;

namespace XRGames.Chapter4.GalaxyCauldron
{
    public class PropBowl
    {
        public static bool FitCap(Vector3[] rimPoints, out float R, out float d, out float rimRMS)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; d = 0f; rimRMS = 999f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            float rRim = sumR / rimPoints.Length;
            R = GalaxyCauldronConfig.InnerRadiusMm;
            d = GalaxyCauldronConfig.BowlDepthMm;
            rimRMS = Mathf.Abs(rRim - GalaxyCauldronConfig.InnerRimRadiusMm);

            return rimRMS < 0.5f;
        }

        public static void ProjectToCap(Vector3 point, out float discRadiusMm, out float thetaDeg)
        {
            float r = Mathf.Sqrt(point.x * point.x + point.y * point.y);
            if (point.z <= -54.9f)
            {
                discRadiusMm = 0f;
                thetaDeg = 0f;
            }
            else
            {
                discRadiusMm = GalaxyCauldronConfig.RimGeodesicDistMm;
                thetaDeg = GalaxyCauldronConfig.RimTruncationAngleDeg;
            }
        }
    }

    public class StirAnalyzer
    {
        public bool IsActive = false;

        public float ComputeStirOmega(Vector2 posMm, Vector2 velMmS)
        {
            // Clockwise is negative: - (x * vy - y * vx) / r^2
            float crossZ = -(posMm.x * velMmS.y - posMm.y * velMmS.x);
            float r2 = posMm.sqrMagnitude;
            return Mathf.Clamp(crossZ / Mathf.Max(r2, 400.0f), -4.5f, 4.5f);
        }

        public bool EvaluateSpeed(float speedMmS)
        {
            if (!IsActive)
            {
                if (speedMmS >= GalaxyCauldronConfig.ActiveStirSpeedMmS)
                {
                    IsActive = true;
                }
            }
            else
            {
                if (speedMmS < GalaxyCauldronConfig.HysteresisStirSpeedMmS)
                {
                    IsActive = false;
                }
            }
            return IsActive;
        }

        public static float ComputeFusionRadius(float hDepthMm)
        {
            // h = 10 -> 12.0 mm, h = 45 -> 15.0 mm
            float t = Mathf.Clamp01((hDepthMm - 10.0f) / 35.0f);
            return Mathf.Lerp(12.0f, 15.0f, t);
        }
    }

    public class Cloud
    {
        public Vector2 PosMm = Vector2.zero;
        public int Species = 0; // 0 = H, 1 = C
        public bool IsDead = false;

        public static bool CheckInteraction(Cloud a, Cloud b, out bool isFusion, out bool isNova)
        {
            isFusion = false;
            isNova = false;
            float dist = Vector2.Distance(a.PosMm, b.PosMm);

            if (a.Species == b.Species)
            {
                if (dist <= 13.0f)
                {
                    isFusion = true;
                    a.IsDead = true;
                    b.IsDead = true;
                    return true;
                }
            }
            else
            {
                if (dist <= 13.0f)
                {
                    isNova = true;
                    a.IsDead = true;
                    b.IsDead = true;
                    return true;
                }
            }
            return false;
        }
    }

    public class DarkMatterSeed
    {
        public float RhoMm = 0f;
        public bool IsComet = false;
        public bool IsEjected = false;

        public void Step(float vRhoMmS, float dt)
        {
            RhoMm += vRhoMmS * dt;
            if (RhoMm >= 86.0f)
            {
                IsComet = true;
            }
            if (RhoMm >= GalaxyCauldronConfig.InnerRimRadiusMm + 10.0f)
            {
                IsEjected = true;
            }
        }
    }
}
