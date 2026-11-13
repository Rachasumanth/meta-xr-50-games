using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public static class MicroPlanetScannerConfig
    {
        // Physical prop
        public const float MarbleRadiusMm = 8.0f;
        public const float MarbleRadiusMeters = 0.008f;
        public const float MarbleRadiusMinMm = 7.88f;
        public const float MarbleRadiusMaxMm = 8.12f;
        public const float SphereFitRmsMaxMm = 0.35f;

        // Virtual Moon
        public const float MoonRadiusMm = 30.0f; // 60 mm diameter -> 30 mm radius
        public const float MoonScaleFactor = 3.75f; // 30 mm / 8 mm = 3.75x

        // Lock Accumulator
        public const float LockDistanceGateMm = 8.0f;
        public const float LockDurationSec = 0.400f; // 400 ms
        public const float LockDrainMultiplier = 1.5f;

        // Tripod Gate
        public const float TripodDistEnterMm = 9.0f;
        public const float TripodDistReleaseMm = 11.0f;
        public const float TripodAngleEnterDeg = 60.0f;
        public const float TripodAngleReleaseDeg = 55.0f;

        // Rolling & Slip
        public const float SlipLimitDegS = 540.0f;

        // Budget & Quota
        public const int MaxParticleBudget = 800;
        public const int TotalSurveyWaves = 5;
    }
}
