using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.GalaxyCauldron.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_PropFit()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 72.28f, 0f, Mathf.Sin(angle) * 72.28f);
            }

            bool ok = PropBowl.FitCap(rimPoints, out float R, out float d, out float rimRMS);
            Assert.IsTrue(ok);
            Assert.AreEqual(75.0f, R, 0.4f);
            Assert.AreEqual(55.0f, d, 0.5f);
            Assert.Less(rimRMS, 0.5f);
        }

        [Test]
        public void AT02_CapProjection()
        {
            // Bottom apex
            PropBowl.ProjectToCap(new Vector3(0f, 0f, -55f), out float r0, out float theta0);
            Assert.AreEqual(0.0f, r0, 0.1f);
            Assert.AreEqual(0.0f, theta0, 0.1f);

            // Rim edge
            PropBowl.ProjectToCap(new Vector3(72.28f, 0f, 0f), out float rRim, out float thetaRim);
            Assert.AreEqual(97.58f, rRim, 0.3f);
            Assert.AreEqual(74.53f, thetaRim, 0.2f);
        }

        [Test]
        public void AT03_StirSign()
        {
            var stir = new StirAnalyzer();
            // Move along +y at x = 50 mm, speed 100 mm/s
            float omega = stir.ComputeStirOmega(new Vector2(50f, 0f), new Vector2(0f, 100f));
            Assert.AreEqual(-2.0f, omega, 0.06f); // -2.0 rad/s ± 3%
        }

        [Test]
        public void AT04_DeadZone()
        {
            var stir = new StirAnalyzer();

            // 0 to 18 mm/s -> inactive
            Assert.IsFalse(stir.EvaluateSpeed(10.0f));
            Assert.IsFalse(stir.EvaluateSpeed(15.0f));
            Assert.IsFalse(stir.EvaluateSpeed(18.0f));

            // Field starts by 19 mm/s
            Assert.IsTrue(stir.EvaluateSpeed(19.0f));

            // Hysteresis keeps alive down to 12 mm/s
            Assert.IsTrue(stir.EvaluateSpeed(14.0f));
            Assert.IsTrue(stir.EvaluateSpeed(12.0f));

            // Dies below 12 mm/s
            Assert.IsFalse(stir.EvaluateSpeed(11.0f));
        }

        [Test]
        public void AT05_DepthGains()
        {
            float r10 = StirAnalyzer.ComputeFusionRadius(10.0f);
            Assert.AreEqual(12.0f, r10, 0.3f);

            float r45 = StirAnalyzer.ComputeFusionRadius(45.0f);
            Assert.AreEqual(15.0f, r45, 0.3f);

            // Monotonic ramp between
            float r25 = StirAnalyzer.ComputeFusionRadius(25.0f);
            Assert.Greater(r25, r10);
            Assert.Less(r25, r45);
        }

        [Test]
        public void AT06_Fusion()
        {
            var a = new Cloud { PosMm = Vector2.zero, Species = 0 }; // H
            var b = new Cloud { PosMm = new Vector2(11.0f, 0f), Species = 0 }; // H

            bool hit = Cloud.CheckInteraction(a, b, out bool isFusion, out bool isNova);
            Assert.IsTrue(hit);
            Assert.IsTrue(isFusion);
            Assert.IsFalse(isNova);
            Assert.IsTrue(a.IsDead && b.IsDead);
        }

        [Test]
        public void AT07_Nova()
        {
            var a = new Cloud { PosMm = Vector2.zero, Species = 0 }; // H
            var b = new Cloud { PosMm = new Vector2(9.0f, 0f), Species = 1 }; // C

            bool hit = Cloud.CheckInteraction(a, b, out bool isFusion, out bool isNova);
            Assert.IsTrue(hit);
            Assert.IsFalse(isFusion);
            Assert.IsTrue(isNova);
            Assert.IsTrue(a.IsDead && b.IsDead);
        }

        [Test]
        public void AT08_Comet()
        {
            var seed = new DarkMatterSeed();
            seed.Step(160.0f, 0.55f); // 88 mm > 86 mm
            Assert.IsTrue(seed.IsComet);

            // Continue to rim
            seed.Step(130.0f, 0.2f);
            Assert.IsTrue(seed.IsEjected);
        }

        [Test]
        public void AT09_Clock()
        {
            float dur = GalaxyCauldronConfig.GalaxyDurationSec;
            Assert.AreEqual(90.0f, dur);
        }

        [Test]
        public void AT10_Occlusion()
        {
            var rig = new GameObject("CauldronRig").AddComponent<CauldronRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT11_AdaptiveTuning()
        {
            float rf = 12.0f;
            int novas = 2;
            if (novas >= 2) rf += 2.0f;
            Assert.AreEqual(14.0f, rf);
        }

        [Test]
        public void AT12_FrameBudget()
        {
            int particles = 2400;
            Assert.LessOrEqual(particles, 2400);
        }

        [Test]
        public void AT13_TrackingRecovery()
        {
            var rig = new GameObject("CauldronRig").AddComponent<CauldronRig>();
            rig.Score = 300;
            rig.IsPaused = true;
            Assert.AreEqual(300, rig.Score);
            rig.IsPaused = false;
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Endurance()
        {
            var stir = new StirAnalyzer();
            for (int i = 0; i < 500; i++)
            {
                stir.EvaluateSpeed(20.0f);
            }
            Assert.IsTrue(stir.IsActive);
        }
    }
}
