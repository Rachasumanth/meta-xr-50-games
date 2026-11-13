using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public class PropTracker
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public float SeamConfidence { get; set; } = 1.0f;
    }
}
