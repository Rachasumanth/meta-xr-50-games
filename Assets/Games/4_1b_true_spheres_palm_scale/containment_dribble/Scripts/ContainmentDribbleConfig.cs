namespace XRGames.Chapter4.ContainmentDribble
{
    public static class ContainmentDribbleConfig
    {
        public const float BallRadiusMm = 32.0f; // 64 mm diameter
        public const float MaxLaunchSpeedMS = 3.30f;
        public const float GravityMS2 = 9.81f;
        public const float RhythmWindowSec = 0.18f;
        public const float MaxTapHeightMm = 70.0f;
        public const float MinTapPeakPenetrationMm = 3.0f;
        public const float MaxTapContactDurationSec = 0.160f;
        public const float DoubleTapFusionWindowSec = 0.050f; // 50 ms window: 40 ms fuses, 60 ms separate
        public const float RunDurationSec = 60.0f;
        public const float FireflyCollectRadiusMm = 26.0f;
    }
}
