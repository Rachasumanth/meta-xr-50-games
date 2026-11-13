using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class RollFusion
    {
        public Vector3 AngularVelocityDegS { get; private set; }
        public Quaternion IntegratedRotation { get; private set; } = Quaternion.identity;

        public Vector3 ComputeFusedOmega(Vector3[] contactOmegas, float[] weights)
        {
            if (contactOmegas == null || contactOmegas.Length == 0)
                return Vector3.zero;

            Vector3 fused = Vector3.zero;
            float totalWeight = 0f;
            for (int i = 0; i < contactOmegas.Length; i++)
            {
                float w = (weights != null && i < weights.Length) ? weights[i] : 1f;
                fused += contactOmegas[i] * w;
                totalWeight += w;
            }

            if (totalWeight > 0.0001f) fused /= totalWeight;
            return fused;
        }

        public Vector3 ApplySlipClamp(Vector3 omegaDegS, float maxLimitDegS = MicroPlanetScannerConfig.SlipLimitDegS)
        {
            float mag = omegaDegS.magnitude;
            if (mag > maxLimitDegS)
            {
                return omegaDegS * (maxLimitDegS / mag);
            }
            return omegaDegS;
        }

        public void Step(Vector3 omegaDegS, float dt)
        {
            AngularVelocityDegS = ApplySlipClamp(omegaDegS);
            float angle = AngularVelocityDegS.magnitude * dt;
            if (angle > 0.0001f)
            {
                Vector3 axis = AngularVelocityDegS.normalized;
                Quaternion delta = Quaternion.AngleAxis(angle, axis);
                IntegratedRotation = delta * IntegratedRotation;
            }
        }
    }
}
