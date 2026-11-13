using System;
using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public enum RunState
    {
        Onboarding,
        Active,
        RunClear,
        GameOver
    }

    public class RunStateMachine
    {
        public RunState State { get; private set; } = RunState.Onboarding;
        public float TimeRemainingSec { get; private set; } = EquatorialParticleRingConfig.RunDurationSec;

        public event Action OnRunClear;
        public event Action OnGameOver;

        public void StartRun()
        {
            TimeRemainingSec = EquatorialParticleRingConfig.RunDurationSec;
            State = RunState.Active;
        }

        public void Tick(float dt, RingCharge charge, RunScoring scoring)
        {
            if (State != RunState.Active) return;

            TimeRemainingSec = Mathf.Max(0f, TimeRemainingSec - dt);

            if (charge.JamCount >= EquatorialParticleRingConfig.MaxJamsAllowed)
            {
                State = RunState.GameOver;
                OnGameOver?.Invoke();
                return;
            }

            if (TimeRemainingSec <= 0f)
            {
                if (scoring.HitsCount >= 6) // Level clear threshold for targets
                {
                    State = RunState.RunClear;
                    OnRunClear?.Invoke();
                }
                else
                {
                    State = RunState.GameOver;
                    OnGameOver?.Invoke();
                }
            }
        }
    }
}
