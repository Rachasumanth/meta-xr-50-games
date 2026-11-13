using System;
using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public enum SeasonState
    {
        Onboarding,
        InSeason,
        Rest,
        Completed,
        Failed
    }

    public class SeasonStateMachine
    {
        public SeasonState State { get; private set; } = SeasonState.Onboarding;
        public float TimeRemainingSec { get; private set; } = KatamariHarvestConfig.SeasonDurationSec;
        public float Quota { get; set; } = 700.0f;
        public bool IsOffTray { get; private set; } = false;
        public int QuotaEvaluatedCount { get; private set; } = 0;
        public bool QuotaMet { get; private set; } = false;

        public event Action<FeedbackEvent> OnFeedback;

        public void StartSeason(float quota = 700.0f)
        {
            Quota = quota;
            TimeRemainingSec = KatamariHarvestConfig.SeasonDurationSec;
            QuotaEvaluatedCount = 0;
            QuotaMet = false;
            State = SeasonState.InSeason;
        }

        public void SetOffTray(bool offTray)
        {
            if (IsOffTray != offTray)
            {
                IsOffTray = offTray;
                if (IsOffTray)
                {
                    OnFeedback?.Invoke(FeedbackEvent.OffTray);
                }
            }
        }

        public void Tick(float dt, float currentMass)
        {
            if (State != SeasonState.InSeason || IsOffTray) return;

            TimeRemainingSec = Mathf.Max(0.0f, TimeRemainingSec - dt);

            if (TimeRemainingSec <= 0.005f && QuotaEvaluatedCount == 0)
            {
                QuotaEvaluatedCount++;
                if (currentMass >= Quota)
                {
                    QuotaMet = true;
                    State = SeasonState.Completed;
                    OnFeedback?.Invoke(FeedbackEvent.QuotaMet);
                }
                else
                {
                    QuotaMet = false;
                    State = SeasonState.Failed;
                }
                OnFeedback?.Invoke(FeedbackEvent.SeasonEnd);
            }
        }
    }
}
