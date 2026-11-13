using System;
using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public enum PhaseState
    {
        Brief,
        Active,
        PhaseClear,
        RunOver
    }

    public class PhaseDirector
    {
        public PhaseState State { get; private set; } = PhaseState.Brief;
        public float TimeRemainingSec { get; private set; } = GeyserVentingConfig.PhaseDurationSec;
        public int CurrentPhase { get; private set; } = 1;
        public int SealedVentsCount { get; private set; } = 0;

        public event Action OnPhaseClear;
        public event Action OnRunOver;

        public void StartPhase(int phase = 1)
        {
            CurrentPhase = phase;
            TimeRemainingSec = GeyserVentingConfig.PhaseDurationSec;
            SealedVentsCount = 0;
            State = PhaseState.Active;
        }

        public void RegisterSeal(PressureIntegrator pressure, CrustModel crust)
        {
            SealedVentsCount++;
            if (SealedVentsCount >= 8)
            {
                State = PhaseState.PhaseClear;
                crust.RepairCracks();
                OnPhaseClear?.Invoke();
            }
        }

        public void Tick(float dt, VentField ventField, PressureIntegrator pressure, CrustModel crust, bool isCovered)
        {
            if (State != PhaseState.Active) return;

            if (crust.IsCollapsed || pressure.IsErupted)
            {
                State = PhaseState.RunOver;
                OnRunOver?.Invoke();
                return;
            }

            if (!isCovered)
            {
                TimeRemainingSec = Mathf.Max(0f, TimeRemainingSec - dt);
                float flux = ventField.ComputeTotalFlux();
                pressure.Integrate(dt, flux);
            }

            if (TimeRemainingSec <= 0f)
            {
                // Timeout penalty: exactly 18.0 pressure per unsealed vent
                int unsealed = 8 - SealedVentsCount;
                pressure.AddPressure(unsealed * GeyserVentingConfig.TimeoutPenaltyPerVent);

                if (pressure.IsErupted)
                {
                    State = PhaseState.RunOver;
                    OnRunOver?.Invoke();
                }
                else
                {
                    State = PhaseState.PhaseClear;
                    crust.RepairCracks();
                    OnPhaseClear?.Invoke();
                }
            }
        }
    }
}
