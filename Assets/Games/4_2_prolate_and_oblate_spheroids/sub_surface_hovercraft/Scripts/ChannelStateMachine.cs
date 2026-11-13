using System;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public enum ChannelState
    {
        BOOT,
        PRESENT,
        CALIBRATE,
        CHANNEL_INTRO,
        RUN,
        CLUTCH,
        CHANNEL_END,
        INTERMISSION,
        RESULTS,
        GAME_OVER
    }

    public class ChannelStateMachine
    {
        public ChannelState CurrentState { get; private set; } = ChannelState.BOOT;
        public int CurrentChannel { get; private set; } = 1;
        public float StateTimer { get; private set; } = 0f;
        public float ChannelClock { get; private set; } = 70.0f;

        public event Action<ChannelState> OnStateChanged;

        public void TransitionTo(ChannelState newState)
        {
            CurrentState = newState;
            StateTimer = 0f;
            OnStateChanged?.Invoke(newState);
        }

        public void StartRun()
        {
            ChannelClock = 70.0f;
            TransitionTo(ChannelState.RUN);
        }

        public void Step(float dt, float craftX, int hull)
        {
            StateTimer += dt;

            if (CurrentState == ChannelState.RUN)
            {
                ChannelClock -= dt;

                if (hull <= 0)
                {
                    TransitionTo(ChannelState.GAME_OVER);
                    return;
                }

                if (craftX >= 0.900f) // 900 mm extraction gate
                {
                    TransitionTo(ChannelState.CHANNEL_END);
                    return;
                }

                if (ChannelClock <= 0f)
                {
                    TransitionTo(ChannelState.CHANNEL_END);
                    return;
                }
            }
            else if (CurrentState == ChannelState.CHANNEL_END)
            {
                if (StateTimer >= 2.0f)
                {
                    if (CurrentChannel >= 5)
                    {
                        TransitionTo(ChannelState.RESULTS);
                    }
                    else
                    {
                        CurrentChannel++;
                        TransitionTo(ChannelState.CHANNEL_INTRO);
                    }
                }
            }
            else if (CurrentState == ChannelState.CHANNEL_INTRO)
            {
                if (StateTimer >= 2.5f)
                {
                    StartRun();
                }
            }
        }
    }
}
