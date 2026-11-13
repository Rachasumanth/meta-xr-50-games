using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class OrbBody
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; }
        public bool IsFalling { get; private set; }

        private readonly HeightController _heightCtrl = new HeightController();
        private readonly SpinFusion _spinFusion = new SpinFusion();

        public void SetTargetHeight(float target)
        {
            _heightCtrl.SetTarget(target);
        }

        public void SetFalling(bool falling)
        {
            IsFalling = falling;
        }

        public void Step(float dt, Vector3 spinInput)
        {
            if (IsFalling)
            {
                // Free fall
                Velocity += Vector3.down * 9.81f * dt;
                Position += Velocity * dt;
                if (Position.y <= 0f)
                {
                    Position = new Vector3(Position.x, 0f, Position.z);
                    Velocity = Vector3.zero;
                }
            }
            else
            {
                _heightCtrl.Step(dt);
                Position = new Vector3(Position.x, _heightCtrl.Height, Position.z);
                Velocity = new Vector3(0f, _heightCtrl.Velocity, 0f);
            }

            _spinFusion.Step(spinInput, dt);
            Vector3 omega = _spinFusion.AngularVelocity;
            float angleDeg = omega.magnitude * Mathf.Rad2Deg * dt;
            if (angleDeg > 0.0001f)
            {
                Rotation = Quaternion.AngleAxis(angleDeg, omega.normalized) * Rotation;
            }
        }
    }
}
