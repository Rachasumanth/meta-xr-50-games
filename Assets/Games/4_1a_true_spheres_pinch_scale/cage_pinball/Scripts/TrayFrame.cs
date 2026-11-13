using UnityEngine;

namespace XRGames.Chapter4.CagePinball
{
    public class TrayFrame
    {
        public Vector3 Normal { get; private set; } = Vector3.up;
        public Vector3 Origin { get; private set; } = Vector3.zero;
        public bool IsReSeating { get; private set; }
        private float _reseatTimer;

        public void ReSolvePlane(Vector3 normal, Vector3 origin)
        {
            Normal = normal.normalized;
            Origin = origin;
            IsReSeating = true;
            _reseatTimer = 0.500f; // 500 ms re-seat
        }

        public void Step(float dt)
        {
            if (IsReSeating)
            {
                _reseatTimer -= dt;
                if (_reseatTimer <= 0f)
                {
                    IsReSeating = false;
                }
            }
        }
    }
}
