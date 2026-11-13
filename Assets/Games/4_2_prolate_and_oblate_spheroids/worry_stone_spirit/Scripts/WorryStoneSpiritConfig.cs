namespace XRGames.Chapter4.WorryStoneSpirit
{
    public static class WorryStoneSpiritConfig
    {
        public const float SemiAxisA = 0.035f; // 35mm
        public const float SemiAxisB = 0.025f; // 25mm
        public const float SemiAxisC = 0.010f; // 10mm (oblate spheroid)
        public const float KwRemovalRate = 0.18f; // mm/s
        public const float NominalPressureP0 = 0.40f; // N
        public const float NominalSpeedV0 = 80.0f; // mm/s
        public const float MaxPressure = 0.55f; // N
        public const float MaxAshDepthMm = 1.20f; // mm
        public const float LayerThicknessMm = 0.20f; // mm
        public const float GeodesicRadiusMm = 6.5f; // mm
        public const float TideDurationSec = 50.0f; // s
    }
}
