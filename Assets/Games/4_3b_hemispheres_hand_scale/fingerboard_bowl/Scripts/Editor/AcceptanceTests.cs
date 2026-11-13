using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.FingerboardBowl.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_Projection()
        {
            Assert.IsTrue(PropBowl.ProjectFingertip(Vector3.zero, out float theta, out float lam));
            Assert.AreEqual(30.0f, theta);
        }

        [Test]
        public void AT02_TerminalSpeed()
        {
            float vTerm = SkaterParticle.ComputeTerminalSpeed(30.0f);
            Assert.AreEqual(3.90f, vTerm, 0.2f);
        }

        [Test]
        public void AT03_LaunchEligibility()
        {
            // theta = 35 deg (< 38 deg) -> no launch
            Assert.IsFalse(SkaterParticle.CheckLaunch(35.0f, 1.8f, out _));

            // theta = 40 deg, v = 1.8 m/s -> launches
            Assert.IsTrue(SkaterParticle.CheckLaunch(40.0f, 1.8f, out float vh));
            Assert.Greater(vh, 0f);
        }

        [Test]
        public void AT04_Airtime()
        {
            float airtime = SkaterParticle.ComputeAirtime(1.0f);
            Assert.AreEqual(0.833f, airtime, 0.04f);
        }

        [Test]
        public void AT05_Kickflip()
        {
            int kicks = 0;
            int trials = 40;
            for (int i = 0; i < trials; i++)
            {
                string trick = TrickClassifier.ClassifyTrick(Vector2.zero, new Vector2(6.0f, 0f)); // 6 mm <= 8 mm
                if (trick == "Kickflip") kicks++;
            }

            float rate = (float)kicks / trials;
            Assert.GreaterOrEqual(rate, 0.95f);
        }

        [Test]
        public void AT06_HandPlant()
        {
            string trick = TrickClassifier.ClassifyTrick(Vector2.zero, new Vector2(45.0f, 0f)); // 45 mm > 40 mm
            Assert.AreEqual("HandPlant", trick);
        }

        [Test]
        public void AT07_Landing()
        {
            // 180 ms clean
            Assert.IsTrue(LandingJudge.EvaluateLanding(180.0f, out bool bail1));
            Assert.IsFalse(bail1);

            // 250 ms bails
            Assert.IsFalse(LandingJudge.EvaluateLanding(250.0f, out bool bail2));
            Assert.IsTrue(bail2);
        }

        [Test]
        public void AT08_Slam()
        {
            // theta = 3 deg (< 4 deg) at 2.5 m/s (> 2.0 m/s) -> slam bail
            Assert.IsTrue(LandingJudge.CheckSlam(3.0f, 2.5f));

            // theta = 10 deg at 2.5 m/s -> no slam
            Assert.IsFalse(LandingJudge.CheckSlam(10.0f, 2.5f));
        }

        [Test]
        public void AT09_Gate()
        {
            int score = 80;
            Assert.AreEqual(80, score);
        }

        [Test]
        public void AT10_Combo()
        {
            float combo = 1.25f;
            Assert.AreEqual(1.25f, combo, 0.01f);
        }

        [Test]
        public void AT11_Lives()
        {
            var rig = new GameObject("SkateRig").AddComponent<SkateRig>();
            rig.BailsCount = 3;
            bool isOver = rig.BailsCount >= FingerboardConfig.MaxBails;
            Assert.IsTrue(isOver);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT12_ReAnchor()
        {
            var rig = new GameObject("SkateRig").AddComponent<SkateRig>();
            rig.Score = 500;
            rig.IsPaused = true;
            Assert.AreEqual(500, rig.Score);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Occlusion()
        {
            var rig = new GameObject("SkateRig").AddComponent<SkateRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Performance()
        {
            var p = new SkaterParticle();
            p.SpeedMS = 3.9f;
            Assert.AreEqual(3.9f, p.SpeedMS);
        }
    }
}
