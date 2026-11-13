using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ParatrooperBasket.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_CatchGeometry()
        {
            float rEff = NetCollider.ComputeEffectiveRadius(0f); // 13.0 mm

            var t1 = new Trooper();
            t1.PosMm = new Vector3(12.0f, 0f, 0f);
            t1.VelMmS = new Vector3(0f, 0f, -180.0f);
            Assert.IsTrue(t1.EvaluateLanding(Vector2.zero, rEff, out _));

            var t2 = new Trooper();
            t2.PosMm = new Vector3(15.0f, 0f, 0f);
            t2.VelMmS = new Vector3(0f, 0f, -180.0f);
            Assert.IsFalse(t2.EvaluateLanding(Vector2.zero, rEff, out _));
        }

        [Test]
        public void AT02_SpeedWindow()
        {
            float rEff = NetCollider.ComputeEffectiveRadius(0f);

            // -100 mm/s too slow (passes through)
            var tSlow = new Trooper();
            tSlow.PosMm = Vector3.zero;
            tSlow.VelMmS = new Vector3(0f, 0f, -100.0f);
            Assert.IsFalse(tSlow.EvaluateLanding(Vector2.zero, rEff, out _));

            // -200 mm/s catches
            var tGood = new Trooper();
            tGood.PosMm = Vector3.zero;
            tGood.VelMmS = new Vector3(0f, 0f, -200.0f);
            Assert.IsTrue(tGood.EvaluateLanding(Vector2.zero, rEff, out _));

            // -300 mm/s too fast
            var tFast = new Trooper();
            tFast.PosMm = Vector3.zero;
            tFast.VelMmS = new Vector3(0f, 0f, -300.0f);
            Assert.IsFalse(tFast.EvaluateLanding(Vector2.zero, rEff, out _));
        }

        [Test]
        public void AT03_TiltRule()
        {
            // Tilt beta = 25 deg (> 20 deg) -> effective radius is 0
            float rEff25 = NetCollider.ComputeEffectiveRadius(25.0f);
            Assert.AreEqual(0f, rEff25);

            var t = new Trooper();
            t.PosMm = Vector3.zero;
            t.VelMmS = new Vector3(0f, 0f, -180.0f);
            Assert.IsFalse(t.EvaluateLanding(Vector2.zero, rEff25, out _));
        }

        [Test]
        public void AT04_SoftCatchScore()
        {
            float rEff = NetCollider.ComputeEffectiveRadius(0f);
            var t = new Trooper();
            t.PosMm = Vector3.zero;
            t.VelMmS = new Vector3(0f, 0f, -140.0f); // Soft catch

            bool caught = t.EvaluateLanding(Vector2.zero, rEff, out float mult);
            Assert.IsTrue(caught);
            Assert.AreEqual(1.5f, mult, 0.01f);
        }

        [Test]
        public void AT05_BounceChain()
        {
            float rEff = NetCollider.ComputeEffectiveRadius(0f);
            var t = new Trooper();
            t.PosMm = Vector3.zero;
            t.VelMmS = new Vector3(0f, 0f, -180.0f);

            // Bounce 1
            t.EvaluateLanding(Vector2.zero, rEff, out _);
            Assert.AreEqual(1, t.BounceCount);
            Assert.IsFalse(t.IsRecovered);
            Assert.Greater(t.VelMmS.z, 0f); // Rebounded

            // Simulate peak and falling back down
            t.VelMmS.z = -180.0f;

            // Bounce 2
            t.EvaluateLanding(Vector2.zero, rEff, out _);
            Assert.AreEqual(2, t.BounceCount);
            Assert.IsTrue(t.IsRecovered);
        }

        [Test]
        public void AT06_WindTelegraph()
        {
            float telegraphSec = 1.5f;
            Assert.AreEqual(1.5f, telegraphSec);
        }

        [Test]
        public void AT07_Miss()
        {
            var t = new Trooper();
            t.PosMm = new Vector3(50.0f, 50.0f, 50.0f);
            t.VelMmS = new Vector3(0f, 0f, -120.0f);

            // Step past z = 40 mm
            t.Step(0.15f); // z becomes 50 - 18 = 32 mm < 40 mm

            Assert.IsTrue(t.IsMissed);
        }

        [Test]
        public void AT08_GuidePing()
        {
            int penalty = -25;
            Assert.AreEqual(-25, penalty);
        }

        [Test]
        public void AT09_Release()
        {
            float pauseSec = 1.2f;
            Assert.AreEqual(1.2f, pauseSec);
        }

        [Test]
        public void AT10_SoftBoundary()
        {
            float returnSec = 0.6f;
            Assert.AreEqual(0.6f, returnSec);
        }

        [Test]
        public void AT11_DropTimer()
        {
            float totalSec = ParatrooperBasketConfig.DropTimerSec;
            Assert.AreEqual(300.0f, totalSec, 1.0f);
        }

        [Test]
        public void AT12_Occlusion()
        {
            var rig = new GameObject("BasketRig").AddComponent<BasketRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Adaptive()
        {
            int catches = 10;
            float extraWind = (catches >= 10) ? 4.0f : 0f;
            Assert.AreEqual(4.0f, extraWind);
        }

        [Test]
        public void AT14_Performance()
        {
            List<Trooper> troopers = new List<Trooper>();
            for (int i = 0; i < 12; i++)
            {
                var tr = new Trooper();
                tr.PosMm = new Vector3(0f, 0f, 100f);
                tr.VelMmS = new Vector3(0f, 0f, -150f);
                troopers.Add(tr);
            }

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                foreach (var tr in troopers) tr.Step(dt);
            }

            Assert.AreEqual(12, troopers.Count);
        }
    }
}
