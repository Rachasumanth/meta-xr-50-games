using System;
using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler
{
    public enum VortexState
    {
        OFF,
        COLLAPSED,
        NOMINAL,
        OVERDRIVE
    }

    public class RockModel
    {
        public const float KPump = 6.0f; // deg per 5mm in-phase
        public const float TauDecay = VortexWobblerConfig.TauDecay; // 1.40 s
        public const float MaxAmplitude = VortexWobblerConfig.MaxAmplitudeDeg; // 26.0 deg

        public float AmplitudeDeg = 0f;
        public float PsiDeg = 0f;
        public float OmegaSpin = 0f;
        public VortexState State = VortexState.OFF;

        public void ApplyRimWalk(float travelMm, float phaseDiffDeg)
        {
            float q = Mathf.Cos(phaseDiffDeg * Mathf.Deg2Rad);
            float deltaA = KPump * q * (travelMm / 5.0f);
            AmplitudeDeg = Mathf.Clamp(AmplitudeDeg + deltaA, 0f, MaxAmplitude);
            UpdateState();
        }

        public void ApplyDeflectionDrain(float dt)
        {
            // Drains 3.6 deg/s
            AmplitudeDeg = Mathf.Max(0f, AmplitudeDeg - 3.6f * dt);
            UpdateState();
        }

        public void StepDecay(float dt)
        {
            AmplitudeDeg *= Mathf.Exp(-dt / TauDecay);
            UpdateState();
        }

        public void UpdateState()
        {
            if (AmplitudeDeg < 1.0f)
            {
                State = VortexState.OFF;
            }
            else if (AmplitudeDeg < VortexWobblerConfig.CollapseAmp)
            {
                State = VortexState.COLLAPSED;
            }
            else if (AmplitudeDeg > VortexWobblerConfig.OverdriveAmp)
            {
                State = VortexState.OVERDRIVE;
            }
            else
            {
                State = VortexState.NOMINAL;
            }
        }
    }
}
