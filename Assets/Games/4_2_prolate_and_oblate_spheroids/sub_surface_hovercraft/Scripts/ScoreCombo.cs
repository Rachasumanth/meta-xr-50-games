using System;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public class ScoreCombo
    {
        public int ComboCount { get; private set; } = 0;
        public float Multiplier { get; private set; } = 1.0f;
        public float ComboTimer { get; private set; } = 0f;
        public int TotalScore { get; private set; } = 0;

        public const float ComboWindowSec = 3.0f;

        public void RegisterHit(int basePoints = 100)
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
            Multiplier = 1.0f + 0.25f * Mathf.Min(ComboCount, 4);
            Multiplier = Mathf.Min(Multiplier, 2.0f);

            TotalScore += Mathf.RoundToInt(basePoints * Multiplier);
        }

        public void RegisterMiss()
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
                    ComboCount = 0;
                    Multiplier = 1.0f;
                }
            }
        }
    }
}
