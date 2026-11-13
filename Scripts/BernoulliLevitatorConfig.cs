using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public static class BernoulliLevitatorConfig
    {
        public const float BallRadiusMm = 20.0f;
        public const float BallMinRadiusMm = 19.75f;
        public const float BallMaxRadiusMm = 20.25f;
        public const float MaxRmsMm = 0.5f;

        public const float PinchEnterDistMm = 26.0f;
        public const float PinchReleaseDistMm = 32.0f;
        public const float PinchEnterStableMs = 30.0f;
        public const float PinchReleaseStableMs = 80.0f;

        public const float MinHeightMm = 0.0f;
        public const float MaxHeightMm = 300.0f;
        public const float DefaultTargetHeightMm = 120.0f;

        public const float MaxVelocityMps = 1.2f;
        public const float MaxAccelerationMps2 = 12.0f;
        public const float ServoWn = 8.0f;
        public const float ServoZeta = 1.0f;

        public const float MaxSlipAngularSpeedDeg = 720.0f;
        public const float RingPassDistanceMm = 45.0f;
        public const float RingPerfectDistanceMm = 15.0f;
        public const int PerfectBonusPoints = 50;

        public const float FreeFallDurationSec = 2.5f;
        public const float FreeFallToleranceSec = 0.2f;

        public const int ParticleBudget = 650;
        public const float AudioVolumeDefault = 1.0f;
    }
}
