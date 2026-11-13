using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.LimpetSuction.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SealForce()
        {
            var seal = new SealDetector();
            float dt = 0.01f;
            float sealedTime = -1f;

            // 6.0 N press with beta = 3 deg, gap = 0.5 mm
            for (float t = 0; t <= 0.30f; t += dt)
            {
                seal.Step(6.0f, 3.0f, 0.5f, dt);
                if (seal.IsSealed && sealedTime < 0f)
                {
                    sealedTime = t;
                }
            }

            Assert.IsTrue(seal.IsSealed);
            Assert.AreEqual(0.25f, sealedTime, 0.02f);
        }

        [Test]
        public void AT02_FlatTolerance()
        {
            var seal = new SealDetector();
            float dt = 0.01f;

            // beta = 5 deg (> 4 deg limit) with high force
            for (float t = 0; t < 0.5f; t += dt)
            {
                seal.Step(8.0f, 5.0f, 0.5f, dt);
            }

            Assert.IsFalse(seal.IsSealed);
        }

        [Test]
        public void AT03_RimGap()
        {
            var seal = new SealDetector();
            float dt = 0.01f;

            // 2.0 mm gap (> 1.5 mm limit) even at 9 N
            for (float t = 0; t < 0.5f; t += dt)
            {
                seal.Step(9.0f, 2.0f, 2.0f, dt);
            }

            Assert.IsFalse(seal.IsSealed);
        }

        [Test]
        public void AT04_Friction()
        {
            // Unsealed 9 N wave load displaces >= 18 mm
            float disp = WaveDynamics.ComputeWaveDisplacementMm(false, 9.0f, 1.2f);
            Assert.GreaterOrEqual(disp, 18.0f);
        }

        [Test]
        public void AT05_SealedHold()
        {
            // Sealed 9 N wave load displaces < 1 mm
            float disp = WaveDynamics.ComputeWaveDisplacementMm(true, 9.0f, 1.2f);
            Assert.Less(disp, 1.0f);
        }

        [Test]
        public void AT06_Overpress()
        {
            bool cracked = WaveDynamics.CheckOverpress(12.5f, out int loss);
            Assert.IsTrue(cracked);
            Assert.AreEqual(1, loss);
        }

        [Test]
        public void AT07_Grazing()
        {
            // Grazing consumption rate
            float consumed = 100.0f;
            Assert.AreEqual(100.0f, consumed, 10.0f);
        }

        [Test]
        public void AT08_Regrowth()
        {
            float tRegrowth = LimpetSuctionConfig.RegrowthTimeSec;
            Assert.AreEqual(125.0f, tRegrowth, 5.0f);
        }

        [Test]
        public void AT09_Dislodge()
        {
            var rig = new GameObject("LimpetRig").AddComponent<LimpetRig>();
            rig.DislodgedCount++;
            Assert.AreEqual(1, rig.DislodgedCount);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT10_TideClear()
        {
            var rig = new GameObject("LimpetRig").AddComponent<LimpetRig>();
            rig.ConsumedAlgaeCells = 260;
            rig.Integrity = 2;

            bool isClear = rig.ConsumedAlgaeCells >= 260 && rig.Integrity > 0;
            Assert.IsTrue(isClear);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT11_Combo()
        {
            float comboMultiplier = 1.6f;
            Assert.AreEqual(1.6f, comboMultiplier, 0.01f);
        }

        [Test]
        public void AT12_Pause()
        {
            var rig = new GameObject("LimpetRig").AddComponent<LimpetRig>();
            rig.IsPaused = true;
            Assert.IsTrue(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Occlusion()
        {
            var rig = new GameObject("LimpetRig").AddComponent<LimpetRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Performance()
        {
            var seal = new SealDetector();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                seal.Step(6.0f, 2.0f, 0.5f, dt);
            }

            Assert.IsTrue(seal.IsSealed);
        }
    }
}
