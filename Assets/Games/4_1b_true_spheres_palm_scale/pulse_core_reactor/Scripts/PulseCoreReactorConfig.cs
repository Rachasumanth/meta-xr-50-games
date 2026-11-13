namespace XRGames.Chapter4.PulseCoreReactor
{
    public static class PulseCoreReactorConfig
    {
        public const float UndeformedApertureMm = 64.0f;
        public const float MaxPenetrationMm = 6.0f;

        public const float ShieldMinRadiusMm = 40.0f;
        public const float ShieldMaxRadiusMm = 160.0f;
        public const float ShieldRampSpeedMmS = 600.0f;

        public const float BombDensityThreshold = 0.70f;
        public const float OverheatLockoutSec = 2.0f;
        public const float VentCooldownSec = 20.0f;
        public const float VentHeatReduction = 35.0f;
    }
}
