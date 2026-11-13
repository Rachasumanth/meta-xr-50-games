using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class SpinFusion
    {
        public Vector3 AngularVelocity { get; private set; }

        public void Step(Vector3 inputOmega, float dt)
        {
            // Apply slip clamp
            float maxDeg = BernoulliLevitatorConfig.MaxSlipAngularVelocityDeg;
            float mag = inputOmega.magnitude;
            if (mag > maxDeg)
            {
                inputOmega = inputOmega.normalized * maxDeg;
            }

            // Low-pass filter / fusion integration
            float alpha = 0.85f;
            AngularVelocity = Vector3.Lerp(AngularVelocity, inputOmega, alpha);
        }
    }
}
