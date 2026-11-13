namespace XRGames.Chapter4.BobsledChute
{
    public static class BobsledChuteConfig
    {
        public const float SemiAxisA = 0.020f; // 20mm
        public const float SemiAxisB = 0.020f; // 20mm
        public const float SemiAxisC = 0.042f; // 42mm (prolate capsule)
        public const float SledMassKg = 0.220f;
        public const float ChuteLengthM = 0.900f; // 900mm
        public const float RimLimitM = 0.024f; // 24mm half-width
        public const float InitialSpeedMPerS = 0.55f; // 0.55 m/s
        public const float FlipThresholdAccel = 6.5f; // m/s^2
        public const float LateralDampingClat = 6.0f; // s^-1
        public const float IceFrictionMu = 0.02f;
        public const float DragCoeff = 0.12f; // s^-1
    }
}
