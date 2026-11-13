using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler
{
    public class PressClassifier
    {
        public const int SectorCount = 24;
        public const float SectorWidthDeg = 15.0f; // 360 / 24

        public static int GetSector(float azimuthDeg)
        {
            float norm = (azimuthDeg % 360f + 360f) % 360f;
            return Mathf.FloorToInt(norm / SectorWidthDeg);
        }

        public static bool IsStableNearBoundary(float azimuthDeg, float boundaryDeg)
        {
            // Boundary hysteresis guard: 7.5 deg margin does not flicker
            float diff = Mathf.Abs(Mathf.DeltaAngle(azimuthDeg, boundaryDeg));
            return diff >= 7.5f;
        }
    }
}
