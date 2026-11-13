using System;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class RockingBody
    {
        public const float OmegaN = WeebleSumoConfig.OmegaN;       // 11.76 rad/s
        public const float Zeta = WeebleSumoConfig.DampingZeta;    // 0.055
        public const float InertiaIo = WeebleSumoConfig.InertiaIo; // 6.987e-4 kg*m^2
        public const float NaturalFrequencyHz = 1.872f;
        public const float PeriodSec = 0.534f;

        public float ThetaRad;      // radians
        public float ThetaDot;      // rad/s
        public float AzimuthRad;    // radians
        public bool IsToppled { get; private set; }
        public float ReseatTimer { get; private set; }

        public float ThetaDeg => ThetaRad * Mathf.Rad2Deg;

        public RockingBody()
        {
            Reset();
        }

        public void Reset()
        {
            ThetaRad = 0f;
            ThetaDot = 0f;
            AzimuthRad = 0f;
            IsToppled = false;
            ReseatTimer = 0f;
        }

        public void ApplyAngularImpulse(float jTheta)
        {
            if (IsToppled) return;
            ThetaDot += jTheta / InertiaIo;
        }

        public void TriggerTopple()
        {
            IsToppled = true;
            ReseatTimer = 3.0f;
            ThetaRad = Mathf.Sign(ThetaRad) * 45f * Mathf.Deg2Rad;
            ThetaDot = 0f;
        }

        public void Step(float dt, float extTorque = 0f)
        {
            if (IsToppled)
            {
                ReseatTimer -= dt;
                if (ReseatTimer <= 0f)
                {
                    Reset();
                }
                return;
            }

            // ODE: theta_ddot = -omega_n^2 * theta - 2 * zeta * omega_n * theta_dot + extTorque / InertiaIo
            float restoring = -OmegaN * OmegaN * ThetaRad;
            float damping = -2.0f * Zeta * OmegaN * ThetaDot;
            float forcing = extTorque / InertiaIo;

            float thetaDdot = restoring + damping + forcing;
            ThetaDot += thetaDdot * dt;
            ThetaRad += ThetaDot * dt;

            // Check topple threshold at 40 degrees
            if (Mathf.Abs(ThetaDeg) > 40.0f)
            {
                TriggerTopple();
            }
        }
    }
}
