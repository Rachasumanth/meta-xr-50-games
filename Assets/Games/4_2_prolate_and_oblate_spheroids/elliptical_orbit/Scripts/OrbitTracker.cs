using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.EllipticalOrbit
{
    public class TailSystem
    {
        public int CutsCount = 0;

        public bool CheckCut(float distanceMm, float crossingAngleDeg)
        {
            // Transverse crossing within 9mm cuts it; tangential (< 30 deg) does not
            if (distanceMm <= EllipticalOrbitConfig.TailCutDistMm && crossingAngleDeg >= 30.0f)
            {
                CutsCount++;
                return true;
            }
            return false;
        }
    }

    public class OrbitTracker
    {
        public float UnwrappedAngleDeg = 0f;
        public float StartPositionMm = 0f;

        public bool CheckClosure(float unwrappedDeg, float returnDistMm, out int bonusScore)
        {
            bonusScore = 0;
            // 300 deg unwrapped angle and return distance <= 24 mm scores +300
            if (unwrappedDeg >= 300.0f && returnDistMm <= 24.0f)
            {
                bonusScore = 300;
                return true;
            }
            return false;
        }
    }

    public class OrbitJudge
    {
        public static int ScoreEllipticity(float e)
        {
            // e = 0.35 scores +200 +- 5; e = 0.10 scores < 60; e = 0.60 scores < 60
            float diff = e - 0.35f;
            int score = Mathf.RoundToInt(200f - 2500f * (diff * diff));
            return Mathf.Max(0, score);
        }
    }
}
