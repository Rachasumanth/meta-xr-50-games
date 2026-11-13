using System;
using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public static class ClamPearlGuardConfig
    {
        // ---- Pearl geometry (spec item 1: r in [8.38, 8.62] mm, RMS < 0.35) ----
        public const float PearlRadiusMin = 8.38f;
        public const float PearlRadiusMax = 8.62f;
        public const float PearlRadiusTarget = 8.5f;
        public const float SphereFitRmsMax = 0.35f;
        public const float OffsetStabilityLimitMm = 0.5f;

        // ---- Palm lock (spec item 2) ----
        public const float PalmPoseErrorMaxMm = 0.6f;
        public const int RenderedClamLagMaxFrames = 2;

        // ---- Fist curl -> openness law (spec item 3: O = 1 at <=20 deg, 0.99 at 21,
        //      0.50 at 70, 0 at >=121; linear ramp across [20, 120]) ----
        public const float OpennessMinCurlDeg = 20.0f;
        public const float OpennessMaxCurlDeg = 120.0f;

        // ---- Cage thresholds (spec item 6: cage at 34mm, no cage at 36mm,
        //      release only above 42mm) ----
        public const float CageCloseThresholdMm = 35.0f;
        public const float CageOpenThresholdMm = 42.0f;
        public const float CageClosureDebounceMs = 100.0f;
        public const float CageClosureTimeMsMax = 120.0f; // fast-bonus window (item 7)

        // ---- Sun charging (spec item 4: 12/s, charge 60 at 5 s, full bloom 8.33 s;
        //      item 5: tilt focus F = 1.00 / 1.17 / 0.80 at 0 / 10 / 30 deg) ----
        public const float SunChargeRatePerSec = 12.0f;
        public const float ChargeFullUnits = 100.0f;
        public const float SunFullBloomSeconds = 8.33f; // ChargeFullUnits / rate
        public const float TiltFocusNeutral = 1.00f;
        public const float TiltFocus10Deg = 1.17f;
        public const float TiltFocus30Deg = 0.80f;

        // ---- Shift / drop (spec item 11: 7mm shift re-solve, 12mm drop in 150 ms) ----
        public const float ShiftToleranceMm = 7.0f;
        public const float DropThresholdMm = 12.0f;
        public const float DropDetectionTimeMsMax = 150.0f;

        // ---- Barracuda bite timing (spec item 8: 610/700/800 ms -> damage 0/11/22) ----
        public const float StrikeTimeEarlyMs = 620.0f;
        public const float StrikeTimeLateMs = 740.0f;
        public const int DamageEarlyBlock = 11;
        public const int DamageBitten = 22;
        public const int InitialIntegrity = 100;

        // ---- Scoring ----
        public const int ScoreFastBonusPoints = 150;
        public const int ScoreNormalBlockPoints = 60;
        public const int ScoreEarlyBlockPoints = 150;
    }
}
