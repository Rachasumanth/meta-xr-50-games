using UnityEngine;

namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class TwistIntegrator
    {
        public float FilteredOmegaDegS { get; private set; }

        public float ProcessTwist(float rawOmegaDegS, bool isPinchValid)
        {
            if (!isPinchValid || Mathf.Abs(rawOmegaDegS) < 0.1f)
            {
                FilteredOmegaDegS = 0f;
            }
            else
            {
                FilteredOmegaDegS = rawOmegaDegS;
            }
            return FilteredOmegaDegS;
        }
    }
}
