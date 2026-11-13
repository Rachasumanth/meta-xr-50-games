using System;
using UnityEngine;

namespace XRGames.Chapter4.PhantomStoneSkipper
{
    public enum TapRating
    {
        Perfect,
        Good,
        Miss
    }

    public class TapJudge
    {
        public const float AudioCorrectionSec = -0.022f; // -22 ms
        public int ConsecutiveMisses = 0;
        public bool IsSunken = false;
        public bool IsPerfectChain = true;

        public TapRating EvaluateTap(float tapOffsetFromTouchSec, out float energyRestore)
        {
            // Effective offset = tapOffset + AudioCorrectionSec
            // tap at t_touch has offset = -AudioCorrectionSec => effective = 0
            float effectiveOffsetSec = tapOffsetFromTouchSec;

            if (Mathf.Abs(effectiveOffsetSec) <= 0.040f) // within +- 40 ms
            {
                ConsecutiveMisses = 0;
                energyRestore = 0.92f;
                return TapRating.Perfect;
            }
            else if (effectiveOffsetSec <= 0.110f) // up to +80-110 ms
            {
                energyRestore = 0.75f;
                IsPerfectChain = false;
                return TapRating.Good;
            }
            else
            {
                ConsecutiveMisses++;
                IsPerfectChain = false;
                if (ConsecutiveMisses >= 2)
                {
                    IsSunken = true;
                }
                energyRestore = 0.45f;
                return TapRating.Miss;
            }
        }
    }

    public class PondModel
    {
        public static string EvaluateTouchdown(float distToPadCenterMm)
        {
            if (distToPadCenterMm <= 16.0f)
            {
                return "Gate";
            }
            else if (distToPadCenterMm <= 22.0f)
            {
                return "Graze";
            }
            else
            {
                return "OpenWater";
            }
        }
    }

    public class PhantomStone
    {
        public float HopLengthMm = 42.8f;
        public float HopHeightMm = 16.0f;
        public float ElapsedHopSec = 0f;
        public int HopIndex = 1;
        public bool IsFlying = false;

        public void Step(float dt, float energyFactor)
        {
            if (!IsFlying) return;

            ElapsedHopSec += dt;
            if (ElapsedHopSec >= PhantomStoneSkipperConfig.HopDurationSec)
            {
                ElapsedHopSec = 0f;
                HopIndex++;
                HopLengthMm *= energyFactor;
                HopHeightMm *= energyFactor;
            }
        }
    }
}
