using UnityEngine;

namespace XRGames.Chapter4.BioSphereTerraformer
{
    public class PropApple
    {
        public bool IsRegistered { get; private set; } = true;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public float RadiusMm { get; set; } = BioSphereConfig.AppleRadiusMm;
    }

    public class StemTwist
    {
        public float SpinOmegaDegS { get; set; } = 120.0f;
        public float AxisTiltDeg { get; set; } = 0.0f;
    }
}
