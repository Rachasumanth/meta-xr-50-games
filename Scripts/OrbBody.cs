using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class OrbBody
    {
        private float height;
        private float velocity;
        private bool isFalling;

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; }
        public bool IsFalling => isFalling;

        public void SetTargetHeight(float target)
        {
            if (!isFalling)
            {
                height = target;
                Position = new Vector3(0f, height, 0f);
            }
        }

        public void SetFalling(bool falling)
        {
            isFalling = falling;
        }

        public void Step(float dt, Vector3 spinInput)
        {
            if (dt <= 0f) return;

            if (isFalling)
            {
                velocity -= 9.81f * dt;
                height += velocity * dt;
                if (height <= 0f)
                {
                    height = 0f;
                    velocity = 0f;
                }
            }
            else
            {
                // Critically damped servo logic from spec
                float wn = 8.0f;
                float zeta = 1.0f;
                float acc = wn * wn * (height - Position.y) - 2f * zeta * wn * velocity;
                acc = Mathf.Clamp(acc, -12.0f, 12.0f);
                velocity = Mathf.Clamp(velocity + acc * dt, -1.2f, 1.2f);
                height = Mathf.Clamp(height + velocity * dt, 0.0f, 0.300f);
            }

            Position = new Vector3(Position.x, height, Position.z);
            Velocity = new Vector3(0f, velocity, 0f);

            // Quaternion integration using Quaternion.AngleAxis per instructions
            if (spinInput.sqrMagnitude > 0.001f)
            {
                float angle = spinInput.magnitude * dt;
                Vector3 axis = spinInput.normalized;
                Rotation = Rotation * Quaternion.AngleAxis(angle, axis);
            }
        }
    }
}
