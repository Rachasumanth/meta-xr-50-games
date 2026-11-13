using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class SpinFusion
    {
        public Vector3 AngularVelocity { get; private set; }

        public void Step(Vector3 inputOmega, float dt)
        {
            // Slip clamp and low-pass filter (working in deg/s)
            float maxSlip = BernoulliLevitatorConfig.MaxSlipSpeed;
            Vector3 clampedOmega = Vector3.ClampMagnitude(inputOmega, maxSlip);

            // Fast low pass filter approx
            float alpha = 1f - Mathf.Exp(-dt * 60f);
            AngularVelocity = Vector3.Lerp(AngularVelocity, clampedOmega, alpha);
        }
    }
}
