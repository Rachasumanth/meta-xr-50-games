using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class PressureState
    {
        public int Pressure { get; private set; } = 3;
        public int Score { get; private set; }
        public int Combo { get; private set; }

        public void AddScore(int points)
        {
            Combo++;
            Score += points * Combo;
        }

        public void Penalty()
        {
            Pressure = Mathf.Max(0, Pressure - 1);
            Combo = 0;
        }

        public void Reset()
        {
            Pressure = 3;
            Score = 0;
            Combo = 0;
        }
    }
}
