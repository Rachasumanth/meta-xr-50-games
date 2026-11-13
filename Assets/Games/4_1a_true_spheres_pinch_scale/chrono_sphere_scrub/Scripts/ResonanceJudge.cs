using UnityEngine;

namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class ResonanceJudge
    {
        public float TargetTMs { get; private set; } = 2500.0f;
        public float WindowMs { get; private set; } = 100.0f;

        public void Configure(float targetTMs, float windowMs)
        {
            TargetTMs = targetTMs;
            WindowMs = windowMs;
        }

        public bool TryLock(float currentTMs, out float precision, out bool isPerfect)
        {
            float delta = Mathf.Abs(currentTMs - TargetTMs);
            precision = 0f;
            isPerfect = false;

            if (delta <= WindowMs)
            {
                precision = 1.0f - (delta / WindowMs);
                if (delta <= 0.1001f * WindowMs)
                {
                    isPerfect = true;
                }
                return true;
            }
            return false;
        }

        public float ComputePitchHz(float deltaSec)
        {
            return 220f + 880f * Mathf.Exp(-deltaSec / 0.5f);
        }
    }
}
