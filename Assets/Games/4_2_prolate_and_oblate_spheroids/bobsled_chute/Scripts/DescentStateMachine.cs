using System;
using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public enum DescentState
    {
        BOOT,
        PRESENT,
        CALIBRATE,
        DESCENT_INTRO,
        HEAT_COUNTDOWN,
        RUN,
        CRASH,
        HEAT_END,
        DESCENT_END,
        RESULTS,
        GAME_OVER
    }

    public class DescentStateMachine
    {
        public DescentState CurrentState { get; private set; } = DescentState.BOOT;
        public int CurrentDescent { get; private set; } = 1;
        public int CurrentHeat { get; private set; } = 1;
        public float StateTimer { get; private set; } = 0f;

        public event Action<DescentState> OnStateChanged;

        public void TransitionTo(DescentState next)
        {
            CurrentState = next;
            StateTimer = 0f;
            OnStateChanged?.Invoke(next);
        }

        public void StartRun()
        {
            TransitionTo(DescentState.RUN);
        }

        public void Step(float dt, bool finished, int crashes)
        {
            StateTimer += dt;

            if (CurrentState == DescentState.RUN)
            {
                if (crashes >= 2)
                {
                    TransitionTo(DescentState.HEAT_END); // DNF
                    return;
                }

                if (finished)
                {
                    TransitionTo(DescentState.HEAT_END);
                    return;
                }
            }
            else if (CurrentState == DescentState.HEAT_END)
            {
                if (StateTimer >= 2.0f)
                {
                    if (CurrentHeat < 3)
                    {
                        CurrentHeat++;
                        TransitionTo(DescentState.HEAT_COUNTDOWN);
                    }
                    else
                    {
                        TransitionTo(DescentState.DESCENT_END);
                    }
                }
            }
            else if (CurrentState == DescentState.DESCENT_END)
            {
                if (StateTimer >= 3.0f)
                {
                    if (CurrentDescent >= 5)
                    {
                        TransitionTo(DescentState.RESULTS);
                    }
                    else
                    {
                        CurrentDescent++;
                        CurrentHeat = 1;
                        TransitionTo(DescentState.DESCENT_INTRO);
                    }
                }
            }
            else if (CurrentState == DescentState.HEAT_COUNTDOWN)
            {
                if (StateTimer >= 1.0f)
                {
                    StartRun();
                }
            }
            else if (CurrentState == DescentState.DESCENT_INTRO)
            {
                if (StateTimer >= 1.5f)
                {
                    TransitionTo(DescentState.HEAT_COUNTDOWN);
                }
            }
        }
    }
}
