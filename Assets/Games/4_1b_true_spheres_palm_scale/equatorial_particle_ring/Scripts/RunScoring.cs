using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public class RunScoring
    {
        public int Score { get; private set; } = 0;
        public int HitsCount { get; private set; } = 0;
        public int GrazeCount { get; private set; } = 0;
        public int Chain { get; private set; } = 0;

        public void RecordHit(Target target, float energy)
        {
            HitsCount++;
            Chain++;
            target.IsDestroyed = true;
            Score += Mathf.RoundToInt(100f * (1.0f + 0.1f * Mathf.Min(Chain, 10)) + energy);
        }

        public void RecordGraze(Target target, float energy)
        {
            GrazeCount++;
            Score += Mathf.RoundToInt(40f + energy * 0.5f);
        }

        public void BreakChain()
        {
            Chain = 0;
        }

        public void Reset()
        {
            Score = 0;
            HitsCount = 0;
            GrazeCount = 0;
            Chain = 0;
        }
    }
}
