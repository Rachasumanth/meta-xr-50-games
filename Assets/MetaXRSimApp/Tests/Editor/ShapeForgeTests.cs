using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MetaXRSim.App;

namespace MetaXRSim.Tests
{
    public class ShapeForgeTests
    {
        private static List<Vector3> Circle(Vector3 c, Vector3 right, Vector3 up, float rx, float ry, int steps, bool close)
        {
            var pts = new List<Vector3>(steps + 1);
            int count = close ? steps + 1 : steps;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)steps;
                float a = t * Mathf.PI * 2f;
                pts.Add(c + right * (Mathf.Cos(a) * rx) + up * (Mathf.Sin(a) * ry));
            }
            return pts;
        }

        private static List<Vector3> Arc(Vector3 c, Vector3 right, Vector3 up, float rx, float ry, int steps, bool cup)
        {
            var pts = new List<Vector3>(steps);
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)(steps - 1);
                float a = cup ? t * Mathf.PI : Mathf.PI - t * Mathf.PI;
                float y = Mathf.Sin(a) * ry * (cup ? -1f : 1f);
                pts.Add(c + right * (Mathf.Cos(a) * rx) + up * y);
            }
            return pts;
        }

        private static ShapeSignature Analyze(List<Vector3> pts)
        {
            var r = new ShapeGestureRecognizer();
            r.Begin();
            for (int i = 0; i < pts.Count; i++) r.AddSample(pts[i], i * 0.012f);
            return r.Complete(Vector3.zero);
        }

        private static ShapeSignature Synthetic(ShapeKind kind, float diameter)
        {
            var sig = ShapeSignature.Unknown(Vector3.zero);
            sig.Kind = kind;
            sig.DiameterMeters = diameter;
            sig.SizeMeters = Vector3.one * diameter;
            sig.Confidence = 0.85f;
            sig.PointCount = 64;
            sig.AspectRatio = kind == ShapeKind.ProlateSpheroid || kind == ShapeKind.OblateSpheroid ? 1.5f : 1f;
            sig.Closure = 1f;
            sig.SweepDegrees = 360f;
            sig.PrincipalAxisWorld = Vector3.up;
            return sig;
        }

        [Test]
        public void Test01_Catalogue_Count_And_Families()
        {
            Assert.AreEqual(50, GameCatalog.All.Count);
            var families = new Dictionary<GameFamily, int>();
            var slugs = new HashSet<string>();
            foreach (var g in GameCatalog.All)
            {
                Assert.IsNotNull(g);
                Assert.IsTrue(slugs.Add(g.Slug), "duplicate slug " + g.Slug);
                if (!families.ContainsKey(g.Family)) families[g.Family] = 0;
                families[g.Family]++;
                Assert.IsFalse(string.IsNullOrEmpty(g.Title));
                Assert.IsFalse(string.IsNullOrEmpty(g.Instruction));
            }
            Assert.AreEqual(5, families.Count);
            foreach (var kv in families) Assert.AreEqual(10, kv.Value, "family " + kv.Key);
        }

        [Test]
        public void Test02_All_Rig_Types_Resolve_To_MonoBehaviours()
        {
            foreach (var g in GameCatalog.All)
            {
                var t = GameCatalog.GetRigType(g.Slug);
                Assert.IsNotNull(t, "missing type for " + g.Slug);
                Assert.IsTrue(typeof(MonoBehaviour).IsAssignableFrom(t), g.Slug + " not a MonoBehaviour");
                Assert.AreEqual(g.RigTypeName, t.FullName, "catalogue string mismatch for " + g.Slug);
            }
        }

        [Test]
        public void Test03_Circle_Is_True_Sphere()
        {
            var pts = Circle(Vector3.zero, Vector3.right, Vector3.up, 0.05f, 0.05f, 70, true);
            var sig = Analyze(pts);
            Assert.AreEqual(ShapeKind.TrueSphere, sig.Kind);
            Assert.That(sig.DiameterMeters, Is.InRange(0.085f, 0.115f));
            Assert.Greater(sig.Confidence, 0.4f);
        }

        [Test]
        public void Test04_Tall_Oval_Is_Prolate()
        {
            var pts = Circle(Vector3.zero, Vector3.right, Vector3.up, 0.032f, 0.058f, 70, true);
            var sig = Analyze(pts);
            Assert.AreEqual(ShapeKind.ProlateSpheroid, sig.Kind);
            Assert.Greater(sig.AspectRatio, 1.2f);
        }

        [Test]
        public void Test05_Wide_Oval_Is_Oblate()
        {
            var pts = Circle(Vector3.zero, Vector3.right, Vector3.up, 0.07f, 0.032f, 70, true);
            var sig = Analyze(pts);
            Assert.AreEqual(ShapeKind.OblateSpheroid, sig.Kind);
        }

        [Test]
        public void Test06_Upward_Arc_Is_Hemisphere()
        {
            var pts = Arc(Vector3.zero, Vector3.right, Vector3.up, 0.06f, 0.06f, 60, false);
            var sig = Analyze(pts);
            Assert.AreEqual(ShapeKind.Hemisphere, sig.Kind);
            Assert.That(sig.SweepDegrees, Is.InRange(120f, 360f));
        }

        [Test]
        public void Test07_Downward_Arc_Is_Domed_Bowl()
        {
            var pts = Arc(Vector3.zero, Vector3.right, Vector3.up, 0.06f, 0.06f, 60, true);
            var sig = Analyze(pts);
            Assert.AreEqual(ShapeKind.DomedBowl, sig.Kind);
        }

        [Test]
        public void Test08_Too_Few_Points_Is_Unknown()
        {
            var pts = Circle(Vector3.zero, Vector3.right, Vector3.up, 0.05f, 0.05f, 10, true);
            var sig = Analyze(pts);
            Assert.AreEqual(ShapeKind.Unknown, sig.Kind);
        }

        [Test]
        public void Test09_Duplicate_Samples_Are_Rejected_But_Time_Is_Measured()
        {
            var r = new ShapeGestureRecognizer();
            r.Begin();
            Assert.IsTrue(r.AddSample(Vector3.zero, 0f));
            Assert.IsFalse(r.AddSample(Vector3.zero, 0.01f));
            Assert.AreEqual(1, r.PointCount);
        }

        [Test]
        public void Test10_Matchmaker_Small_Sphere_Is_Pinch_Family()
        {
            var m = new GameMatchmaker().Match(Synthetic(ShapeKind.TrueSphere, 0.060f), RealObjectProfile.None(Vector3.zero), 7);
            Assert.IsTrue(m.HasGame);
            Assert.AreEqual(GameFamily.SpherePinch, m.ResolvedFamily);
            Assert.AreEqual(GameFamily.SpherePinch, m.Game.Family);
        }

        [Test]
        public void Test11_Matchmaker_Large_Sphere_Is_Palm_Family()
        {
            var m = new GameMatchmaker().Match(Synthetic(ShapeKind.TrueSphere, 0.125f), RealObjectProfile.None(Vector3.zero), 7);
            Assert.AreEqual(GameFamily.SpherePalm, m.ResolvedFamily);
            Assert.AreEqual(GameFamily.SpherePalm, m.Game.Family);
        }

        [Test]
        public void Test12_Matchmaker_Spheroid_Uses_Spheroid_Family()
        {
            var m = new GameMatchmaker().Match(Synthetic(ShapeKind.ProlateSpheroid, 0.150f), RealObjectProfile.None(Vector3.zero), 7);
            Assert.AreEqual(GameFamily.Spheroid, m.ResolvedFamily);
            Assert.AreEqual(ShapeKind.ProlateSpheroid, m.Game.Shape);
        }

        [Test]
        public void Test13_Matchmaker_Dome_Size_Splits_Pinch_And_Hand()
        {
            var small = new GameMatchmaker().Match(Synthetic(ShapeKind.DomedBowl, 0.100f), RealObjectProfile.None(Vector3.zero), 7);
            Assert.AreEqual(GameFamily.HemispherePinch, small.ResolvedFamily);
            var large = new GameMatchmaker().Match(Synthetic(ShapeKind.DomedBowl, 0.260f), RealObjectProfile.None(Vector3.zero), 7);
            Assert.AreEqual(GameFamily.HemisphereHand, large.ResolvedFamily);
        }

        [Test]
        public void Test14_Matchmaker_Uses_Real_Object_Label_And_Size()
        {
            var obj = new RealObjectProfile
            {
                HasObject = true,
                Label = "TABLE",
                SizeMeters = new Vector3(1.2f, 0.05f, 0.7f),
                PositionWorld = new Vector3(0f, 0.75f, 0.5f),
                RotationWorld = Quaternion.identity,
                SurfaceNormalWorld = Vector3.up,
                Source = RealObjectSource.SceneVolume,
                Confidence = 0.9f
            };
            Assert.AreEqual(ShapeKind.DomedBowl, obj.InferShape());
            var m = new GameMatchmaker().Match(ShapeSignature.Unknown(Vector3.zero), obj, 3);
            Assert.AreEqual(GameFamily.HemisphereHand, m.ResolvedFamily);
            Assert.IsTrue(m.Game.RealObjectKeywords != null && m.Game.RealObjectKeywords.Length > 0);
        }

        [Test]
        public void Test15_Matchmaker_Is_Deterministic_For_A_Seed()
        {
            var sig = Synthetic(ShapeKind.TrueSphere, 0.060f);
            var a = new GameMatchmaker().Match(sig, RealObjectProfile.None(Vector3.zero), 42);
            var b = new GameMatchmaker().Match(sig, RealObjectProfile.None(Vector3.zero), 42);
            Assert.AreEqual(a.Game.Slug, b.Game.Slug);
        }

        [Test]
        public void Test16_Each_Family_Is_Reachable_From_Its_Median_Size()
        {
            var mm = new GameMatchmaker();
            var cases = new[]
            {
                new { Kind = ShapeKind.TrueSphere, Size = 0.055f, Family = GameFamily.SpherePinch },
                new { Kind = ShapeKind.TrueSphere, Size = 0.130f, Family = GameFamily.SpherePalm },
                new { Kind = ShapeKind.OblateSpheroid, Size = 0.150f, Family = GameFamily.Spheroid },
                new { Kind = ShapeKind.DomedBowl, Size = 0.100f, Family = GameFamily.HemispherePinch },
                new { Kind = ShapeKind.DomedBowl, Size = 0.260f, Family = GameFamily.HemisphereHand },
            };
            foreach (var c in cases)
            {
                var m = mm.Match(Synthetic(c.Kind, c.Size), RealObjectProfile.None(Vector3.zero), 1);
                Assert.AreEqual(c.Family, m.ResolvedFamily, c.Kind + " " + c.Size);
            }
        }

        [Test]
        public void Test17_Real_Object_Shape_Inference()
        {
            Assert.AreEqual(ShapeKind.TrueSphere, MakeObj("BALL", 0.10f, 0.10f, 0.10f).InferShape());
            Assert.AreEqual(ShapeKind.ProlateSpheroid, MakeObj("BOTTLE", 0.07f, 0.25f, 0.07f).InferShape());
            Assert.AreEqual(ShapeKind.OblateSpheroid, MakeObj("COUCH", 1.8f, 0.6f, 0.8f).InferShape());
            Assert.AreEqual(ShapeKind.DomedBowl, MakeObj("BOWL", 0.20f, 0.07f, 0.20f).InferShape());
        }

        private static RealObjectProfile MakeObj(string label, float x, float y, float z)
        {
            return new RealObjectProfile
            {
                HasObject = true,
                Label = label,
                SizeMeters = new Vector3(x, y, z),
                PositionWorld = Vector3.zero,
                RotationWorld = Quaternion.identity,
                SurfaceNormalWorld = Vector3.up,
                Source = RealObjectSource.SceneVolume,
                Confidence = 0.9f
            };
        }

        [Test]
        public void Test18_Default_Signature_For_Every_Game_Is_Usable()
        {
            foreach (var g in GameCatalog.All)
            {
                var sig = new ShapeSignature
                {
                    Kind = g.Shape,
                    DiameterMeters = g.NominalSizeMeters,
                    Confidence = 0.75f,
                    PointCount = 64
                };
                Assert.IsTrue(sig.IsUsable, g.Slug);
            }
        }
    }
}
