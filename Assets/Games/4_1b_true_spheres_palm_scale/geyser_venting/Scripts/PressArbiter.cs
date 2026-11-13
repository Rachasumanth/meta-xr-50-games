using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public class PressArbiter
    {
        public event Action<Vent> OnVentSealed;
        public event Action OnCrack;

        public void StepHold(Vent vent, int effectorCount, float dt, PressureIntegrator pressure, CrustModel crust)
        {
            float weight = effectorCount >= 2 ? 1.8f : 1.0f;
            float reqTime = effectorCount >= 2 ? GeyserVentingConfig.TwoEffectorHoldDurationSec : GeyserVentingConfig.SingleHoldDurationSec;

            vent.HoldTimer += weight * dt;
            if (vent.HoldTimer >= reqTime && !vent.IsSealed)
            {
                SealVent(vent, pressure);
            }
        }

        public void DecayHold(Vent vent, float timeSinceHold, float dt)
        {
            if (timeSinceHold > 0.100f && !vent.IsSealed)
            {
                vent.HoldTimer = Mathf.Max(0f, vent.HoldTimer - GeyserVentingConfig.VentDecayRate * dt);
            }
        }

        public void SealVent(Vent vent, PressureIntegrator pressure)
        {
            vent.State = VentState.Sealed;
            if (pressure != null)
            {
                pressure.SubtractPressure(GeyserVentingConfig.SealPressureDrop);
            }
            OnVentSealed?.Invoke(vent);
        }

        public void TriggerBareCrustCrack(PressureIntegrator pressure, CrustModel crust)
        {
            if (crust != null)
            {
                crust.AddCrack();
            }
            if (pressure != null)
            {
                pressure.AddPressure(GeyserVentingConfig.CrackPressureRise);
            }
            OnCrack?.Invoke();
        }
    }
}
