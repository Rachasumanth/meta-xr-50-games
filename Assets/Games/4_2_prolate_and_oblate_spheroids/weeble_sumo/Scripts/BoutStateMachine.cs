using System;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public enum BoutState
    {
        BOOT,
        PRESENT,
        CALIBRATE,
        BOUT_INTRO,
        FIGHT,
        LUNGE,
        RESEAT,
        BOUT_END,
        RESULTS,
        GAME_OVER
    }

    public class BoutStateMachine
    {
        public BoutState CurrentState { get; private set; } = BoutState.BOOT;
        public int CurrentBout { get; private set; } = 1;
        public float BoutClock { get; private set; } = 40.0f;
        public float StateTimer { get; private set; } = 0f;

        public event Action<BoutState> OnStateChanged;

        public void TransitionTo(BoutState next)
        {
            CurrentState = next;
            StateTimer = 0f;
            OnStateChanged?.Invoke(next);
        }

        public void StartFight()
        {
            BoutClock = 40.0f;
            TransitionTo(BoutState.FIGHT);
        }

        public void Step(float dt, bool isBoutLost, bool isGameOver, int impsRemaining)
        {
            StateTimer += dt;

            if (isGameOver)
            {
                if (CurrentState != BoutState.GAME_OVER)
                    TransitionTo(BoutState.GAME_OVER);
                return;
            }

            if (CurrentState == BoutState.FIGHT || CurrentState == BoutState.LUNGE)
            {
                BoutClock -= dt;

                if (isBoutLost)
                {
                    TransitionTo(BoutState.BOUT_END);
                    return;
                }

                if (impsRemaining <= 0 || BoutClock <= 0f)
                {
                    TransitionTo(BoutState.BOUT_END);
                    return;
                }
            }
            else if (CurrentState == BoutState.BOUT_END)
            {
                if (StateTimer >= 2.0f)
                {
                    if (CurrentBout >= 5)
                    {
                        TransitionTo(BoutState.RESULTS);
                    }
                    else
                    {
                        CurrentBout++;
                        TransitionTo(BoutState.BOUT_INTRO);
                    }
                }
            }
            else if (CurrentState == BoutState.BOUT_INTRO)
            {
                if (StateTimer >= 2.0f)
                {
                    StartFight();
                }
            }
        }
    }
}
