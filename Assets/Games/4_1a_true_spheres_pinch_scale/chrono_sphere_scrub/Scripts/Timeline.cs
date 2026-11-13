using UnityEngine;

namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class Timeline
    {
        public float T { get; private set; }

        public void ScrubDegrees(float degrees, bool isPinchHeld)
        {
            if (!isPinchHeld) return;
            T += degrees * ChronoSphereScrubConfig.DegToMs;
        }

        public void Step(float dt, bool isPinchHeld)
        {
            if (T > ChronoSphereScrubConfig.TimelineMaxMs)
            {
                // Elastic bounce back
                float pullSpeed = (T - ChronoSphereScrubConfig.TimelineMaxMs) * 10f; // time constant 100ms
                T = Mathf.Max(ChronoSphereScrubConfig.TimelineMaxMs, T - pullSpeed * dt);
            }
            else if (T < ChronoSphereScrubConfig.TimelineMinMs)
            {
                float pullSpeed = (ChronoSphereScrubConfig.TimelineMinMs - T) * 10f;
                T = Mathf.Min(ChronoSphereScrubConfig.TimelineMinMs, T + pullSpeed * dt);
            }
        }

        public void SetT(float tMs)
        {
            T = tMs;
        }
    }
}
