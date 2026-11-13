using UnityEngine;

namespace XRGames.Chapter4.LimpetSuction
{
    public static class LimpetSuctionConfig
    {
        public const float OuterRadiusMm = 22.0f;
        public const float WallThicknessMm = 1.8f;
        public const float InnerRadiusMm = 20.2f;
        public const float ShellMassG = 4.0f;
        public const float MaxSealTiltDeg = 4.0f;
        public const float MaxSealGapMm = 1.5f;
        public const float MinSealForceN = 6.0f;
        public const float SealDwellSec = 0.250f;
        public const float UnsealedMu = 0.30f;
        public const float SealedMu = 3.0f;
        public const float DislodgeThresholdMm = 18.0f;
        public const float OverpressLimitN = 12.0f;
        public const float RegrowthTimeSec = 125.0f;
    }
}
