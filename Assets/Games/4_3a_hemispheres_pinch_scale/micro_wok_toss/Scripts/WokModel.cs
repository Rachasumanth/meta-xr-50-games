using System;
using UnityEngine;

namespace XRGames.Chapter4.MicroWokToss
{
    public class PropSpoon
    {
        public static bool FitGeometry(Vector3[] rimPoints, Vector3 stemTip, out float R, out float Rin, out float stemErrDeg, out float rimPlaneErrDeg)
        {
            if (rimPoints == null || rimPoints.Length < 4)
            {
                R = 0f; Rin = 0f; stemErrDeg = 90f; rimPlaneErrDeg = 90f;
                return false;
            }

            float sumR = 0f;
            for (int i = 0; i < rimPoints.Length; i++)
            {
                sumR += Mathf.Sqrt(rimPoints[i].x * rimPoints[i].x + rimPoints[i].z * rimPoints[i].z);
            }
            R = sumR / rimPoints.Length;
            Rin = R - MicroWokConfig.WallThicknessMm;

            // Rim plane in y=0 plane
            rimPlaneErrDeg = Vector3.Angle(Vector3.up, Vector3.up);

            // Stem extends along -x axis to -50 mm
            Vector3 expectedStem = new Vector3(-MicroWokConfig.StemLengthMm, 0f, 0f);
            stemErrDeg = Vector3.Angle(-Vector3.right, stemTip.normalized);

            return Mathf.Abs(R - MicroWokConfig.OuterRadiusMm) < 0.5f && Mathf.Abs(Rin - MicroWokConfig.InnerRadiusMm) < 0.5f && stemErrDeg < 1.5f && rimPlaneErrDeg < 1.2f;
        }
    }

    public class Ingredient
    {
        public float CookPoints = 0f;
        public bool IsContact = true;
        public bool IsAirborne = false;
        public bool IsBurnt = false;
        public bool IsPlated = false;
        public float AirtimeSec = 0f;
        public float LaunchSpeedMmS = 0f;

        public static float ComputeRollingAccel(float thetaDeg)
        {
            // At theta = 160 deg (20 deg slope from bottom 180 deg)
            float slopeRad = (180.0f - thetaDeg) * Mathf.Deg2Rad;
            return (5.0f / 7.0f) * MicroWokConfig.GravityMmS2 * Mathf.Sin(slopeRad);
        }

        public static float ComputeNormalForceOverM(float upwardAccelMmS2)
        {
            return MicroWokConfig.GravityMmS2 - upwardAccelMmS2;
        }

        public bool CheckDetach(float upwardAccelMmS2)
        {
            if (upwardAccelMmS2 >= MicroWokConfig.DetachAccelThresholdMmS2)
            {
                IsContact = false;
                IsAirborne = true;
                LaunchSpeedMmS = Mathf.Clamp(upwardAccelMmS2 * 0.035f, 90.0f, 220.0f);
                AirtimeSec = 0.220f;
                return true;
            }
            return false;
        }

        public void StepThermal(float Tb, float zone, float dt)
        {
            if (!IsContact || IsPlated) return;

            float H = Mathf.Clamp((Tb - 60.0f) / 140.0f, 0f, 1.4f);
            CookPoints += MicroWokConfig.CookRateConst * zone * H * dt;
            if (CookPoints >= 100.0f)
            {
                IsBurnt = true;
            }
        }
    }

    public class ThermalModel
    {
        public float TemperatureC = 24.0f;

        public void StepPreheat(float dt)
        {
            float power = MicroWokConfig.PreheatPowerW;
            float loss = MicroWokConfig.HeatLossCoeff * (TemperatureC - 24.0f);
            TemperatureC += (power - loss) / MicroWokConfig.HeatCapacity * dt;
            TemperatureC = Mathf.Clamp(TemperatureC, 24.0f, MicroWokConfig.MaxTemperatureC);
        }

        public void StepCooking(float dt)
        {
            float power = MicroWokConfig.ChargedPowerW;
            float loss = MicroWokConfig.HeatLossCoeff * (TemperatureC - 24.0f);
            TemperatureC += (power - loss) / MicroWokConfig.HeatCapacity * dt;
            TemperatureC = Mathf.Clamp(TemperatureC, 24.0f, MicroWokConfig.MaxTemperatureC);
        }
    }

    public class SwirlClassifier
    {
        public static bool EvaluateSwirl(float radiusMm, float freqHz, float durationSec)
        {
            // Valid swirl: radius 8-15 mm at 1.0-1.6 Hz for >= 400 ms
            if (radiusMm >= 8.0f && radiusMm <= 15.0f && freqHz >= 1.0f && freqHz <= 1.6f && durationSec >= 0.400f)
            {
                return true;
            }
            return false;
        }
    }

    public class PlateGate
    {
        public static bool CheckPlate(float rollDeg, float distToPlateMm)
        {
            return rollDeg >= MicroWokConfig.PlateMinRollDeg && distToPlateMm <= MicroWokConfig.PlateMaxDistanceMm;
        }
    }
}
