using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon
{
    public class OrbJudge
    {
        public float MapToLateral(float sMm)
        {
            return sMm * ScarabCanyonConfig.LateralScale;
        }

        public void EvaluatePass(float lateralOffsetMm, out int scoreAwarded, out bool nearMiss)
        {
            // 8 mm lateral offset awards +50 clean and +20 near-miss = +70
            scoreAwarded = 50;
            nearMiss = false;

            if (lateralOffsetMm <= 10.0f)
            {
                scoreAwarded += 20; // near miss
                nearMiss = true;
            }
        }
    }
}
