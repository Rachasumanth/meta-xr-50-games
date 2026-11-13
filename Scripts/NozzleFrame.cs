using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class NozzleFrame
    {
        private bool wasBumped;
        private float bumpTimer;

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; }

        public void Update(Vector3 targetPos, Quaternion targetRot, float dt, bool tableBumped)
        {
            if (tableBumped)
            {
                wasBumped = true;
                bumpTimer = 0.2f;
            }

            if (wasBumped)
            {
                bumpTimer -= dt;
                // Add minor chaotic offset during bump recovery
                Vector3 bumpOffset = new Vector3(
                    Mathf.Sin(Time.time * 45f) * 0.005f,
                    Mathf.Cos(Time.time * 50f) * 0.005f,
                    Mathf.Sin(Time.time * 40f) * 0.005f
                );
                targetPos += bumpOffset;

                if (bumpTimer <= 0f)
                {
                    wasBumped = false;
                }
            }

            if (dt > 0f)
            {
                Velocity = (targetPos - Position) / dt;
            }
            Position = targetPos;
            Rotation = targetRot;
        }
    }
}
