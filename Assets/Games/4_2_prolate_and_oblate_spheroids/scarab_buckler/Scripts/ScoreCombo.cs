using System;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class ScoreCombo
    {
        public int ComboCount { get; private set; } = 0;
        public float Multiplier { get; private set; } = 1.0f;
        public float ComboTimer { get; private set; } = 0f;
        public int TotalScore { get; private set; } = 0;

        public const float ComboWindowSec = 1.2f;

        public void RegisterTargetHit(float angleErrorDeg = 0f)
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
            Multiplier = 1.0f + 0.20f * Mathf.Min(ComboCount, 5);
            Multiplier = Mathf.Min(Multiplier, 2.0f);

            float prec = 1.0f + 0.5f * Mathf.Max(0f, (1.0f - angleErrorDeg / 3.0f));
            int points = Mathf.RoundToInt(100f * Multiplier * prec);
            TotalScore += points;
        }

        public void RegisterNova(int targetsDestroyed)
        {
            int points = 300 + 50 * targetsDestroyed;
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
