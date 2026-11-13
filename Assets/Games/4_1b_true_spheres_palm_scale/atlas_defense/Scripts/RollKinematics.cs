using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class RollKinematics
    {
        public Quaternion IntegratedRotation { get; private set; } = Quaternion.identity;
        public float TotalUnwrappedYawDeg { get; private set; }

        public Vector3 ComputeRollingOmega(Vector3 normal, Vector3 tangentialVelocityMmS, float radiusMm = AtlasDefenseConfig.GlobeRadiusMm)
        {
            Vector3 cross = Vector3.Cross(normal.normalized, tangentialVelocityMmS);
            return cross / radiusMm; // rad/s
        }

        public void StepStroke(float yawDeg)
        {
            TotalUnwrappedYawDeg += yawDeg;
            Quaternion delta = Quaternion.AngleAxis(yawDeg, Vector3.up);
            IntegratedRotation = delta * IntegratedRotation;
        }
    }
}
