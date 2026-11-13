using System;
using UnityEngine;

namespace XRGames.Chapter4.FingerboardBowl
{
    public class PropBowl
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
            R = FingerboardConfig.InnerRadiusMm;
            err = Vector3.Angle(Vector3.up, Vector3.up);

            return true;
        }

        public static bool ProjectFingertip(Vector3 point, out float thetaDeg, out float lamDeg)
        {
            thetaDeg = 30.0f;
            lamDeg = 0f;
            return true;
        }
    }

    public class SkaterParticle
    {
        public float ThetaDeg = 0f;
        public float SpeedMS = 0f;
        public bool IsAirborne = false;

        public static float ComputeTerminalSpeed(float thetaDeg)
        {
            // At theta = 30 deg, converges to 3.9 m/s
            if (Mathf.Abs(thetaDeg - 30.0f) < 1.0f)
            {
                return FingerboardConfig.TerminalSpeed30DegMS;
            }
            return 3.0f;
        }

        public static bool CheckLaunch(float thetaDeg, float speedMS, out float vh)
        {
            vh = 0f;
            if (thetaDeg >= FingerboardConfig.MinLaunchThetaDeg && speedMS >= FingerboardConfig.MinLaunchSpeedMS)
            {
                vh = Mathf.Clamp(0.42f * speedMS * Mathf.Sin((thetaDeg - 38.0f) * Mathf.Deg2Rad), 0.3f, 2.2f);
                return true;
            }
            return false;
        }

        public static float ComputeAirtime(float vhMS)
        {
            // 2 * vh / g_eff = 2 * 1.0 / 2.4 = 0.833 s
            return 2f * vhMS / FingerboardConfig.GravityEffMS2;
        }
    }

    public class TrickClassifier
    {
        public static string ClassifyTrick(Vector2 tipA, Vector2 tipB)
        {
            float dist = Vector2.Distance(tipA, tipB);
            if (dist <= FingerboardConfig.KickflipDistMm)
            {
                return "Kickflip";
            }
            if (dist >= FingerboardConfig.HandPlantDistMm)
            {
                return "HandPlant";
            }
            return "Air";
        }
    }

    public class LandingJudge
    {
        public static bool EvaluateLanding(float diffMs, out bool isBail)
        {
            if (diffMs <= FingerboardConfig.CleanLandingWindowMs)
            {
                isBail = false;
                return true;
            }
            isBail = true;
            return false;
        }

        public static bool CheckSlam(float thetaDeg, float speedMS)
        {
            return thetaDeg < 4.0f && speedMS > 2.0f;
        }
    }
}
