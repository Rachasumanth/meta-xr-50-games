using UnityEngine;

namespace XRGames.Chapter4.BeyCapClash
{
    public static class BeyCapConfig
    {
        public const float OuterRadiusMm = 16.0f;
        public const float WallThicknessMm = 0.5f;
        public const float CapMassG = 2.0f;
        public const float StadiumRadiusMm = 75.0f;
        public const float FrictionDecelRadS2 = 0.44f;
        public const float TapImpulseMmSPerN = 30.0f;
        public const float RestitutionE = 0.55f;
        public const float DeadSpinThresholdRadS = 8.0f;
        public const float DeadSpinTimeoutSec = 0.50f;
        public const float MaxSpinRadS = 220.0f;
    }
}
