using System;
using UnityEngine;

namespace XRGames.Chapter4.MicroMortarAlchemist
{
    public class PropMortar
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

            return Mathf.Abs(R - MicroMortarConfig.OuterRadiusMm) < 0.5f && rimPlaneTiltDeg < 1.5f;
        }
    }

    public class Pestle
    {
        public static float ComputeForce(float penetrationMm)
        {
            // 1 mm penetration produces ~0.6 N
            return Mathf.Max(0f, penetrationMm * 0.60f);
        }
    }

    public class RhythmClock
    {
        public static float ScorePhase(float phase)
        {
            // phi = 0.0 scores 1.0; phi = 0.3 scores 0.35
            return Mathf.Abs(phase) < 0.1f ? 1.0f : 0.35f;
        }
    }

    public class Shard
    {
        public float Essence = 0f;
        public float Impurity = 0f;
        public bool IsDissolved = false;

        public void ApplyGrind(float qBeat, float forceN)
        {
            float forceFactor = Mathf.Clamp(forceN / 0.60f, 0.3f, 1.2f);
            if (qBeat >= 0.9f)
            {
                Essence += 7.0f * forceFactor;
            }
            else
            {
                Impurity += 3.0f;
            }

            if (Essence >= 100.0f)
            {
                IsDissolved = true;
            }
        }
    }

    public class PourController
    {
        public static float ComputeFlowRateGPerS(float tiltDeg)
        {
            if (tiltDeg <= MicroMortarConfig.MinPourTiltDeg)
            {
                return 0f;
            }
            return MicroMortarConfig.PourFlowMultiplier * Mathf.Sin(tiltDeg * Mathf.Deg2Rad);
        }

        public static bool EvaluateDose(float doseG, out bool spoiled)
        {
            if (doseG >= MicroMortarConfig.SpoilDoseG)
            {
                spoiled = true;
                return false;
            }
            spoiled = false;
            return Mathf.Abs(doseG - MicroMortarConfig.TargetDoseG) <= MicroMortarConfig.DoseToleranceG;
        }
    }
}
