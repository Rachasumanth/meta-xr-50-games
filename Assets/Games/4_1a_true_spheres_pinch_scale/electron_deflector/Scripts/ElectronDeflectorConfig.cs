namespace XRGames.Chapter4.ElectronDeflector
{
    public static class ElectronDeflectorConfig
    {
        public const float BallRadiusMm = 20.0f;
        public const float PoleLatitudeMaxDeg = 35.0f; // |phi| <= 35 deg allowed

        public const int SectorCount = 12;
        public const float SectorArcDeg = 360f / SectorCount; // 30 deg each
        public const float SectorCooldownSec = 1.2f;

        public const float RingTubeRadiusMm = 15.0f; // within 15 mm of torus center
        public const float CoreAbsorbRadiusMm = 26.0f; // core absorbs at <= 26 mm

        public const float SurgeDurationSec = 3.0f;
        public const float SurgeBoostFactor = 0.15f; // 15% boost

        public const int MaxParticleBudget = 420;
    }
}
