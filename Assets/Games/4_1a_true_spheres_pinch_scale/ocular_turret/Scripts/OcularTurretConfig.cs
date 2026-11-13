namespace XRGames.Chapter4.OcularTurret
{
    public static class OcularTurretConfig
    {
        public const float MarbleRadiusMm = 8.0f;
        public const float MarbleCircumferenceMm = 50.26548f;
        public const float DegPerMmTravel = 360f / MarbleCircumferenceMm; // ~7.162 deg/mm

        public const float SlipClampSpeedMmS = 300.0f;

        public const float HeatAccumulationRate = 30.0f; // 100 in 3.33 s
        public const float HeatDissipationRate = 50.0f; // 100 in 2.0 s
        public const float BlinkHeatReduction = 20.0f;
        public const float BlinkCutDurationSec = 0.300f;

        public const float BurstCooldownSec = 8.0f;
        public const float BurstPetrifyAmount = 0.5f;

        public const int MaxParticleBudget = 400;
    }
}
