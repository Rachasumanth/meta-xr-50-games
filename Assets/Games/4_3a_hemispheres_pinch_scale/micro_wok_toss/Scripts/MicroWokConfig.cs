using UnityEngine;

namespace XRGames.Chapter4.MicroWokToss
{
    public static class MicroWokConfig
    {
        public const float OuterRadiusMm = 18.0f;
        public const float InnerRadiusMm = 17.2f;
        public const float WallThicknessMm = 0.8f;
        public const float BowlMassG = 6.0f;
        public const float StemLengthMm = 50.0f;
        public const float GravityMmS2 = 9810.0f;
        public const float RollingAccelTheta160MmS2 = 2395.0f; // (5/7) * g * sin(20 deg)
        public const float DetachAccelThresholdMmS2 = 2500.0f;
        public const float PreheatPowerW = 34.5f;
        public const float ChargedPowerW = 18.0f;
        public const float HeatLossCoeff = 0.12f;
        public const float HeatCapacity = 1.2f;
        public const float MaxTemperatureC = 240.0f;
        public const float CookRateConst = 22.0f;
        public const float PlateMinRollDeg = 28.0f;
        public const float PlateMaxDistanceMm = 60.0f;
    }
}
