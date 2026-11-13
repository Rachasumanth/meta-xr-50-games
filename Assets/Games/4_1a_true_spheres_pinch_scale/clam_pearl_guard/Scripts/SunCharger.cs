using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class SunCharger
    {
        private float _charge;
        private float _bloom;

        public float Charge => _charge;
        public float Bloom => _bloom;

        // Tilt-focus law (spec item 5): F = 1.00 at 0 deg, 1.17 at 10 deg, 0.80 at
        // 30 deg (+/- 0.02). Piecewise-linear through the three spec anchors; the
        // 0-10 deg and 10-30 deg segments interpolate exactly through them.
        // Beyond the anchors the slope continues, clamped to a sane [0.5, 1.2] band.
        public static float ComputeTiltFocus(float palmTiltDegrees)
        {
            float f0 = ClamPearlGuardConfig.TiltFocusNeutral;   // at 0 deg
            float f10 = ClamPearlGuardConfig.TiltFocus10Deg;    // at 10 deg
            float f30 = ClamPearlGuardConfig.TiltFocus30Deg;    // at 30 deg

            float focus;
            if (palmTiltDegrees <= 0f)
            {
                focus = f0;
            }
            else if (palmTiltDegrees <= 10f)
            {
                focus = Mathf.Lerp(f0, f10, palmTiltDegrees / 10f);
            }
            else if (palmTiltDegrees <= 30f)
            {
                focus = Mathf.Lerp(f10, f30, (palmTiltDegrees - 10f) / 20f);
            }
            else
            {
                float slope = (f30 - f10) / 20f; // per degree past 30
                focus = Mathf.Max(0.5f, f30 + slope * (palmTiltDegrees - 30f));
            }
            return Mathf.Clamp(focus, 0.5f, 1.2f);
        }

        public void UpdateCharge(float openness, float palmTiltDegrees, float deltaTime)
        {
            float focus = ComputeTiltFocus(palmTiltDegrees);

            if (openness > 0.9f)
            {
                _charge += ClamPearlGuardConfig.SunChargeRatePerSec * focus * deltaTime;
                _charge = Mathf.Clamp(_charge, 0f, ClamPearlGuardConfig.ChargeFullUnits);
            }

            _bloom = _charge / ClamPearlGuardConfig.ChargeFullUnits;
        }

        public void Reset()
        {
            _charge = 0f;
            _bloom = 0f;
        }
    }
}
