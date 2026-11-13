using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class ShieldSystem
    {
        public bool UpdateLock(MeteorInstance m, float thetaDeg, float dt)
        {
            if (m == null) return false;

            if (thetaDeg < AtlasDefenseConfig.LockAngleEnterDeg)
            {
                m.LockTimeSec += dt;
            }
            else if (thetaDeg > AtlasDefenseConfig.LockAngleReleaseDeg)
            {
                m.LockTimeSec = 0f;
            }

            m.IsLocked = (m.LockTimeSec >= AtlasDefenseConfig.LockHoldDurationSec - 0.0001f);
            return m.IsLocked;
        }

        public bool CheckIntercept(MeteorInstance m, float thetaDeg)
        {
            return (m != null && m.IsLocked && thetaDeg < AtlasDefenseConfig.InterceptAngleCapDeg);
        }
    }
}
