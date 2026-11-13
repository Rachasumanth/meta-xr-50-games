using UnityEngine;

namespace XRGames.Chapter4.TempleBell
{
    public static class TempleBellConfig
    {
        public const float OuterRadiusMm = 15.0f;
        public const float WallThicknessMm = 0.5f;
        public const float CapMassG = 2.0f;
        public const float PendulumFreqHz = 4.1f;
        public const float PendulumDampingZeta = 0.14f;
        public const float TapImpulseBetaDotRadS = 0.40f;
        public const float FallThresholdDeg = 20.0f;
        public const float FallTimeoutSec = 0.120f;
        public const float RingSpeedMmS = 220.0f;
        public const float ResonantGainThreshold = 0.85f;
        public const float SpiritLossRadiusMm = 30.0f;
        public const float DebounceSec = 0.150f;
        public const float MinTapForceN = 0.08f;
    }
}
