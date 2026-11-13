using UnityEngine;

namespace XRGames.Chapter4.ContainmentDribble
{
    public enum Lane
    {
        Center,
        Left,
        Right
    }

    public class PropFoam
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public float RadiusMm { get; set; } = ContainmentDribbleConfig.BallRadiusMm;
    }

    public class TapClassifier
    {
        public static Lane SectorFromLongitude(float longitudeDeg)
        {
            float norm = Mathf.Repeat(longitudeDeg, 360f);
            if (norm >= 60f && norm < 180f) return Lane.Left;
            if (norm >= 180f && norm < 300f) return Lane.Right;
            return Lane.Center;
        }

        public bool ClassifyTap(float durationSec, float peakPenetrationMm, out bool isValid)
        {
            isValid = (durationSec <= ContainmentDribbleConfig.MaxTapContactDurationSec &&
                       peakPenetrationMm >= ContainmentDribbleConfig.MinTapPeakPenetrationMm);
            return isValid;
        }

        public static float FuseTaps(float speed1, float speed2, float timeDeltaSec, out bool wasFused)
        {
            if (timeDeltaSec <= ContainmentDribbleConfig.DoubleTapFusionWindowSec)
            {
                wasFused = true;
                return Mathf.Max(speed1, speed2) * 1.12f;
            }
            wasFused = false;
            return speed2;
        }
    }
}
