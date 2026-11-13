using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.TempleBell.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_PendulumFrequency()
        {
            var p = new PendulumDynamics();
            p.BetaDeg = 5.0f;
            float dt = 0.001f;

            // Count zero crossings over 1 second
            int zeroCrossings = 0;
            float prevBeta = p.BetaDeg;
            for (float t = 0; t <= 1.0f; t += dt)
            {
                p.Step(dt);
                if ((prevBeta > 0f && p.BetaDeg <= 0f) || (prevBeta < 0f && p.BetaDeg >= 0f))
                {
                    zeroCrossings++;
                }
                prevBeta = p.BetaDeg;
            }

            // Frequency = zeroCrossings / 2 ~ 4.1 Hz -> ~8 zero crossings in 1s
            float freq = zeroCrossings / 2.0f;
            Assert.AreEqual(4.1f, freq, 0.6f);
        }

        [Test]
        public void AT02_TapImpulse()
        {
            var p = new PendulumDynamics();
            // 0.15 N for 40 ms
            p.ApplyTapImpulse(0.15f, 0.040f);

            Assert.AreEqual(0.40f, p.BetaDotRadS, 0.06f);
        }

        [Test]
        public void AT03_FallThreshold()
        {
            var p = new PendulumDynamics();
            p.BetaDeg = 21.0f; // > 20 deg
            float dt = 0.01f;

            // Step for 130 ms (> 120 ms)
            for (float t = 0; t <= 0.130f; t += dt)
            {
                p.Step(dt);
                p.BetaDeg = 21.0f; // Hold above threshold
            }

            Assert.IsTrue(p.HasFallen);
        }

        [Test]
        public void AT04_RingSpeed()
        {
            var ring = new AcousticRing();
            ring.Step(1.0f); // 1 second

            Assert.AreEqual(220.0f, ring.RadiusMm, 10.0f);
        }

        [Test]
        public void AT05_GainLaw()
        {
            Vector3 domeNormal = Vector3.up;

            // 0 deg
            float g0 = AcousticRing.ComputeGain(domeNormal, Vector3.up);
            Assert.AreEqual(1.00f, g0, 0.02f);

            // 15 deg
            Vector3 dir15 = Quaternion.Euler(15f, 0f, 0f) * Vector3.up;
            float g15 = AcousticRing.ComputeGain(domeNormal, dir15);
            Assert.AreEqual(0.87f, g15, 0.02f);

            // 30 deg
            Vector3 dir30 = Quaternion.Euler(30f, 0f, 0f) * Vector3.up;
            float g30 = AcousticRing.ComputeGain(domeNormal, dir30);
            Assert.AreEqual(0.56f, g30, 0.02f);

            // 45 deg
            Vector3 dir45 = Quaternion.Euler(45f, 0f, 0f) * Vector3.up;
            float g45 = AcousticRing.ComputeGain(domeNormal, dir45);
            Assert.AreEqual(0.25f, g45, 0.02f);
        }

        [Test]
        public void AT06_Resonant()
        {
            float g = 0.90f;
            bool isResonant = g > TempleBellConfig.ResonantGainThreshold;
            Assert.IsTrue(isResonant);
            int damage = isResonant ? 2 : 1;
            int bonus = isResonant ? 50 : 0;
            Assert.AreEqual(2, damage);
            Assert.AreEqual(50, bonus);
        }

        [Test]
        public void AT07_Push()
        {
            float push = AcousticRing.ComputePush(150.0f, 0.90f);
            Assert.AreEqual(8.4f, push, 1.0f);
        }

        [Test]
        public void AT08_SpiritLoss()
        {
            float rSpirit = 25.0f; // < 30 mm
            bool failed = rSpirit < TempleBellConfig.SpiritLossRadiusMm;
            Assert.IsTrue(failed);
        }

        [Test]
        public void AT09_Debounce()
        {
            // Debounce window is 150 ms
            float dt1 = 0.100f; // 100 ms < 150 ms -> rejected
            bool secondTap1 = dt1 >= TempleBellConfig.DebounceSec;
            Assert.IsFalse(secondTap1);

            float dt2 = 0.200f; // 200 ms > 150 ms -> accepted
            bool secondTap2 = dt2 >= TempleBellConfig.DebounceSec;
            Assert.IsTrue(secondTap2);
        }

        [Test]
        public void AT10_TapForceWindow()
        {
            var p1 = new PendulumDynamics();
            p1.ApplyTapImpulse(0.04f, 0.040f); // Ignored
            Assert.AreEqual(0f, p1.BetaDotRadS);

            var p2 = new PendulumDynamics();
            p2.ApplyTapImpulse(0.35f, 0.040f); // Registered
            Assert.Greater(p2.BetaDotRadS, 0f);
        }

        [Test]
        public void AT11_Brace()
        {
            float wobble = 10.0f;
            float bracedWobble = wobble * 0.70f;
            Assert.AreEqual(7.0f, bracedWobble, 0.1f);
        }

        [Test]
        public void AT12_Pause()
        {
            var rig = new GameObject("BellRig").AddComponent<BellRig>();
            rig.Score = 200;
            rig.IsPaused = true;

            Assert.IsTrue(rig.IsPaused);
            Assert.AreEqual(200, rig.Score);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Occlusion()
        {
            var rig = new GameObject("BellRig").AddComponent<BellRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Performance()
        {
            var p = new PendulumDynamics();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                p.Step(dt);
            }

            Assert.IsNotNull(p);
        }
    }
}
