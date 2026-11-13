using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class MoonState
    {
        public int Corruption { get; private set; }
        public int Score { get; private set; }
        public int PurgeChain { get; private set; }
        public bool IsDestroyed => Corruption >= 100;

        public void ApplyCoreHit(int amount = 20)
        {
            Corruption = Mathf.Clamp(Corruption + amount, 0, 100);
            PurgeChain = 0;
        }

        public void HealCorruption(int amount = 10)
        {
            Corruption = Mathf.Clamp(Corruption - amount, 0, 100);
            PurgeChain++;
        }

        public void AddScore(int baseScore)
        {
            int bonus = PurgeChain > 1 ? (PurgeChain - 1) * 50 : 0;
            Score += baseScore + bonus;
        }

        public void Reset()
        {
            Corruption = 0;
            Score = 0;
            PurgeChain = 0;
        }
    }
}
