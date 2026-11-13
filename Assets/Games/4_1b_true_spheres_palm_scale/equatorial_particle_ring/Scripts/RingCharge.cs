using System;
using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public class RingCharge
    {
        public float Heat { get; private set; } = 0f;
        public int JamCount { get; private set; } = 0;
        public bool IsJammed { get; private set; } = false;
        public float JamTimer { get; private set; } = 0f;

        public event Action OnJam;
        public event Action OnVent;

        public static float ComputeEnergy(float arcLengthMm, float omegaPeakRadS)
        {
            float normL = Mathf.Clamp01(arcLengthMm / 120.0f);
            float normW = Mathf.Clamp01(omegaPeakRadS / 10.0f);
            return 30.0f + 70.0f * (0.6f * normL + 0.4f * normW);
        }

        public void AddHeat(float amount)
        {
            if (IsJammed) return;

            Heat += amount;
            if (Heat >= EquatorialParticleRingConfig.MaxHeat)
            {
                TriggerJam();
            }
        }

        public void IntegrateHeat(float omegaRadS, float dt)
        {
            if (IsJammed) return;

            float normW = omegaRadS / 10.0f;
            float rate = 4.0f + 22.0f * (normW * normW);
            AddHeat(rate * dt);
        }

        public void TriggerJam()
        {
            IsJammed = true;
            JamTimer = EquatorialParticleRingConfig.JamDurationSec;
            JamCount++;
            Heat = EquatorialParticleRingConfig.HeatPostJam;
            OnJam?.Invoke();
        }

        public void Vent()
        {
            if (IsJammed) return;
            Heat = Mathf.Max(0f, Heat - 25.0f);
            OnVent?.Invoke();
        }

        public void Update(float dt, EquatorTracer tracer, CradleState cradle)
        {
            if (IsJammed)
            {
                JamTimer -= dt;
                if (JamTimer <= 0f)
                {
                    IsJammed = false;
                }
                return;
            }

            if (cradle == CradleState.Slip && tracer != null)
            {
                tracer.ArcLengthMm *= Mathf.Exp(-0.40f * dt);
            }
            else if (tracer == null || tracer.OmegaRadS <= 0.01f)
            {
                // passive cooling 9%/s
                Heat = Mathf.Max(0f, Heat - 9.0f * dt);
            }
        }
    }
}
