using UnityEngine;

namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class PropMarble
    {
        public Vector3 Center { get; private set; }
        public Vector3 HoldAxis { get; private set; } = Vector3.up;
        public bool IsRegistered { get; private set; }

        public bool Register(Vector3 pos, Vector3 pinchAxis)
        {
            Center = pos;
            HoldAxis = pinchAxis.normalized;
            IsRegistered = true;
            return true;
        }
    }
}
