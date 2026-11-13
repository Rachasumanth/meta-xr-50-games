namespace XRGames.Chapter4.CagePinball
{
    public static class CagePinballConfig
    {
        public const float BallRadiusMm = 20.0f;
        public const float BallRadiusMinMm = 19.75f;
        public const float BallRadiusMaxMm = 20.25f;
        public const float SphereFitRmsMaxMm = 0.5f;

        public const float CageRadiusMinMm = 60.0f;
        public const float CageRadiusMaxMm = 110.0f;

        public const float BumperMinSpeedMmS = 40.0f;
        public const float BumperCooldownSec = 0.120f; // 120 ms
        public const float ComboTimeoutSec = 0.800f; // 800 ms

        public const float PinballAimThresholdDeg = 9.5f; // breaks at <= 9, misses at 10

        public const float EscapeThresholdMm = 15.0f; // R + 15 mm
        public const float EscapeResetMm = 5.0f; // R + 5 mm
        public const float EscapeHoldTimeSec = 0.100f; // 100 ms
        public const float EscapeGraceDurationSec = 1.5f; // 1.5 s

        public const int MaxParticleBudget = 850;
    }
}
