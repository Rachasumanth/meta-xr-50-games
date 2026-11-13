using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class HeightController
    {
        private float filteredHeight;
        private float filteredVelocity;
        private float oneEuroPrevRaw;
        private float oneEuroPrevDeriv;

        public float Height { get; private set; }
        public float Velocity { get; private set; }
        public float TargetHeight { get; private set; }

        public void SetTarget(float target)
        {
            TargetHeight = Mathf.Clamp(target, BernoulliLevitatorConfig.MinHeightM, BernoulliLevitatorConfig.MaxHeightM);
        }

        public void Step(float dt)
        {
            if (dt <= 0f) return;

            // One Euro filter simplified for height
            float rawDiff = (TargetHeight - oneEuroPrevRaw) / dt;
            oneEuroPrevRaw = TargetHeight;

            // Critically damped servo implementation as requested in spec
            float wn = BernoulliLevitatorConfig.ServoWn;
            float zeta = BernoulliLevitatorConfig.ServoZeta;

            float acc = wn * wn * (TargetHeight - Height) - 2.0f * zeta * wn * Velocity;
            acc = Mathf.Clamp(acc, -BernoulliLevitatorConfig.MaxAcceleration, BernoulliLevitatorConfig.MaxAcceleration);

            Velocity = Mathf.Clamp(Velocity + acc * dt, -BernoulliLevitatorConfig.MaxVelocity, BernoulliLevitatorConfig.MaxVelocity);
            Height = Mathf.Clamp(Height + Velocity * dt, BernoulliLevitatorConfig.MinHeightM, BernoulliLevitatorConfig.MaxHeightM);
        }
    }
}
