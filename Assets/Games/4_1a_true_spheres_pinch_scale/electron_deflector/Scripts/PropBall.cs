using UnityEngine;

namespace XRGames.Chapter4.ElectronDeflector
{
    public class PropBall
    {
        public Vector3 Center { get; private set; }
        public Vector3 PoleAxis { get; private set; } = Vector3.up;
        public bool IsRegistered { get; private set; }

        public bool Register(Vector3 center, Vector3 poleAxis)
        {
            Center = center;
            PoleAxis = poleAxis.normalized;
            IsRegistered = true;
            return true;
        }
    }
}
