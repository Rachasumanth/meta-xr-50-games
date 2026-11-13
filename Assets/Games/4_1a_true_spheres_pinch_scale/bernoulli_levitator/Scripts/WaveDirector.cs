using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class WaveDirector
    {
        public int CurrentWave { get; private set; } = 1;

        public void NextWave()
        {
            if (CurrentWave < 6)
            {
                CurrentWave++;
            }
        }
    }
}
