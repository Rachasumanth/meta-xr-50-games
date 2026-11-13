namespace XRGames.Chapter4.ChronoSphereScrub
{
    public static class ChronoSphereScrubConfig
    {
        public const float MarbleRadiusMm = 8.0f;
        public const float PinchMinSeparationMm = 15.0f;
        public const float PinchMaxSeparationMm = 23.0f;
        public const float PinchReleaseMm = 26.0f;

        public const float MsPerFullTurn = 1000.0f; // 1 turn (360 deg) = 1000 ms timeline
        public const float DegToMs = MsPerFullTurn / 360.0f;

        public const float TimelineMaxMs = 5000.0f;
        public const float TimelineMinMs = 0.0f;

        public const int PerfectBonusPoints = 300;
    }
}
