using System;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public class DepthSonar
    {
        public const int MinBand = 0;
        public const int MaxBand = 4;
        public const float PingIntervalSec = 1.4f;
        public const float RingExpansionSpeed = 0.500f; // 500 mm/s
        public const float MaxRingRadius = 0.220f;      // 220 mm

        public int CurrentBand { get; private set; } = 0;
        public float PingTimer { get; private set; } = 0f;
        public float RingRadius { get; private set; } = 0f;
        public bool IsPingActive { get; private set; } = false;

        public event Action<int> OnBandChanged;
        public event Action OnPingTriggered;

        public void ChangeBand(int delta)
        {
            int oldBand = CurrentBand;
            CurrentBand = Mathf.Clamp(CurrentBand + delta, MinBand, MaxBand);
            if (CurrentBand != oldBand)
            {
                TriggerPing();
                OnBandChanged?.Invoke(CurrentBand);
            }
        }

        public void TriggerPing()
        {
            IsPingActive = true;
            RingRadius = 0f;
            PingTimer = 0f;
            OnPingTriggered?.Invoke();
        }

        public void Step(float dt)
        {
            PingTimer += dt;
            if (PingTimer >= PingIntervalSec)
            {
                TriggerPing();
            }

            if (IsPingActive)
            {
                RingRadius += RingExpansionSpeed * dt;
                if (RingRadius >= MaxRingRadius)
                {
                    RingRadius = MaxRingRadius;
                    IsPingActive = false;
                }
            }
        }
    }
}
