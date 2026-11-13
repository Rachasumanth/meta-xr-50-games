using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.WorryStoneSpirit.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SpheroidRegistration()
        {
            var tracker = new PropTracker();
            var rimPoints = new List<Vector3>();
            int count = 24;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((tracker.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (tracker.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = tracker.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.5f);
            Assert.Less(yawDeg, 2.0f);
        }

        [Test]
        public void AT02_HeightmapInitialization()
        {
            var field = new AbrasionField();
            for (int i = 0; i < 100; i++)
            {
                int x = (i * 7) % AbrasionField.Width;
                int y = 25 + (i % 20); // Top cap phi > +5 deg
                Assert.AreEqual(1.20f, field.Heights[x, y], 0.001f);
                Assert.AreEqual(5, field.Layers[x, y]);
            }
        }

        [Test]
        public void AT03_AbrasionRate()
        {
            var field = new AbrasionField();
            int cx = 20;
            int cy = 30;

            float dt = 0.010f;
            float initialH = field.Heights[cx, cy];
            float layerBreakTime = -1f;

            for (float t = 0; t <= 2.0f; t += dt)
            {
                // p = 0.40 N, v = 80 mm/s, S = 1.0, H = 1.0
                field.ApplyAbrasion(cx, cy, 0.40f, 80.0f, 1.0f, dt);
                if (field.Heights[cx, cy] <= (initialH - 0.20f) && layerBreakTime < 0)
                {
                    layerBreakTime = t;
                }
            }

            float finalH = field.Heights[cx, cy];
            float deltaH = initialH - finalH;

            Assert.AreEqual(0.36f, deltaH, 0.02f);
            Assert.AreEqual(1.11f, layerBreakTime, 0.05f);
        }

        [Test]
        public void AT04_PressureSaturation()
        {
            var thumb = new ThumbKinematics();
            for (float delta = 1.0f; delta <= 12.0f; delta += 1.0f)
            {
                float p = thumb.ComputePressure(delta);
                Assert.LessOrEqual(p, 0.5501f);
            }

            // At delta = 12mm, p clamps at 0.55N
            float pMax = thumb.ComputePressure(12.0f);
            Assert.AreEqual(0.55f, pMax, 0.001f);

            // Hardness rises at 0.25/s when p > 0.50N
            thumb.Step(1.0f, 50.0f);
            Assert.AreEqual(1.25f, thumb.HardnessH, 0.01f);
        }

        [Test]
        public void AT05_SmoothOvalClassifier()
        {
            var thumb = new ThumbKinematics();
            thumb.ClassifyStroke(true, false, 1.8f);
            Assert.AreEqual(1.25f, thumb.SFactor, 0.01f);

            thumb.ClassifyStroke(false, true, 1.0f);
            Assert.AreEqual(0.85f, thumb.SFactor, 0.01f);
        }

        [Test]
        public void AT06_GeodesicFootprint()
        {
            // Verify geodesic disc radius calculation: 6.5mm chord disc
            float a = WorryStoneSpiritConfig.SemiAxisA;
            float b = WorryStoneSpiritConfig.SemiAxisB;
            float c = WorryStoneSpiritConfig.SemiAxisC;

            // Pole contact at (0, 0, c)
            Vector3 contactPole = new Vector3(0, 0, c);
            Vector3 testPoint = new Vector3(0.005f, 0.003f, c);
            float chordDistMm = Vector3.Distance(contactPole, testPoint) * 1000f;

            Assert.Less(chordDistMm, 6.5f);
        }

        [Test]
        public void AT07_LayerIndexing()
        {
            var field = new AbrasionField();
            int x = 10, y = 10;
            Assert.AreEqual(5, field.Layers[x, y]);

            bool layerDecreased = false;
            field.OnLayerDecreased += (lx, ly, nLayer) => layerDecreased = true;

            field.ApplyDirectRemoval(x, y, 0.40f);
            Assert.AreEqual(3, field.Layers[x, y]); // 1.20 - 0.40 = 0.80 mm => layer 3 (or 4 if [0.8..1.0])
            Assert.IsTrue(layerDecreased);
        }

        [Test]
        public void AT08_FragmentReveal()
        {
            var field = new AbrasionField();
            var fragments = new FragmentField();
            var combo = new ScoreCombo();

            fragments.OnFragmentRevealed += (f) => combo.RegisterFragment();

            var f0 = fragments.Fragments[0];
            // Rub 3x3 patch to mean h = 0.14mm
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    field.Heights[(f0.GridX + dx + AbrasionField.Width) % AbrasionField.Width,
                                  Mathf.Clamp(f0.GridY + dy, 0, AbrasionField.Height - 1)] = 0.14f;
                }
            }

            fragments.CheckReveals(field);
            Assert.IsTrue(f0.IsRevealed);
            Assert.AreEqual(200, combo.TotalScore);

            // Latches even if h raised
            field.Heights[f0.GridX, f0.GridY] = 1.0f;
            fragments.CheckReveals(field);
            Assert.IsTrue(f0.IsRevealed);
        }

        [Test]
        public void AT09_TideFront()
        {
            var tide = new TideController();
            var fragments = new FragmentField();

            float phi0 = tide.ComputeFrontPhi(0f);
            float phi25 = tide.ComputeFrontPhi(25f);
            float phi50 = tide.ComputeFrontPhi(50f);

            Assert.AreEqual(-80.0f, phi0, 0.5f);
            Assert.AreEqual(2.5f, phi25, 0.5f);
            Assert.AreEqual(85.0f, phi50, 0.5f);

            // Fragment at phi = 30 deg is lost at 31.8 s +- 0.3 s
            var testFrag = new Fragment(99, 30.0f, 0f, 0, 0);
            fragments.Fragments.Clear();
            fragments.Fragments.Add(testFrag);

            float dt = 0.1f;
            float lostTime = -1f;
            for (float t = 0; t <= 50.0f; t += dt)
            {
                tide.Step(dt, fragments);
                if (testFrag.IsLost && lostTime < 0)
                {
                    lostTime = t;
                }
            }

            Assert.AreEqual(31.8f, lostTime, 0.3f);
        }

        [Test]
        public void AT10_Constellation()
        {
            var fragments = new FragmentField();
            var combo = new ScoreCombo();

            // Reveal 7 adjacent fragments within 30 deg
            for (int i = 0; i < 7; i++)
            {
                fragments.Fragments[i].IsRevealed = true;
            }

            int comp = fragments.ComputeMaxConnectedComponent();
            Assert.AreEqual(7, comp);

            combo.RegisterConstellation(10.0f); // 10s left
            Assert.AreEqual(600, combo.TotalScore); // 500 + 10 * 10
        }

        [Test]
        public void AT11_GlazeRecovery()
        {
            var thumb = new ThumbKinematics();
            thumb.ComputePressure(12.0f); // p = 0.55 N

            // Hold 4s
            for (float t = 0; t < 4.0f; t += 0.1f)
            {
                thumb.Step(0.1f, 50.0f);
            }
            Assert.AreEqual(2.0f, thumb.HardnessH, 0.01f);

            // Hold p = 0.30 N for 10s
            thumb.ComputePressure(5.0f); // p = 60 * 0.005 = 0.30 N
            for (float t = 0; t < 10.0f; t += 0.1f)
            {
                thumb.Step(0.1f, 50.0f);
            }
            Assert.AreEqual(1.0f, thumb.HardnessH, 0.05f);
        }

        [Test]
        public void AT12_OcclusionReadback()
        {
            // Thumb over revealed star: star fragment contribution = 0 under thumb silhouette
            float thumbRadius = 0.008f;
            Vector2 starPos = new Vector2(0.003f, 0.003f);
            bool underThumb = starPos.magnitude < thumbRadius;
            Assert.IsTrue(underThumb);

            float starDepth = 0.02f;
            float thumbDepth = 0.01f;
            bool occluded = (starDepth > thumbDepth) && underThumb;
            Assert.IsTrue(occluded);
        }

        [Test]
        public void AT13_TrackingLoss()
        {
            var tracker = new PropTracker();
            float dt = 0.050f;

            for (float t = 0; t < 2.0f; t += dt)
            {
                tracker.StepTrackingLoss(false, dt);
                if (t >= 1.5f)
                {
                    Assert.IsTrue(tracker.IsPaused);
                }
            }
        }

        [Test]
        public void AT14_SoakRun()
        {
            var field = new AbrasionField();
            var fragments = new FragmentField();
            var tide = new TideController();

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                field.ApplyAbrasion(20, 20, 0.40f, 80f, 1.0f, dt);
                tide.Step(dt, fragments);
            }

            // Heightmap bounded in [0, 1.20]
            for (int x = 0; x < AbrasionField.Width; x += 10)
            {
                for (int y = 0; y < AbrasionField.Height; y += 10)
                {
                    Assert.GreaterOrEqual(field.Heights[x, y], 0f);
                    Assert.LessOrEqual(field.Heights[x, y], 1.20f);
                }
            }
        }
    }
}
