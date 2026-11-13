using System;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit
{
    public class ScoreCombo
    {
        public int ComboCount { get; private set; } = 0;
        public float ComboTimer { get; private set; } = 0f;
        public int TotalScore { get; private set; } = 0;

        public const float ComboWindowSec = 4.0f;

        public void RegisterFragment()
        {
            if (ComboTimer > 0f)
            {
                ComboCount++;
            }
            else
            {
                ComboCount = 1;
            }

            ComboTimer = ComboWindowSec;
            float multiplier = 1.0f + 0.15f * Mathf.Min(ComboCount - 1, 5);
            int points = Mathf.RoundToInt(200f * multiplier);
            TotalScore += points;
        }

        public void RegisterConstellation(float timeRemainingSec)
        {
            int points = 500 + Mathf.RoundToInt(10f * timeRemainingSec);
            TotalScore += points;
        }

        public void ResetCombo()
        {
            ComboCount = 0;
            ComboTimer = 0f;
        }

        public void Step(float dt)
        {
            if (ComboTimer > 0f)
            {
                ComboTimer -= dt;
                if (ComboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }
    }
}
