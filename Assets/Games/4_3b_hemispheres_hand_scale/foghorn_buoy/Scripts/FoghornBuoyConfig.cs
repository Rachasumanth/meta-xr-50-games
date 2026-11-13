using UnityEngine;

namespace XRGames.Chapter4.FoghornBuoy
{
    public static class FoghornBuoyConfig
    {
        public const float RimRadiusMm = 80.0f;
        public const float CavityDepthMm = 60.0f;
        public const float FootRingRadiusMm = 36.0f;
        public const float MassKg = 0.400f;
        public const float DComM = 0.026f;
        public const float EffectiveInertia = 2.1e-3f; // kg*m^2
        public const float DampingCp = 0.020f; // N*m*s/rad
        public const float RockFrequencyHz = 0.55f;
        public const float WarningTiltDeg = 20.0f;
        public const float CapsizeTiltDeg = 28.0f;
        public const float MaxTiltCapDeg = 35.0f;
        public const float AirMax = 100.0f;
        public const float FoghornAirCost = 15.0f;
        public const float AirRegenPerSec = 8.0f;
        public const float SharkDangerRadiusMm = 40.0f;
        public const float FoghornRepelRadiusMm = 200.0f;
        public const float FoghornRepelDistanceMm = 120.0f;
    }
}
