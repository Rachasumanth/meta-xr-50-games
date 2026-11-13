using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class LockAccumulator
    {
        public float LockTimeSec { get; private set; }
        public bool PurgeTriggered { get; private set; }

        public void Reset()
        {
            LockTimeSec = 0f;
            PurgeTriggered = false;
        }

        public bool Step(float distanceMm, float dt)
        {
            PurgeTriggered = false;

            if (distanceMm < MicroPlanetScannerConfig.LockDistanceGateMm)
            {
                LockTimeSec = Mathf.Min(MicroPlanetScannerConfig.LockDurationSec, LockTimeSec + dt);
            }
            else
            {
                LockTimeSec = Mathf.Max(0f, LockTimeSec - MicroPlanetScannerConfig.LockDrainMultiplier * dt);
            }

            if (LockTimeSec >= MicroPlanetScannerConfig.LockDurationSec - 0.0001f)
            {
                PurgeTriggered = true;
                LockTimeSec = 0f; // reset on purge
                return true;
            }
            return false;
        }
    }
}
