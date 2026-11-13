using UnityEngine;

namespace XRGames.Chapter4.ParatrooperBasket
{
    public static class ParatrooperBasketConfig
    {
        public const float OuterRadiusMm = 15.0f;
        public const float WallThicknessMm = 0.5f;
        public const float InnerRadiusMm = 14.5f;
        public const float NetRadiusMm = 13.0f;
        public const float CapMassG = 2.0f;
        public const float MaxCatchTiltDeg = 20.0f;
        public const float MinCatchSpeedMmS = -260.0f;
        public const float MaxCatchSpeedMmS = -120.0f;
        public const float SoftCatchThresholdMmS = -160.0f;
        public const float NetRestitution = 0.45f;
        public const float GroundMissZMm = 40.0f;
        public const float DropTimerSec = 300.0f; // 5 x 60s
    }
}
