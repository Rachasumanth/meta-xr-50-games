namespace XRGames.Chapter4.GeyserVenting
{
    public static class GeyserVentingConfig
    {
        public const float OrangeRadiusMm = 38.0f; // 76 mm diameter
        public const float SingleHoldDurationSec = 0.30f; // 300 ms
        public const float TwoEffectorHoldDurationSec = 0.167f; // 167 ms
        public const float VentDecayRate = 4.0f; // 4x decay per second
        public const float LeakRate = 1.2f; // 1.2 per second pressure fall when no active vents
        public const float BaseFluxF0 = 2.4f;
        public const float VentAngleRefDeg = 18.0f;
        public const float SealPressureDrop = 4.0f;
        public const float CrackPressureRise = 2.0f;
        public const float MaxPressure = 100.0f;
        public const int MaxCracks = 3;
        public const float PhaseDurationSec = 40.0f;
        public const float TimeoutPenaltyPerVent = 18.0f;
        public const float ReachableCapAngleDeg = 78.0f;
    }
}
