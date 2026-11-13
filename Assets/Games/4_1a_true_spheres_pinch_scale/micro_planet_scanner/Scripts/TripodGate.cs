using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public enum TripodState
    {
        Broken,
        Hold,
        Locked
    }

    public class TripodGate
    {
        public TripodState State { get; private set; } = TripodState.Broken;
        public bool IsDistanceValid { get; private set; }
        public bool IsAngleValid { get; private set; }

        public bool EvaluateDistance(float distanceMm)
        {
            if (!IsDistanceValid)
            {
                if (distanceMm <= MicroPlanetScannerConfig.TripodDistEnterMm)
                {
                    IsDistanceValid = true;
                }
            }
            else
            {
                if (distanceMm >= MicroPlanetScannerConfig.TripodDistReleaseMm)
                {
                    IsDistanceValid = false;
                }
            }
            return IsDistanceValid;
        }

        public bool EvaluateAngle(float minPairwiseAngleDeg)
        {
            if (!IsAngleValid)
            {
                if (minPairwiseAngleDeg >= MicroPlanetScannerConfig.TripodAngleEnterDeg)
                {
                    IsAngleValid = true;
                }
            }
            else
            {
                if (minPairwiseAngleDeg < MicroPlanetScannerConfig.TripodAngleReleaseDeg)
                {
                    IsAngleValid = false;
                }
            }
            return IsAngleValid;
        }

        public TripodState Step(float[] distancesMm, float minPairwiseAngleDeg, float minAgeSec)
        {
            if (minAgeSec < 0.030f)
            {
                State = TripodState.Hold;
                return State;
            }

            bool distOk = true;
            foreach (var d in distancesMm)
            {
                if (!EvaluateDistance(d)) distOk = false;
            }

            bool angOk = EvaluateAngle(minPairwiseAngleDeg);

            if (distOk && angOk)
            {
                State = TripodState.Locked;
            }
            else
            {
                State = TripodState.Broken;
            }
            return State;
        }
    }
}
