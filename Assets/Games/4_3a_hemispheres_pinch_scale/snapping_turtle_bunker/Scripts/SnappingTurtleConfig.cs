using UnityEngine;

namespace XRGames.Chapter4.SnappingTurtleBunker
{
    public static class SnappingTurtleConfig
    {
        public const float OuterRadiusMm = 22.0f;
        public const float WallThicknessMm = 1.8f;
        public const float InnerRadiusMm = 20.2f;
        public const float ShellMassG = 4.0f;
        public const float MaxTiltDeg = 8.0f;
        public const float Calibrated08NTiltDeg = 3.91f;
        public const float TiltTauSec = 0.180f;
        public const float ApexSealMinForceN = 0.35f;
        public const float ApexSealMinDwellSec = 0.150f;
        public const float BiteReachMm = 26.0f;
        public const float SnapFrenzyWindowSec = 4.0f;
        public const float SnapFrenzyDurationSec = 2.0f;
        public const float SnapFrenzySpeedMultiplier = 0.75f;
    }
}
