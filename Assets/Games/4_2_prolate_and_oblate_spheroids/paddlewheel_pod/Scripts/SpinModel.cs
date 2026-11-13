using System;
using UnityEngine;

namespace XRGames.Chapter4.PaddlewheelPod
{
    public enum PinchState
    {
        Armed,
        Slip,
        Released
    }

    public class PinchModel
    {
        public PinchState State = PinchState.Released;
        public float SlipTimer = 0f;

        public PinchState UpdatePinch(float poleSepMm, float dt)
        {
            if (poleSepMm >= 65.0f && poleSepMm <= 78.0f)
            {
                State = PinchState.Armed;
                SlipTimer = 0f;
            }
            else if (poleSepMm > 78.0f && poleSepMm <= 84.5f)
            {
                State = PinchState.Slip;
                SlipTimer += dt;
            }
            else
            {
                State = PinchState.Released;
                SlipTimer = 0f;
            }
            return State;
        }
    }

    public class SpinModel
    {
        public const float MaxSpin = PaddlewheelPodConfig.MaxSpinRadS; // 18.0 rad/s
        public const float Kup = 4.0f; // mm/s per rad/s
        public float OmegaS = 0f;

        public void ApplyEquatorialStroke(float strokeSpeedMmS, float durationSec)
        {
            // 300 mm/s for 100 ms raises omega_s by 1.25 rad/s
            float dOmega = 1.25f * (strokeSpeedMmS / 300.0f) * (durationSec / 0.100f);
            OmegaS = Mathf.Clamp(OmegaS + dOmega, 0f, MaxSpin);
        }

        public void Step(float dt)
        {
            // Decays from 12 to 4.0 rad/s in 1.9s
            float linearDrag = 0.55f * OmegaS;
            float quadDrag = 0.06f * OmegaS * OmegaS / 18.0f;
            OmegaS = Mathf.Max(0f, OmegaS - (linearDrag + quadDrag) * dt);
        }

        public float ComputeAscentSpeed(float steamLevel, bool isInKelp)
        {
            if (steamLevel <= 0f)
            {
                return -24.0f; // 24 mm/s sink when steam empty
            }

            if (OmegaS > 2.0f)
            {
                float vUp = Mathf.Min(Kup * OmegaS, 72.0f);
                if (isInKelp) vUp *= 0.75f; // 75% in kelp
                return vUp;
            }

            return 0f; // depth holds
        }

        public float ComputeSteamDrain(float dt)
        {
            if (OmegaS > 2.0f)
            {
                // Drains 5.5/s at omega = 12
                return 5.5f * (OmegaS / 12.0f) * dt;
            }
            return 0f;
        }
    }

    public class SteerModel
    {
        public float LateralPosMm = 0f;

        public static float ComputeLateralSpeedMmS(float tiltDeg)
        {
            // 22 deg gives 90 mm/s; 10 deg gives 42 mm/s
            float sign = Mathf.Sign(tiltDeg);
            float absTilt = Mathf.Abs(tiltDeg);
            float rad = absTilt * Mathf.Deg2Rad;
            // Scale based on sin(tilt)
            float speed = 90.0f * (Mathf.Sin(rad) / Mathf.Sin(22.0f * Mathf.Deg2Rad));
            return sign * speed;
        }

        public void Step(float tiltDeg, float dt)
        {
            float vLat = ComputeLateralSpeedMmS(tiltDeg);
            LateralPosMm = Mathf.Clamp(LateralPosMm + vLat * dt, -PaddlewheelPodConfig.MaxShaftClampMm, PaddlewheelPodConfig.MaxShaftClampMm);
        }
    }
}
