using UnityEngine;

namespace XRGames.Chapter4.ScarabCanyon
{
    public class MarbleTracker
    {
        public Vector3 Center { get; private set; }
        public float S { get; private set; }
        public float SDot { get; private set; }
        public bool IsRegistered { get; private set; }

        public bool Register(Vector3 pos)
        {
            Center = pos;
            IsRegistered = true;
            return true;
        }

        public void SetGrooveState(float s, float sdot)
        {
            S = s;
            SDot = sdot;
        }
    }
}
