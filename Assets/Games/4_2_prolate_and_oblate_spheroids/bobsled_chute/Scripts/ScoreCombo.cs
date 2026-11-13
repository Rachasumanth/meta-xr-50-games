using System;
using UnityEngine;

namespace XRGames.Chapter4.BobsledChute
{
    public class ScoreCombo
    {
        public int TotalScore { get; private set; } = 0;

        public void BankHeatScore(float timeSec, bool clean)
        {
            // heat_score = max(0, 1000 - 500 * (t - 0.80 s))
            int heatScore = Mathf.Max(0, Mathf.RoundToInt(1000f - 500f * (timeSec - 0.80f)));
            if (clean) heatScore += 250;
            TotalScore += heatScore;
        }

        public void BankDescentBonus(float bestTimeSec)
        {
            if (bestTimeSec <= 1.35f)
            {
                TotalScore += 500; // gold bonus
            }
        }
    }
}
