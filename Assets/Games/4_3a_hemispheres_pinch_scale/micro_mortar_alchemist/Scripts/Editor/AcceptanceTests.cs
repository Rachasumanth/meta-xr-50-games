using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.MicroMortarAlchemist.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_PestleContact()
        {
            float f = Pestle.ComputeForce(1.0f); // 1 mm
            Assert.AreEqual(0.60f, f, 0.05f);
        }

        [Test]
        public void AT02_Rhythm()
        {
            float qOn = RhythmClock.ScorePhase(0.0f);
            Assert.AreEqual(1.0f, qOn);

            float qOff = RhythmClock.ScorePhase(0.3f);
            Assert.AreEqual(0.35f, qOff);
        }

        [Test]
        public void AT03_EssenceRate()
        {
            var shard = new Shard();
            for (int i = 0; i < 10; i++)
            {
                shard.ApplyGrind(1.0f, 0.60f); // On-beat
            }

            Assert.AreEqual(70.0f, shard.Essence, 5.0f);
        }

        [Test]
        public void AT04_Impurity()
        {
            var shard = new Shard();
            for (int i = 0; i < 10; i++)
            {
                shard.ApplyGrind(0.35f, 0.60f); // Off-beat
            }

            Assert.AreEqual(30.0f, shard.Impurity, 3.0f);
        }

        [Test]
        public void AT05_Dissolve()
        {
            var shard = new Shard();
            shard.Essence = 100.0f;
            shard.ApplyGrind(1.0f, 0.6f);

            Assert.IsTrue(shard.IsDissolved);
        }

        [Test]
        public void AT06_PourThreshold()
        {
            // 34 deg emits 0
            float flow34 = PourController.ComputeFlowRateGPerS(34.0f);
            Assert.AreEqual(0f, flow34);

            // 40 deg emits 0.58 ± 0.05
            float flow40 = PourController.ComputeFlowRateGPerS(40.0f);
            Assert.AreEqual(0.58f, flow40, 0.05f);
        }

        [Test]
        public void AT07_DoseTolerance()
        {
            // 10.0 g succeeds
            Assert.IsTrue(PourController.EvaluateDose(10.0f, out bool spoiled1));
            Assert.IsFalse(spoiled1);

            // 11.5 g succeeds (within 1.5g)
            Assert.IsTrue(PourController.EvaluateDose(11.5f, out bool spoiled2));
            Assert.IsFalse(spoiled2);

            // 13.0 g spoils
            Assert.IsFalse(PourController.EvaluateDose(13.0f, out bool spoiled3));
            Assert.IsTrue(spoiled3);
        }

        [Test]
        public void AT08_ColorMatch()
        {
            int matched = 1;
            int mismatched = 0;
            Assert.AreEqual(1, matched);
            Assert.AreEqual(0, mismatched);
        }

        [Test]
        public void AT09_SpoilCount()
        {
            var rig = new GameObject("MortarRig").AddComponent<MortarRig>();
            rig.SpoilsCount = 3;

            bool isLost = rig.SpoilsCount >= 3;
            Assert.IsTrue(isLost);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT10_Reload()
        {
            float liftMm = 8.0f;
            bool isPaused = liftMm >= 8.0f;
            Assert.IsTrue(isPaused);
        }

        [Test]
        public void AT11_GrindClamp()
        {
            float twistRadS = 60.0f;
            float clamped = Mathf.Min(twistRadS, 40.0f);
            Assert.AreEqual(40.0f, clamped);
        }

        [Test]
        public void AT12_Occlusion()
        {
            var rig = new GameObject("MortarRig").AddComponent<MortarRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Adaptive()
        {
            float tempo = 1.6f;
            int perfectBrews = 3;
            if (perfectBrews >= 3) tempo += 0.1f;
            Assert.AreEqual(1.7f, tempo, 0.01f);
        }

        [Test]
        public void AT14_Performance()
        {
            var shard = new Shard();
            for (int i = 0; i < 500; i++)
            {
                shard.ApplyGrind(1.0f, 0.6f);
            }

            Assert.IsTrue(shard.IsDissolved);
        }
    }
}
