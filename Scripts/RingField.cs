using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class RingField
    {
        private float nextSpawnTime;

        public bool CheckPass(float orbDistanceToCenterMm)
        {
            return orbDistanceToCenterMm <= BernoulliLevitatorConfig.RingPassThresholdMm;
        }

        public bool CheckPerfect(float orbDistanceToCenterMm)
        {
            return orbDistanceToCenterMm <= BernoulliLevitatorConfig.RingPerfectThresholdMm;
        }
    }
}
