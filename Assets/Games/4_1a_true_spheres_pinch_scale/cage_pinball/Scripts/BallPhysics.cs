using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public class BallPhysics
    {
        public Vector3 Velocity { get; private set; }
        public Vector3 AngularVelocityRadS { get; private set; }
        public float SeamRotationDeg { get; private set; }

        public Vector3 ComputeRollingOmega(Vector3 linearVelocityMmS, float radiusMm = CagePinballConfig.BallRadiusMm)
        {
            // Table rolling law: omega = (n x v) / r with n = (0, 1, 0)
            Vector3 n = Vector3.up;
            Vector3 cross = Vector3.Cross(n, linearVelocityMmS);
            return cross / radiusMm;
        }

        public void Step(Vector3 linearVelocityMmS, float dt)
        {
            Velocity = linearVelocityMmS;
            AngularVelocityRadS = ComputeRollingOmega(linearVelocityMmS);
            float speedRadS = AngularVelocityRadS.magnitude;
            SeamRotationDeg += speedRadS * Mathf.Rad2Deg * dt;
        }

        public void Reset()
        {
            Velocity = Vector3.zero;
            AngularVelocityRadS = Vector3.zero;
            SeamRotationDeg = 0f;
        }
    }
}
