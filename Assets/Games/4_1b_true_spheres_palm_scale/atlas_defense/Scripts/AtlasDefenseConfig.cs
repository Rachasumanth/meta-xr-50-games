namespace XRGames.Chapter4.AtlasDefense
{
    public static class AtlasDefenseConfig
    {
        public const float GlobeRadiusMm = 38.0f; // 76 mm diameter
        public const float GlobeRadiusMinMm = 37.8f;
        public const float GlobeRadiusMaxMm = 38.2f;
        public const float SphereFitRmsMaxMm = 0.5f;

        // Lock & Shield
        public const float LockAngleEnterDeg = 20.0f;
        public const float LockAngleReleaseDeg = 24.0f;
        public const float LockHoldDurationSec = 0.250f; // 250 ms
        public const float InterceptAngleCapDeg = 25.0f;

        // Kinematics & Offset
        public const float SaturationOmegaDegS = 720.0f;
        public const float PlanetOffsetClampMm = 15.0f;

        // Wave & Cities
        public const int TotalCities = 5;
        public const int MeteorsPerWave = 12;
        public const int MaxConcurrentMeteors = 4;
        public const int MaxParticleBudget = 500;
    }
}
