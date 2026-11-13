using System;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public enum TideState
    {
        BOOT,
        PRESENT,
        CALIBRATE,
        TIDE_INTRO,
        RUB,
        TIDE_END,
        RESULTS,
        GAME_OVER
    }

    public class TideStateMachine
    {
        public TideState CurrentState { get; private set; } = TideState.BOOT;
        public int CurrentTide { get; private set; } = 1;
        public int RecordedConstellations { get; private set; } = 0;
        public float StateTimer { get; private set; } = 0f;

        public event Action<TideState> OnStateChanged;

        public void TransitionTo(TideState next)
        {
            CurrentState = next;
            StateTimer = 0f;
            OnStateChanged?.Invoke(next);
        }

        public void StartTide()
        {
            TransitionTo(TideState.RUB);
        }

        public void RecordConstellation()
        {
            RecordedConstellations++;
        }

        public void Step(float dt, bool tideFinished)
        {
            StateTimer += dt;

            if (CurrentState == TideState.RUB)
            {
                if (tideFinished)
                {
                    TransitionTo(TideState.TIDE_END);
                }
            }
            else if (CurrentState == TideState.TIDE_END)
            {
                if (StateTimer >= 2.0f)
                {
                    if (CurrentTide >= 5)
                    {
                        TransitionTo(RecordedConstellations >= 4 ? TideState.RESULTS : TideState.GAME_OVER);
                    }
                    else
                    {
                        CurrentTide++;
                        TransitionTo(TideState.TIDE_INTRO);
                    }
                }
            }
            else if (CurrentState == TideState.TIDE_INTRO)
            {
                if (StateTimer >= 1.5f)
                {
                    StartTide();
                }
            }
        }
    }
}
