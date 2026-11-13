using UnityEngine;

namespace XRGames.Chapter4.GyroSphereMech
{
    public class PropOrange
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public float RadiusMm { get; set; } = GyroSphereMechConfig.BallRadiusMm;
    }

    public class GyroController
    {
        public float HeadingDeg { get; set; } = 0f;
        public float Throttle { get; private set; } = 0f;
        public float D0 { get; set; } = 80.0f;
        public bool NeedsRegripPrompt { get; private set; } = false;

        public float ComputeDriveVelocity(float omegaDegS)
        {
            float omegaRad = omegaDegS * Mathf.Deg2Rad;
            float v = GyroSphereMechConfig.BallRadiusMm * omegaRad;
            return Mathf.Clamp(v, -GyroSphereMechConfig.MaxDriveSpeedMmS, GyroSphereMechConfig.MaxDriveSpeedMmS);
        }

        public float ComputeThrottle(float currentD)
        {
            float squeeze = D0 - currentD;
            Throttle = Mathf.Clamp01(squeeze / GyroSphereMechConfig.ThrottleMaxSqueezeMm);
            return Throttle;
        }

        public void ApplyYaw(float rawYawDeg)
        {
            if (Mathf.Abs(rawYawDeg) > GyroSphereMechConfig.MaxYawAngleDeg)
            {
                HeadingDeg = Mathf.Sign(rawYawDeg) * GyroSphereMechConfig.MaxYawAngleDeg;
                NeedsRegripPrompt = true;
            }
            else
            {
                HeadingDeg = rawYawDeg;
                NeedsRegripPrompt = false;
            }
        }

        public void ClutchRegrip(float newD0 = 80.0f)
        {
            D0 = newD0;
            HeadingDeg = 0f;
            NeedsRegripPrompt = false;
            Throttle = 0f;
        }
    }
}
