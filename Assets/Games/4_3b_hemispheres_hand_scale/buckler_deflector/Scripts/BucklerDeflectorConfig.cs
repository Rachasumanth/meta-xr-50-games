using UnityEngine;

namespace XRGames.Chapter4.BucklerDeflector
{
    public static class BucklerDeflectorConfig
    {
        public const float OuterRadiusMm = 60.0f;
        public const float DepthMm = 45.0f;
        public const float InnerRadiusRInMm = 56.0f;
        public const float RimRadiusMm = 42.0f;
        public const float FootRingRadiusMm = 27.0f;
        public const float MaxCaptureRadiusMm = 50.0f;
        public const float CaptureConeDeg = 32.0f;
        public const int MaxCaptives = 5;
        public const float FlingMinDeltaMm = 18.0f;
        public const float FlingMinSpeedMS = 0.30f;
        public const float BeamConeDeg = 5.0f;
        public const float BeamRangeMm = 600.0f;
        public const float BodyHitRadiusMm = 120.0f;
        public const float OverloadDisableDurationSec = 2.0f;
    }
}
