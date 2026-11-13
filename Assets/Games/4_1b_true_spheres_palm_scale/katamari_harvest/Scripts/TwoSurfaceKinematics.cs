using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public class TwoSurfaceKinematics
    {
        public Vector3 ComputeBallVelocity(Vector3 palmVelocityMmS)
        {
            return palmVelocityMmS * 0.5f;
        }

        public Vector3 ComputeRollingOmega(Vector3 normal, Vector3 palmVelocityMmS, float radiusMm = KatamariHarvestConfig.BallRadiusMm)
        {
            Vector3 cross = Vector3.Cross(normal.normalized, palmVelocityMmS);
            return cross / (2.0f * radiusMm);
        }
    }
}
