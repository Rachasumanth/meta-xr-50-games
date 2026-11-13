using UnityEngine;

namespace XRGames.Chapter4.PhantomShellGame
{
    public static class PhantomShellConfig
    {
        public const float OuterRadiusMm = 22.0f;
        public const float WallThicknessMm = 1.8f;
        public const float InnerRadiusMm = 20.2f;
        public const float ShellMassG = 4.0f;
        public const float FrictionDecelMmS2 = 2747.0f;
        public const float PatchBoundaryMm = 95.0f;
        public const float MaxSlideSpeedMmS = 220.0f;
        public const float SteadySlideGain = 0.90f;
        public const float SlideResponseTauSec = 0.090f; // 90 ms
        public const float LiftThresholdMm = 12.0f;
        public const float ReseatThresholdMm = 4.0f;
        public const float ExposureThresholdSec = 1.2f;
        public const float BlockThresholdMm = 36.0f;
        public const float NearMissThresholdMm = 48.0f;
        public const float FairySnatchRadiusMm = 16.0f;
        public const float HologramLagTauSec = 0.180f; // 180 ms
    }
}
