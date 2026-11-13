using System;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class StabilityMeter
    {
        public int Value { get; private set; } = 100;
        public int LossesCount { get; private set; } = 0;
        public bool IsBoutLost => Value <= 0;
        public bool IsGameOver => LossesCount >= 2;

        public event Action<int> OnStabilityChanged;
        public event Action OnBoutLost;
        public event Action OnGameOver;

        public void Reset()
        {
            Value = 100;
        }

        public void ApplyImpPush()
        {
            Value = Mathf.Max(0, Value - 8);
            OnStabilityChanged?.Invoke(Value);
            CheckLoss();
        }

        public void ApplyToppleLoss()
        {
            Value = Mathf.Max(0, Value - 34);
            OnStabilityChanged?.Invoke(Value);
            CheckLoss();
        }

        public void RestoreOnRingOut()
        {
            Value = Mathf.Min(100, Value + 3);
            OnStabilityChanged?.Invoke(Value);
        }

        public void Step(float dt, float thetaDeg)
        {
            if (Value > 0 && Mathf.Abs(thetaDeg) < 5.0f && Value < 100)
            {
                // Regenerates 4/s
                Value = Mathf.Min(100, Value + Mathf.RoundToInt(4f * dt));
            }
        }

        private void CheckLoss()
        {
            if (Value <= 0)
            {
                LossesCount++;
                OnBoutLost?.Invoke();
                if (LossesCount >= 2)
                {
                    OnGameOver?.Invoke();
                }
            }
        }
    }
}
