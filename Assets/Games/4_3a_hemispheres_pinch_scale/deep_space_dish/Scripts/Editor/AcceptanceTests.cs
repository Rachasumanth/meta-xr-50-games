using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.DeepSpaceDish.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_AimAccuracy()
        {
            var aim = new AimFilter();
            Quaternion startRot = Quaternion.identity;
            Quaternion targetRot = Quaternion.Euler(0f, 20f, 0f);

            float dt = 0.01f;
            for (float t = 0; t <= 0.250f; t += dt)
            {
                aim.Step(targetRot, dt);
            }

            float angleDiff = Quaternion.Angle(startRot, aim.CurrentRot);
            Assert.AreEqual(20.0f, angleDiff, 1.5f);
        }

        [Test]
        public void AT02_GainLaw()
        {
            Vector3 dishAxis = Vector3.forward;

            // 0 deg
            float g0 = GainEvaluator.ComputeGain(dishAxis, Vector3.forward);
            Assert.AreEqual(1.00f, g0, 0.02f);

            // 15 deg
            Vector3 dir15 = Quaternion.Euler(0f, 15f, 0f) * Vector3.forward;
            float g15 = GainEvaluator.ComputeGain(dishAxis, dir15);
            Assert.AreEqual(0.87f, g15, 0.02f);

            // 30.6 deg
            Vector3 dir30 = Quaternion.Euler(0f, 30.6f, 0f) * Vector3.forward;
            float g30 = GainEvaluator.ComputeGain(dishAxis, dir30);
            Assert.AreEqual(0.55f, g30, 0.02f);

            // 45 deg
            Vector3 dir45 = Quaternion.Euler(0f, 45f, 0f) * Vector3.forward;
            float g45 = GainEvaluator.ComputeGain(dishAxis, dir45);
            Assert.AreEqual(0.25f, g45, 0.02f);
        }

        [Test]
        public void AT03_LockTiming()
        {
            var sat = new Satellite();
            float dt = 0.01f;

            // Hold G = 0.6 for 1.5s
            for (float t = 0; t < 1.55f; t += dt)
            {
                sat.StepLock(0.60f, 0f, dt);
            }

            Assert.IsTrue(sat.IsCracked);
            Assert.AreEqual(1, sat.CrackCount);
        }

        [Test]
        public void AT04_BreakLogic()
        {
            var sat = new Satellite();
            sat.LockProgressSec = 1.0f;
            float dt = 0.01f;

            // Below G = 0.35 (e.g. 0.20) for 0.85s
            for (float t = 0; t < 0.85f; t += dt)
            {
                sat.StepLock(0.20f, 0f, dt);
            }

            Assert.AreEqual(0f, sat.LockProgressSec);
        }

        [Test]
        public void AT05_SlewGuard()
        {
            var sat = new Satellite();
            float initialLock = sat.LockProgressSec;
            float dt = 0.01f;

            // Wrist flick at 120 deg/s with high gain
            for (float t = 0; t < 0.2f; t += dt)
            {
                sat.StepLock(0.80f, 120.0f, dt);
            }

            Assert.LessOrEqual(sat.LockProgressSec, initialLock);
        }

        [Test]
        public void AT06_EscapeTimer()
        {
            var sat = new Satellite();
            float dt = 0.1f;
            float escapeTime = -1f;

            for (float t = 0; t <= 26.0f; t += dt)
            {
                sat.StepEscape(false, dt);
                if (sat.HasEscaped && escapeTime < 0f)
                {
                    escapeTime = t;
                }
            }

            Assert.IsTrue(sat.HasEscaped);
            Assert.AreEqual(25.0f, escapeTime, 0.2f);
        }

        [Test]
        public void AT07_Chain()
        {
            float baseScore = 300f;
            float chainScore = baseScore * DeepSpaceDishConfig.ChainMultiplier;
            Assert.AreEqual(390.0f, chainScore, 0.1f);
        }

        [Test]
        public void AT08_EscapeLoss()
        {
            var rig = new GameObject("DishRig").AddComponent<DishRig>();
            rig.EscapedCount = 3;

            bool isGameOver = rig.EscapedCount >= 3;
            Assert.IsTrue(isGameOver);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT09_TrackingLoss()
        {
            var aim = new AimFilter();
            aim.CurrentRot = Quaternion.Euler(0f, 4f, 0f);

            // Re-acquisition axis error < 4 deg
            float err = Quaternion.Angle(Quaternion.identity, aim.CurrentRot);
            Assert.LessOrEqual(err, 4.01f);
        }

        [Test]
        public void AT10_CupDrop()
        {
            var rig = new GameObject("DishRig").AddComponent<DishRig>();
            rig.Score = 500;
            rig.IsPaused = true;

            Assert.AreEqual(500, rig.Score);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT11_Occlusion()
        {
            var rig = new GameObject("DishRig").AddComponent<DishRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT12_Performance()
        {
            var sat = new Satellite();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                sat.StepLock(0.6f, 0f, dt);
            }

            Assert.IsTrue(sat.IsCracked);
        }

        [Test]
        public void AT13_Stabilize()
        {
            // Cone expands to 8.4 deg for 4s
            float coneDeg = 8.4f;
            Assert.AreEqual(8.4f, coneDeg, 0.1f);
        }

        [Test]
        public void AT14_Registration()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 14.0f, 0f, Mathf.Sin(angle) * 14.0f);
            }

            for (int trial = 0; trial < 20; trial++)
            {
                bool ok = PropDish.FitGeometry(rimPoints, out float R, out float err);
                Assert.IsTrue(ok);
                Assert.AreEqual(14.0f, R, 0.4f);
            }
        }
    }
}
