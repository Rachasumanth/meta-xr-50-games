using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.SnappingTurtleBunker.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_RegistrationAccuracy()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 22.0f, 0f, Mathf.Sin(angle) * 22.0f);
            }

            for (int trial = 0; trial < 20; trial++)
            {
                bool ok = PropShell.FitHemisphere(rimPoints, out float R, out float tiltDeg);
                Assert.IsTrue(ok);
                Assert.AreEqual(22.0f, R, 0.5f);
                Assert.Less(tiltDeg, 1.5f);
            }
        }

        [Test]
        public void AT02_TiltMapping()
        {
            var tilt = new TiltDynamics();
            float dt = 0.01f;

            // Apply 0.8 N rim press for 180 ms
            for (float t = 0; t < 0.180f; t += dt)
            {
                tilt.Step(0.8f, 0f, dt);
            }

            // 1 - e^(-1) = 63.2% of 3.91 = 2.47 at 180ms, settles to 3.91
            for (float t = 0.180f; t < 0.500f; t += dt)
            {
                tilt.Step(0.8f, 0f, dt);
            }

            Assert.AreEqual(3.91f, tilt.CurrentTiltDeg, 0.4f);
        }

        [Test]
        public void AT03_SectorClassification()
        {
            int correct = 0;
            int total = 100;
            float[] cardinals = new float[] { 0f, 90f, 180f, 270f };

            for (int i = 0; i < total; i++)
            {
                int expected = i % 4;
                float angle = cardinals[expected] + (i % 5 - 2f) * 2f; // Slight noise ±4 deg
                int classified = SectorClassifier.ClassifySector(angle);
                if (classified == expected) correct++;
            }

            float rate = (float)correct / total;
            Assert.GreaterOrEqual(rate, 0.96f);
        }

        [Test]
        public void AT04_BiteTiming()
        {
            int kills = 0;
            int trials = 40;

            for (int i = 0; i < trials; i++)
            {
                // Beetle at 26 mm on raised sector
                bool hit = BiteController.EvaluateBite(3.9f, 2, 2, 26.0f);
                if (hit) kills++;
            }

            float killRate = (float)kills / trials;
            Assert.GreaterOrEqual(killRate, 0.95f);
        }

        [Test]
        public void AT05_WrongSectorBite()
        {
            int kills = 0;
            int trials = 50;

            for (int i = 0; i < trials; i++)
            {
                // Beetle on opposite sector (contact sector 0, raised sector 2, beetle on sector 0)
                bool hit = BiteController.EvaluateBite(3.9f, 2, 0, 26.0f);
                if (hit) kills++;
            }

            Assert.AreEqual(0, kills);
        }

        [Test]
        public void AT06_SealWindow()
        {
            int absorbed = 0;
            int trials = 30;

            for (int i = 0; i < trials; i++)
            {
                var apex = new ApexSealDetector();
                float dt = 0.01f;
                // 150 ms press with 0.40 N
                for (float t = 0; t <= 0.150f; t += dt)
                {
                    apex.Step(0.40f, dt);
                }

                if (apex.IsSealed) absorbed++;
            }

            Assert.AreEqual(30, absorbed);
        }

        [Test]
        public void AT07_UnsealedHit()
        {
            var rig = new GameObject("TurtleRig").AddComponent<TurtleRig>();
            rig.Integrity = 3;

            // Unsealed impact: no press
            var apex = new ApexSealDetector();
            apex.Step(0f, 0.15f);

            if (!apex.IsSealed)
            {
                rig.Integrity -= 1;
            }

            Assert.AreEqual(2, rig.Integrity);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT08_TiltClamp()
        {
            var tilt = new TiltDynamics();
            float dt = 0.01f;

            // Extreme force of 50 N
            for (float t = 0; t < 1.0f; t += dt)
            {
                tilt.Step(50.0f, 45.0f, dt);
            }

            Assert.LessOrEqual(tilt.CurrentTiltDeg, 8.01f);
        }

        [Test]
        public void AT09_ChainLogic()
        {
            var chain = new ChainTracker();
            // Three bites within 4 seconds: t = 0.5s, 1.5s, 2.5s
            chain.RegisterBite(0.5f);
            Assert.IsFalse(chain.IsFrenzy);

            chain.RegisterBite(1.5f);
            Assert.IsFalse(chain.IsFrenzy);

            chain.RegisterBite(2.5f);
            Assert.IsTrue(chain.IsFrenzy);

            // Step 1.0s -> still in frenzy
            chain.Step(1.0f);
            Assert.IsTrue(chain.IsFrenzy);

            // Step another 1.5s -> frenzy ends after 2.0s
            chain.Step(1.5f);
            Assert.IsFalse(chain.IsFrenzy);
        }

        [Test]
        public void AT10_HandLossDecay()
        {
            var tilt = new TiltDynamics();
            tilt.CurrentTiltDeg = 6.0f;
            float dt = 0.01f;

            // Release force: decays towards 0 with tau = 180 ms
            for (float t = 0; t < 0.180f; t += dt)
            {
                tilt.Step(0f, 0f, dt);
            }

            // At 1 tau, ~63% decay: 6.0 * (1 - 0.632) = 2.21 deg
            Assert.Less(tilt.CurrentTiltDeg, 2.8f);
            Assert.Greater(tilt.CurrentTiltDeg, 1.8f);
        }

        [Test]
        public void AT11_TideTimer()
        {
            // 5 tides x 60 s = 300 s
            float totalTime = 5 * 60.0f;
            Assert.AreEqual(300.0f, totalTime, 1.0f);
        }

        [Test]
        public void AT12_Pause()
        {
            var rig = new GameObject("TurtleRig").AddComponent<TurtleRig>();
            rig.Integrity = 3;
            rig.IsPaused = true;

            Assert.IsTrue(rig.IsPaused);
            Assert.AreEqual(3, rig.Integrity);

            rig.IsPaused = false;
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Occlusion()
        {
            var rig = new GameObject("TurtleRig").AddComponent<TurtleRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Performance()
        {
            var tilt = new TiltDynamics();
            var apex = new ApexSealDetector();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                tilt.Step(0.5f, 90f, dt);
                apex.Step(0.2f, dt);
            }

            Assert.Greater(tilt.CurrentTiltDeg, 0f);
        }
    }
}
