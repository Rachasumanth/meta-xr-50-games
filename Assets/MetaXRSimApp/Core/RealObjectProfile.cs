using System;
using UnityEngine;

namespace MetaXRSim.App
{
    public enum RealObjectSource
    {
        None = 0,
        SceneVolume = 1,
        ScenePlane = 2,
        SurfaceRaycast = 3,
        HandGesture = 4,
        Manual = 5
    }

    /// <summary>
    /// The physical object the player wants to "convert" into a mini-game.
    /// The struct is deliberately engine-light: it can be produced by the OVR
    /// scene probe at runtime or by the gesture itself when no scene model is
    /// available.
    /// </summary>
    public struct RealObjectProfile
    {
        public bool HasObject;
        public string Label;
        public Vector3 SizeMeters;
        public Vector3 PositionWorld;
        public Quaternion RotationWorld;
        public Vector3 SurfaceNormalWorld;
        public RealObjectSource Source;
        public string AnchorUuid;
        public float Confidence;

        public float MaxDimension => Mathf.Max(SizeMeters.x, Mathf.Max(SizeMeters.y, SizeMeters.z));
        public float MinDimension => Mathf.Min(SizeMeters.x, Mathf.Min(SizeMeters.y, SizeMeters.z));
        public float VolumeM3 => Mathf.Max(0f, SizeMeters.x * SizeMeters.y * SizeMeters.z);

        public bool IsValid => HasObject && MaxDimension >= 0.01f && MaxDimension <= 4f;

        public static RealObjectProfile None(Vector3 fallbackPosition)
        {
            return new RealObjectProfile
            {
                HasObject = false,
                Label = "Open space",
                SizeMeters = new Vector3(0.10f, 0.10f, 0.10f),
                PositionWorld = fallbackPosition,
                RotationWorld = Quaternion.identity,
                SurfaceNormalWorld = Vector3.up,
                Source = RealObjectSource.None,
                AnchorUuid = string.Empty,
                Confidence = 0f
            };
        }

        /// <summary>
        /// Converts physical proportions + semantic label into the closest
        /// spherical family. This is the bridge between "real object" and
        /// "mini-game conversion".
        /// </summary>
        public ShapeKind InferShape()
        {
            if (!HasObject) return ShapeKind.Unknown;

            string l = (Label ?? string.Empty).ToLowerInvariant();
            if (l.Contains("bowl") || l.Contains("vase") || l.Contains("cup") ||
                l.Contains("pot") || l.Contains("sink") || l.Contains("basin"))
                return ShapeKind.DomedBowl;
            if (l.Contains("ball") || l.Contains("sphere") || l.Contains("orb"))
                return ShapeKind.TrueSphere;
            if (l.Contains("bottle") || l.Contains("can") || l.Contains("cylinder"))
                return ShapeKind.ProlateSpheroid;
            if (l.Contains("table") || l.Contains("desk") || l.Contains("shelf") ||
                l.Contains("floor") || l.Contains("rug") || l.Contains("bed"))
                return ShapeKind.DomedBowl;
            if (l.Contains("couch") || l.Contains("sofa") || l.Contains("chair"))
                return ShapeKind.OblateSpheroid;

            // Volume proportions: Y is the world-up axis.
            float horizontal = Mathf.Max(0.001f, Mathf.Max(SizeMeters.x, SizeMeters.z));
            float heightRatio = SizeMeters.y / horizontal;
            float flatness = Mathf.Min(SizeMeters.x, SizeMeters.z) / Mathf.Max(0.001f, Mathf.Max(SizeMeters.x, SizeMeters.z));

            if (heightRatio < 0.42f) return ShapeKind.OblateSpheroid;
            if (heightRatio > 1.32f) return ShapeKind.ProlateSpheroid;
            if (flatness < 0.55f) return ShapeKind.OblateSpheroid;
            return ShapeKind.TrueSphere;
        }

        public float SizeTierMeters()
        {
            return MaxDimension;
        }

        public string Describe()
        {
            if (!HasObject) return "No real object selected";
            return string.Format("{0}  {1:0} x {2:0} x {3:0} mm  [{4}]",
                string.IsNullOrEmpty(Label) ? "Object" : Label,
                SizeMeters.x * 1000f, SizeMeters.y * 1000f, SizeMeters.z * 1000f,
                Source);
        }
    }
}
