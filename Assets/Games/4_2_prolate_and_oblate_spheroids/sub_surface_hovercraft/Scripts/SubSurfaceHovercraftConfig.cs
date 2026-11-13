namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public static class SubSurfaceHovercraftConfig
    {
        public const float SemiAxisA = 31.0f; // mm
        public const float SemiAxisB = 31.0f; // mm
        public const float SemiAxisC = 19.0f; // mm
        public const float MotionGain = 3.0f;
        public const float MassKg = 0.095f;
        public const float StaticFrictionBreakawayN = 0.391f;
        public const float SkirtDragCoeff = 0.55f;
        public const float KineticFrictionCoeff = 0.42f;
        public const float HarpoonSpeedMmS = 800.0f;
        public const float HarpoonLifeSec = 0.42f;
        public const float HarpoonDragDecay = 0.40f;
        public const float DriftImpulseDeltaV = 0.21f; // m/s
        public const float DriftHeatCost = 26.0f;
        public const float DriftDurationSec = 0.45f;
        public const float OverheatLockoutSec = 3.0f;
        public const float ChannelDurationSec = 70.0f;
    }
}
