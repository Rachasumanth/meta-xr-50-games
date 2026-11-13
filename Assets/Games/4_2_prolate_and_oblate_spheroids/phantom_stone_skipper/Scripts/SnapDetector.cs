using System;
using UnityEngine;

namespace XRGames.Chapter4.PhantomStoneSkipper
{
    public class SnapDetector
    {
        public bool IsArmed = false;

        public bool CheckArmPinch(float separationMm)
        {
            IsArmed = (separationMm >= PhantomStoneSkipperConfig.MinPinchArmMm &&
                       separationMm <= PhantomStoneSkipperConfig.MaxPinchArmMm);
            return IsArmed;
        }

        public static bool CheckReleaseLaunch(bool armed, float separationMm, float timeSincePeakSec, out bool isDrySwing)
        {
            isDrySwing = false;
            if (!armed) return false;

            if (separationMm >= PhantomStoneSkipperConfig.ReleaseSeparationMm)
            {
                // Window [-40ms, 120ms]
                if (timeSincePeakSec >= -0.040f && timeSincePeakSec <= 0.120f)
                {
                    return true;
                }
                else if (timeSincePeakSec > 0.120f)
                {
                    isDrySwing = true;
                    return false;
                }
            }
            return false;
        }

        public static void ComputeInitialHop(float snapDistMm, float speedMPerS, float pitchDeg, float handYawRate,
                                             out float hopLengthMm, out float hopHeightMm, out float spinOmega, out bool isDiving, out bool isStall)
        {
            // S0 = 42.8 +- 1.5 mm at 30mm snap at 1.2 m/s
            hopLengthMm = 42.8f * (snapDistMm / 30.0f) * (speedMPerS / 1.2f);
            spinOmega = 12.0f + 3.4f * handYawRate;

            isDiving = false;
            isStall = false;

            if (pitchDeg < 6.0f)
            {
                isDiving = true;
                hopHeightMm = 2.0f;
            }
            else if (pitchDeg > 24.0f)
            {
                isStall = true;
                hopHeightMm = 6.0f;
                hopLengthMm *= 0.6f;
            }
            else
            {
                // Pitch 12 deg gives h1 = 16 mm
                hopHeightMm = 16.0f;
            }
        }
    }
}
