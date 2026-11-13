namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public static class FusionCoreStabilizerConfig
    {
        public const float CoreRadiusMm = 12.0f;

        // Pressure mapping: p = clamp((30 - d) / 8, 0, 1)
        public const float PalmDistMaxMm = 30.0f;
        public const float PalmDistMinMm = 22.0f;
        public const float PalmDistRangeMm = 8.0f;

        // Instability rates
        public const float GrowthRatePerSec = 0.55f;
        public const float DecayRatePerSec = 0.40f;
        public const float WobbleGrowthRatePerSec = 0.50f;

        // Grace & Limits
        public const float GraceDurationSec = 1.5f;
        public const float OneHandedPressureCap = 0.35f;
        public const float BandWidenFactor = 0.15f; // 15%
    }
}
