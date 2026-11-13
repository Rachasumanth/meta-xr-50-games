namespace XRGames.Chapter4.VortexWobbler
{
    public static class VortexWobblerConfig
    {
        public const float SemiAxisA = 0.030f; // 30mm
        public const float SemiAxisB = 0.030f; // 30mm
        public const float SemiAxisC = 0.012f; // 12mm (oblate disc)
        public const float TauDecay = 1.40f; // s
        public const float MaxAmplitudeDeg = 26.0f; // deg
        public const float NominalMinAmp = 12.0f;
        public const float NominalMaxAmp = 20.0f;
        public const float OverdriveAmp = 22.0f;
        public const float CollapseAmp = 8.0f;
        public const float AirlockRadiusMm = 14.0f; // pods dock
        public const float BreachRadiusMm = 12.0f;  // debris breaches
    }
}
