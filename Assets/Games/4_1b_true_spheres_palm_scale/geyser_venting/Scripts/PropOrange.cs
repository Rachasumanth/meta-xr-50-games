using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public class PropOrange
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public float RadiusMm { get; set; } = GeyserVentingConfig.OrangeRadiusMm;
        public bool IsCovered { get; set; } = false;

        public void Rotate(Quaternion deltaRot)
        {
            Rotation = (deltaRot * Rotation).normalized;
        }
    }
}
