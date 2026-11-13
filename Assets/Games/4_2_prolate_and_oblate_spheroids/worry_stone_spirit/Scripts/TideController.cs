using System;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public class TideController
    {
        public const float DurationSec = WorryStoneSpiritConfig.TideDurationSec; // 50.0 s

        public float ElapsedTime { get; private set; } = 0f;
        public float FrontPhiDeg { get; private set; } = -80.0f;
        public bool IsCompleted => ElapsedTime >= DurationSec;
        public float TimeRemaining => Mathf.Max(0f, DurationSec - ElapsedTime);

        public event Action OnTideCompleted;

        public void Reset()
        {
            ElapsedTime = 0f;
            FrontPhiDeg = -80.0f;
        }

        public float ComputeFrontPhi(float t)
        {
            // phi_t(t) = -80 deg + 165 deg * (t / 50 s)
            return -80.0f + 165.0f * (t / DurationSec);
        }

        public void Step(float dt, FragmentField fragments)
        {
            if (IsCompleted) return;

            ElapsedTime += dt;
            FrontPhiDeg = ComputeFrontPhi(ElapsedTime);
            fragments.CheckTideLoss(FrontPhiDeg);

            if (ElapsedTime >= DurationSec)
            {
                ElapsedTime = DurationSec;
                OnTideCompleted?.Invoke();
            }
        }
    }
}
