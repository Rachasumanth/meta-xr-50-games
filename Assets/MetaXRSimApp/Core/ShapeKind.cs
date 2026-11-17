using System;

namespace MetaXRSim.App
{
    /// <summary>
    /// Spatial families that the 50 mini-games are grouped into. The value is
    /// deliberately shared by the gesture recogniser, the real-object probe and
    /// the catalogue so a physical object and an air gesture resolve to the same
    /// family without any string marshalling.
    /// </summary>
    public enum ShapeKind
    {
        Unknown = 0,
        TrueSphere = 1,
        ProlateSpheroid = 2,
        OblateSpheroid = 3,
        Hemisphere = 4,
        DomedBowl = 5
    }

    public static class ShapeKindExtensions
    {
        public static bool IsSphere(this ShapeKind kind)
        {
            return kind == ShapeKind.TrueSphere;
        }

        public static bool IsSpheroid(this ShapeKind kind)
        {
            return kind == ShapeKind.ProlateSpheroid || kind == ShapeKind.OblateSpheroid;
        }

        public static bool IsDome(this ShapeKind kind)
        {
            return kind == ShapeKind.Hemisphere || kind == ShapeKind.DomedBowl;
        }

        public static string ToDisplayName(this ShapeKind kind)
        {
            switch (kind)
            {
                case ShapeKind.TrueSphere: return "True Sphere";
                case ShapeKind.ProlateSpheroid: return "Prolate Spheroid";
                case ShapeKind.OblateSpheroid: return "Oblate Spheroid";
                case ShapeKind.Hemisphere: return "Hemisphere";
                case ShapeKind.DomedBowl: return "Domed Bowl";
                default: return "Unknown";
            }
        }

        public static ShapeKind FromFamily(GameFamily family)
        {
            switch (family)
            {
                case GameFamily.SpherePinch:
                case GameFamily.SpherePalm:
                    return ShapeKind.TrueSphere;
                case GameFamily.Spheroid:
                    return ShapeKind.OblateSpheroid;
                case GameFamily.HemispherePinch:
                case GameFamily.HemisphereHand:
                    return ShapeKind.DomedBowl;
                default:
                    return ShapeKind.Unknown;
            }
        }
    }

    public enum GameFamily
    {
        SpherePinch = 0,
        SpherePalm = 1,
        Spheroid = 2,
        HemispherePinch = 3,
        HemisphereHand = 4
    }

    public enum ShapeInteractionMode
    {
        DrawInAir = 0,
        TwoHandScale = 1,
        PlaceOnRealObject = 2
    }

    public enum MiniGameObjective
    {
        Spin = 0,
        Balance = 1,
        Catch = 2,
        Stir = 3,
        Aim = 4,
        Defend = 5,
        Collect = 6,
        Steady = 7,
        Pour = 8,
        Race = 9
    }
}
