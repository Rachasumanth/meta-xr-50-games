using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon
{
    public class TiltModel
    {
        public float FilteredThetaDeg { get; private set; }

        public float ProcessRoll(float rawRollDeg)
        {
            if (Mathf.Abs(rawRollDeg) < ScarabCanyonConfig.DeadzoneDeg)
            {
                FilteredThetaDeg = 0f;
            }
            else
            {
                FilteredThetaDeg = rawRollDeg;
            }
            return FilteredThetaDeg;
        }
    }
}
