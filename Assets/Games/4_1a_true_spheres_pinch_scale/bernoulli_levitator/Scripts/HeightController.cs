using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class HeightController
    {
        public float Height { get; private set; }
        public float Velocity { get; private set; }
        public float TargetHeight { get; private set; }

        public void SetTarget(float target)
        {
            TargetHeight = Mathf.Clamp(target, 0.0f, 0.300f);
        }

        public void Step(float dt)
        {
            float wn = 18f;
            float zeta = 1.0f;

            float acc = wn * wn * (TargetHeight - Height) - 2 * zeta * wn * Velocity;
            acc = Mathf.Clamp(acc, -BernoulliLevitatorConfig.MaxOrbAcceleration, BernoulliLevitatorConfig.MaxOrbAcceleration);

            Velocity = Mathf.Clamp(Velocity + acc * dt, -BernoulliLevitatorConfig.MaxOrbVelocity, BernoulliLevitatorConfig.MaxOrbVelocity);
            Height = Mathf.Clamp(Height + Velocity * dt, 0.0f, 0.300f);

            if (Mathf.Abs(TargetHeight - Height) < 0.001f)
            {
                Height = TargetHeight;
            }
        }
    }
}
