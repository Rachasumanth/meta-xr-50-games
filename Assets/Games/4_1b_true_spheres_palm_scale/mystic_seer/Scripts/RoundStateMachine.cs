using System;
using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public enum RoundState
    {
        Onboarding,
        Active,
        RoundClear,
        GameOver
    }

    public class RoundStateMachine
    {
        public RoundState State { get; private set; } = RoundState.Onboarding;
        public float TimeRemainingSec { get; private set; } = MysticSeerConfig.RoundDurationSec;

        public event Action OnRoundClear;
        public event Action OnGameOver;

        public void StartRound()
        {
            TimeRemainingSec = MysticSeerConfig.RoundDurationSec;
            State = RoundState.Active;
        }

        public void Tick(float dt, ThiefDirector thiefDirector, FogField fog)
        {
            if (State != RoundState.Active) return;

            TimeRemainingSec = Mathf.Max(0f, TimeRemainingSec - dt);

            if (thiefDirector.IsGameOver)
            {
                State = RoundState.GameOver;
                OnGameOver?.Invoke();
                return;
            }

            if (TimeRemainingSec <= 0f)
            {
                // Round ends at 75 s. Any unrevealed or active thief counts as an escape
                bool allCleared = true;
                for (int i = 0; i < thiefDirector.Thieves.Count; i++)
                {
                    var t = thiefDirector.Thieves[i];
                    if (t.State == ThiefState.Hidden || t.State == ThiefState.Revealed)
                    {
                        thiefDirector.EscapeThief(t, fog);
                        allCleared = false;
                    }
                }

                if (thiefDirector.IsGameOver)
                {
                    State = RoundState.GameOver;
                    OnGameOver?.Invoke();
                }
                else if (allCleared && thiefDirector.EscapeCount == 0)
                {
                    State = RoundState.RoundClear;
                    OnRoundClear?.Invoke();
                }
                else
                {
                    State = RoundState.GameOver;
                    OnGameOver?.Invoke();
                }
            }
        }
    }
}
