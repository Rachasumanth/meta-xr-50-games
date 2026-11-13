using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public class PropTracker
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; } = Quaternion.identity;
    }
}
