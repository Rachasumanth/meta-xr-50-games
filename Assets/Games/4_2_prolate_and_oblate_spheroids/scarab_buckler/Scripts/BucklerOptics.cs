using System;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class BucklerOptics
    {
        public const float Rp = ScarabBucklerConfig.RpCurvatureMm; // 60 mm
        public const float ReflectionEnergy = 0.92f;

        public static Vector3 Reflect(Vector3 incidentDir, Vector3 surfaceNormal, out float energy)
        {
            energy = ReflectionEnergy;
            // r = d - 2(d . n)n
            return incidentDir - 2.0f * Vector3.Dot(incidentDir, surfaceNormal) * surfaceNormal;
        }

        public static bool CheckAperture(float phiDeg, out bool isRimScatter)
        {
            isRimScatter = false;
            if (phiDeg > 50.0f) return false; // misses optical cap

            if (phiDeg >= 40.0f)
            {
                isRimScatter = true;
            }
            return true;
        }
    }
}
