using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class NozzleFrame
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Velocity { get; private set; }

        private Vector3 _lastPosition;
        private bool _initialized;

        public void Update(Vector3 targetPos, Quaternion targetRot, float dt, bool tableBumped)
        {
            if (tableBumped)
            {
                // Blend or shock absorption on bump
                targetPos += UnityEngine.Random.insideUnitSphere * 0.01f;
            }

            if (!_initialized)
            {
                Position = targetPos;
                Rotation = targetRot;
                _lastPosition = targetPos;
                _initialized = true;
                Velocity = Vector3.zero;
                return;
            }

            if (dt > 0.0001f)
            {
                Velocity = (targetPos - _lastPosition) / dt;
            }

            Position = targetPos;
            Rotation = targetRot;
            _lastPosition = targetPos;
        }
    }
}
