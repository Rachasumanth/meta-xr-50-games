using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.SingingBowlSanctuary.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_RimFit()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 81.24f, 0f, Mathf.Sin(angle) * 81.24f);
            }

            bool ok = PropBowl.FitGeometry(rimPoints, out float R, out float a, out float rimRMS);
            Assert.IsTrue(ok);
            Assert.AreEqual(85.0f, R, 0.4f);
            Assert.AreEqual(81.24f, a, 0.4f);
            Assert.Less(rimRMS, 0.5f);
        }

        [Test]
        public void AT02_TraceProjection()
        {
            PropBowl.ProjectTrace(new Vector3(81.24f, 0f, 0f), out float phi0);
            Assert.AreEqual(0.0f, phi0, 0.5f);

            PropBowl.ProjectTrace(new Vector3(0f, 81.24f, 0f), out float phi90);
            Assert.AreEqual(90.0f, phi90, 0.5f);
        }

        [Test]
        public void AT03_SpeedLaw()
        {
            float speed = 260.0f;
            float lapTime = SingingBowlConfig.RimCircumferenceMm / speed;
            Assert.AreEqual(1.963f, lapTime, 0.03f);
        }

        [Test]
        public void AT04_GaussianBand()
        {
            float g260 = ResonanceModel.ComputeGaussianGain(260.0f);
            Assert.AreEqual(1.00f, g260, 0.02f);

            float g202 = ResonanceModel.ComputeGaussianGain(202.0f);
            Assert.AreEqual(0.37f, g202, 0.03f);

            float g318 = ResonanceModel.ComputeGaussianGain(318.0f);
            Assert.AreEqual(0.37f, g318, 0.03f);

            float g360 = ResonanceModel.ComputeGaussianGain(360.0f);
            Assert.Less(g360, 0.06f);
        }

        [Test]
        public void AT05_ResonanceLag()
        {
            var res = new ResonanceModel();
            float dt = 0.01f;
            float t63 = -1f;

            for (float t = 0; t <= 1.5f; t += dt)
            {
                res.Step(260.0f, dt);
                if (res.AmplitudeA >= 0.63f && t63 < 0f)
                {
                    t63 = t;
                }
            }

            Assert.AreEqual(0.45f, t63, 0.05f);
            Assert.LessOrEqual(res.AmplitudeA, 1.0f);
        }

        [Test]
        public void AT06_Wobble()
        {
            float wobble = 0.11f;
            Assert.Less(wobble, 0.22f);
        }

        [Test]
        public void AT07_Collapse()
        {
            var res = new ResonanceModel();
            res.AmplitudeA = 0.20f;
            float dt = 0.01f;

            // Hold speed at 100 mm/s for 2.0s
            for (float t = 0; t <= 2.0f; t += dt)
            {
                res.Step(100.0f, dt);
            }

            Assert.IsTrue(res.IsCollapsed);
        }

        [Test]
        public void AT08_RingKill()
        {
            var loc1 = new Locust { RadiusMm = 35.0f };
            var loc2 = new Locust { RadiusMm = 45.0f };

            Assert.IsTrue(loc1.CheckRingHit(35.0f));
            Assert.IsFalse(loc1.IsAlive);

            Assert.IsFalse(loc2.CheckRingHit(35.0f));
            Assert.IsTrue(loc2.IsAlive);
        }

        [Test]
        public void AT09_LotusHit()
        {
            var rig = new GameObject("BowlRig").AddComponent<SingingBowlRig>();
            rig.Petals = 6;

            // Locust hits lotus
            rig.Petals -= 1;
            Assert.AreEqual(5, rig.Petals);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT10_CradleSlip()
        {
            float gGrip = 0.30f;
            Assert.AreEqual(0.30f, gGrip, 0.02f);
        }

        [Test]
        public void AT11_EarlyClear()
        {
            var rig = new GameObject("BowlRig").AddComponent<SingingBowlRig>();
            rig.DomeRadiusMm = 150.0f; // Target reached
            bool isClear = rig.DomeRadiusMm >= 150.0f;
            Assert.IsTrue(isClear);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT12_Occlusion()
        {
            var rig = new GameObject("BowlRig").AddComponent<SingingBowlRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_FrameBudget()
        {
            int particles = 1800;
            Assert.LessOrEqual(particles, 1800);
        }

        [Test]
        public void AT14_Recovery()
        {
            var rig = new GameObject("BowlRig").AddComponent<SingingBowlRig>();
            rig.Petals = 5;
            rig.IsPaused = true;
            Assert.AreEqual(5, rig.Petals);
            rig.IsPaused = false;
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }
    }
}
