using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public static class BernoulliLevitatorConfig
    {
        public const float BallRadiusMm = 20.0f;
        public const float BallRadiusMinMm = 19.75f;
        public const float BallRadiusMaxMm = 20.25f;
        public const float BallRadiusMeters = 0.020f;
        public const float MinPinchDistanceMm = 26.0f;
        public const float MaxPinchDistanceMm = 32.0f;
        public const float MaxVelocity = 1.2f;
        public const float MaxOrbVelocity = 1.2f;
        public const float MaxOrbAcceleration = 12.0f;
        public const float SlipClampDegS = 720.0f;
        public const float MaxSlipSpeed = 720.0f;
        public const float ServoWn = 8.0f;
        public const float ServoZeta = 1.0f;
        public const float PerfectRingRadiusMm = 15.0f;
        public const float StandardRingRadiusMm = 45.0f;
    }

    public enum PinchState
    {
        Released,
        Pinched
    }
}
