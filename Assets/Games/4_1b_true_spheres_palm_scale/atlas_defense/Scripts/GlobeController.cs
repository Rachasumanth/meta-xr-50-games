using UnityEngine;

namespace XRGames.Chapter4.AtlasDefense
{
    public class GlobeController
    {
        public Vector3 OffsetMm { get; private set; }
        public Quaternion Rotation { get; private set; } = Quaternion.identity;

        public void SetOffset(Vector3 rawOffsetMm)
        {
            float limit = AtlasDefenseConfig.PlanetOffsetClampMm;
            OffsetMm = new Vector3(
                Mathf.Clamp(rawOffsetMm.x, -limit, limit),
                Mathf.Clamp(rawOffsetMm.y, -limit, limit),
                Mathf.Clamp(rawOffsetMm.z, -limit, limit)
            );
        }

        public void SetRotation(Quaternion rot)
        {
            Rotation = rot;
        }

        public Vector3 GetShieldAxisV()
        {
            return Rotation * Vector3.forward;
        }
    }
}
