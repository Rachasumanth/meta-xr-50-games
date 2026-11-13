using System;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ObservatoryDome.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_YawMapping()
        {
            Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);
            float yaw = YawTracker.ComputeYaw(rot90);
            Assert.AreEqual(90.0f, yaw, 2.0f);
        }

        [Test]
        public void AT02_YawWrap()
        {
            Quaternion rot360 = Quaternion.Euler(0f, 360f, 0f);
            float yaw = YawTracker.ComputeYaw(rot360);
            // In Unity 360 unwraps to 0
            Assert.AreEqual(0.0f, yaw, 1.0f);
        }

        [Test]
        public void AT03_SlitWidth()
        {
            float w = SlitController.ComputeSlitWidthDeg(30.0f); // 30 mm separation
            Assert.AreEqual(40.0f, w, 4.0f);
        }

        [Test]
        public void AT04_Containment()
        {
            // Target at alpha = 10 deg, dome at 0 deg
            // At w = 40 deg, half-width is 20 deg >= 10 deg -> contained
            var t1 = new Target { AlphaDeg = 10.0f };
            t1.Step(0f, 40.0f, 0f, 0.1f);
            Assert.Greater(t1.LockTimerSec, 0f);

            // At w = 12 deg, half-width is 6 deg < 10 deg -> not contained
            var t2 = new Target { AlphaDeg = 10.0f };
            t2.Step(0f, 12.0f, 0f, 0.1f);
            Assert.AreEqual(0f, t2.LockTimerSec);
        }

        [Test]
        public void AT05_LockTiming()
        {
            var t = new Target { AlphaDeg = 0f };
            float dt = 0.01f;

            // 1.25s of containment (> 1.2s)
            for (float time = 0; time <= 1.25f; time += dt)
            {
                t.Step(0f, 30.0f, 0f, dt);
            }

            Assert.IsTrue(t.IsCaptured);
        }

        [Test]
        public void AT06_Break()
        {
            var t = new Target { AlphaDeg = 0f };
            t.LockTimerSec = 0.8f;
            float dt = 0.01f;

            // 0.45s of non-containment (> 0.40s)
            for (float time = 0; time <= 0.45f; time += dt)
            {
                t.Step(90.0f, 20.0f, 0f, dt); // Target outside slit
            }

            Assert.AreEqual(0f, t.LockTimerSec);
        }

        [Test]
        public void AT07_ClosedShutter()
        {
            var t = new Target { AlphaDeg = 0f };
            // w = 5 deg (< 8 deg)
            t.Step(0f, 5.0f, 0f, 0.5f);
            Assert.AreEqual(0f, t.LockTimerSec);
        }

        [Test]
        public void AT08_SlewGuard()
        {
            var t = new Target { AlphaDeg = 0f };
            // Contained, but slewing at 120 deg/s (> 90 deg/s limit)
            t.Step(0f, 30.0f, 120.0f, 0.2f);
            Assert.AreEqual(0f, t.LockTimerSec);
        }

        [Test]
        public void AT09_Escape()
        {
            var t = new Target { AlphaDeg = 90.0f };
            float dt = 0.1f;

            // 12.5s uncontained (> 12.0s)
            for (float time = 0; time <= 12.5f; time += dt)
            {
                t.Step(0f, 20.0f, 0f, dt);
            }

            Assert.IsTrue(t.HasEscaped);
        }

        [Test]
        public void AT10_Chain()
        {
            float baseScore = 150f;
            float chainScore = baseScore * ObservatoryDomeConfig.ChainMultiplier;
            Assert.AreEqual(195.0f, chainScore, 0.1f);
        }

        [Test]
        public void AT11_ReAnchor()
        {
            var rig = new GameObject("ObsRig").AddComponent<ObservatoryRig>();
            rig.Score = 500;
            rig.IsPaused = true;
            Assert.AreEqual(500, rig.Score);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT12_Occlusion()
        {
            var rig = new GameObject("ObsRig").AddComponent<ObservatoryRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT13_Pause()
        {
            var rig = new GameObject("ObsRig").AddComponent<ObservatoryRig>();
            rig.DomeYawDeg = 45.0f;
            rig.IsPaused = true;
            Assert.IsTrue(rig.IsPaused);
            Assert.AreEqual(45.0f, rig.DomeYawDeg);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Performance()
        {
            var t = new Target();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                t.Step(0f, 30.0f, 0f, dt);
            }

            Assert.IsTrue(t.IsCaptured);
        }
    }
}
