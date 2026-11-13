using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public class SeanceScoring
    {
        public int Score { get; private set; } = 0;
        public int BanishCount { get; private set; } = 0;
        public int WrongTapCount { get; private set; } = 0;

        public void OnBanish(Thief thief, float clarity)
        {
            BanishCount++;
            int basePoints = 100;
            int clarityBonus = Mathf.RoundToInt((1.0f - clarity) * 50f);
            Score += basePoints + clarityBonus;
        }

        public void OnWrongTap()
        {
            WrongTapCount++;
            Score -= 25;
        }

        public void Reset()
        {
            Score = 0;
            BanishCount = 0;
            WrongTapCount = 0;
        }
    }
}
