using System;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public enum WaveState
    {
        BOOT,
        PRESENT,
        CALIBRATE,
        WAVE_INTRO,
        INBOUND,
        WAVE_END,
        RESULTS,
        GAME_OVER
    }

    public class WaveStateMachine
    {
        public WaveState CurrentState { get; private set; } = WaveState.BOOT;
        public int CurrentWave { get; private set; } = 1;
        public float StateTimer { get; private set; } = 0f;
        public float WaveClock { get; private set; } = 55.0f;

        public event Action<WaveState> OnStateChanged;

        public void TransitionTo(WaveState next)
        {
            CurrentState = next;
            StateTimer = 0f;
            OnStateChanged?.Invoke(next);
        }

        public void StartWave()
        {
            WaveClock = 55.0f;
            TransitionTo(WaveState.INBOUND);
        }

        public void Step(float dt, int playerHull, int liveTargets)
        {
            StateTimer += dt;

            if (CurrentState == WaveState.INBOUND)
            {
                WaveClock -= dt;
                if (playerHull <= 0)
                {
                    TransitionTo(WaveState.GAME_OVER);
                    return;
                }

                if (WaveClock <= 0f || liveTargets <= 0)
                {
                    TransitionTo(WaveState.WAVE_END);
                    return;
                }
            }
            else if (CurrentState == WaveState.WAVE_END)
            {
                if (StateTimer >= 2.0f)
                {
                    if (CurrentWave >= 5)
                    {
                        TransitionTo(WaveState.RESULTS);
                    }
                    else
                    {
                        CurrentWave++;
                        TransitionTo(WaveState.WAVE_INTRO);
                    }
                }
            }
            else if (CurrentState == WaveState.WAVE_INTRO)
            {
                if (StateTimer >= 1.5f)
                {
                    StartWave();
                }
            }
        }
    }
}
