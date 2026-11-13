using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class RingField
    {
        public bool CheckPass(float orbDistanceToCenter)
        {
            float distMm = orbDistanceToCenter < 1.0f ? orbDistanceToCenter * 1000f : orbDistanceToCenter;
            return distMm <= BernoulliLevitatorConfig.StandardRingRadiusMm;
        }

        public bool CheckPerfect(float orbDistanceToCenter)
        {
            float distMm = orbDistanceToCenter < 1.0f ? orbDistanceToCenter * 1000f : orbDistanceToCenter;
            return distMm <= BernoulliLevitatorConfig.PerfectRingRadiusMm;
        }
    }
}
