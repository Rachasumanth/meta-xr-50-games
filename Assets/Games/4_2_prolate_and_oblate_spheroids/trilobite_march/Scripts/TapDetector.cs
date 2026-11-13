using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.TrilobiteMarch
{
    public struct TapEvent
    {
        public char Finger; // 'I', 'M', 'R'
        public float Timestamp;
        public float Impulse;

        public TapEvent(char finger, float t, float j)
        {
            Finger = finger;
            Timestamp = t;
            Impulse = j;
        }
    }

    public class TapDetector
    {
        public static bool ClassifyTap(char finger, float durationSec, float impulse, float distFromZoneMm, out TapEvent tap)
        {
            tap = default;
            // 70 ms is a tap, 150 ms is not
            if (durationSec > 0.120f) return false;
            // 15 mm outside zone is rejected
            if (distFromZoneMm > 10.0f) return false;
            // Impulse in [0.015, 0.06]
            if (impulse < 0.015f || impulse > 0.06f) return false;

            tap = new TapEvent(finger, 0f, impulse);
            return true;
        }
    }

    public class GallopClassifier
    {
        public static bool EvaluateTriplet(TapEvent a, TapEvent b, TapEvent c, out float q, out bool isStumble)
        {
            q = 0f;
            isStumble = false;

            // Check timing intervals
            float dt1 = (b.Timestamp - a.Timestamp) * 1000f; // ms
            float dt2 = (c.Timestamp - b.Timestamp) * 1000f; // ms

            // Stumble check: 3 distinct fingers (e.g. I-M-R)
            if (a.Finger != b.Finger && b.Finger != c.Finger && a.Finger != c.Finger)
            {
                isStumble = true;
                return false;
            }

            // Valid pattern requires repeated pair + distinct third: (I-I-M) or (I-M-M)
            bool repeatedPair = (a.Finger == b.Finger && c.Finger != a.Finger) ||
                                (b.Finger == c.Finger && a.Finger != b.Finger);
            if (!repeatedPair)
            {
                isStumble = true;
                return false;
            }

            // Quality q
            float q1 = Mathf.Clamp01(1.0f - Mathf.Abs(dt1 - 90.0f) / 90.0f);
            float q2 = Mathf.Clamp01(1.0f - Mathf.Abs(dt2 - 90.0f) / 90.0f);
            q = (q1 + q2) * 0.5f;

            return true;
        }
    }
}
