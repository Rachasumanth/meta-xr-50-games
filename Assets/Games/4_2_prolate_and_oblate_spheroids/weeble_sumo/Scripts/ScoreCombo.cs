using System;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class ScoreCombo
    {
        public int ComboCount { get; private set; } = 0;
        public float Multiplier { get; private set; } = 1.0f;
        public float ComboTimer { get; private set; } = 0f;
        public int TotalScore { get; private set; } = 0;

        public const float ComboWindowSec = 2.5f;

        public void RegisterBump(float thetaBumpDeg)
        {
            float tiltFactor = 1.0f + 0.5f * Mathf.Min(thetaBumpDeg, 22.0f) / 22.0f;
            int points = Mathf.RoundToInt(100f * Multiplier * tiltFactor);
            TotalScore += points;
        }

        public void RegisterRingOut()
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
            Multiplier = 1.0f + 0.25f * Mathf.Min(ComboCount - 1, 4);
            Multiplier = Mathf.Min(Multiplier, 2.0f);

            int points = Mathf.RoundToInt(150f * Multiplier);
            TotalScore += points;
        }

        public void ResetCombo()
        {
            ComboCount = 0;
            Multiplier = 1.0f;
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
