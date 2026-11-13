namespace XRGames.Chapter4.ScarabCanyon
{
    public static class ScarabCanyonConfig
    {
        public const float GravityMmS2 = 9800.0f;
        public const float LateralScale = 4.1f; // s -> canyon lateral
        public const float GrooveHalfMm = 24.0f; // spill at |s| > 24 mm for 60 ms
        public const float SpillHoldTimeSec = 0.060f; // 60 ms

        public const float DeadzoneDeg = 1.0f;
        public const float StictionThresholdSpeed = 8.0f; // mm/s
        public const float BaseScrollSpeed = 100.0f; // mm/s
        public const float ReducedScrollMultiplier = 0.7f; // when glow = 0

        public const int MaxParticleBudget = 380;
    }
}
