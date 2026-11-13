using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor
{
    public class PropTracker
    {
        public bool IsRegistered { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;

        public void SetPose(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
            IsRegistered = true;
        }
    }
}
