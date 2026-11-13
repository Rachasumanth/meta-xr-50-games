using UnityEngine;

namespace XRGames.Chapter4.TardigradePond
{
    public static class TardigradePondConfig
    {
        public const float OuterRadiusMm = 14.0f;
        public const float InnerRadiusMm = 13.0f;
        public const float WallThicknessMm = 1.0f;
        public const float WaveSpeedMmS = 90.0f;
        public const float WaveTransitTimeSec = 0.139f; // 139 ms
        public const float DampingTauSec = 0.71f;
        public const float AlgaeSaveRadiusMm = 4.2f;
        public const float MaxCreatureSpeedMmS = 26.0f;
        public const float MineMobilityFactor = 0.35f;
        public const float DropletTimeSec = 42.0f;
    }
}
