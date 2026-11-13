using UnityEngine;

namespace XRGames.Chapter4.MysticSeer
{
    public class PropTracker
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public bool IsAtRestOnTable { get; set; } = true;

        public void CheckRestDisc(Vector3 newPos)
        {
            float dist = Vector3.Distance(Position, newPos);
            IsAtRestOnTable = (dist < 0.005f); // within 5 mm
            Position = newPos;
        }
    }
}
