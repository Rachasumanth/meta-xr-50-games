using UnityEngine;

namespace XRGames.Chapter4.DeepSpaceDish
{
    public static class DeepSpaceDishConfig
    {
        public const float OuterRadiusMm = 14.0f;
        public const float InnerRadiusMm = 13.0f;
        public const float WallThicknessMm = 1.0f;
        public const float MassG = 3.0f;
        public const float LockGainThreshold = 0.55f;
        public const float BreakGainThreshold = 0.35f;
        public const float BreakTimeoutSec = 0.80f;
        public const float LockRequiredSec = 1.50f;
        public const float SlewGuardDegS = 90.0f;
        public const float EscapeTimeoutSec = 25.0f;
        public const float ChainMultiplier = 1.30f;
    }
}
