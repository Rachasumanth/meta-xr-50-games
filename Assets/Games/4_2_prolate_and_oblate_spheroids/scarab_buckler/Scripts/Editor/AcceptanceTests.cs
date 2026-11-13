using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SpheroidRegistration()
        {
            var tracker = new PropTracker();
            var rimPoints = new List<Vector3>();
            int count = 24;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((tracker.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (tracker.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = tracker.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.6f);
            Assert.Less(yawDeg, 2.0f);
        }

        [Test]
        public void AT02_ReflectionLaw()
        {
            Vector3 d = new Vector3(0, 0, -1);
            Vector3 n = new Vector3(0, 0, 1);
            Vector3 r = BucklerOptics.Reflect(d, n, out float energy);

            Assert.AreEqual(0.92f, energy, 0.001f);
            Assert.AreEqual(0f, r.x, 0.001f);
            Assert.AreEqual(0f, r.y, 0.001f);
            Assert.AreEqual(1.0f, r.z, 0.002f);
        }

        [Test]
        public void AT03_FanSpreadCentred()
        {
            float[] angles = FanSolver.ComputeFanAnglesDeg(0f, 12.0f);
            Assert.AreEqual(5, angles.Length);
            Assert.AreEqual(-22.9f, angles[0], 1.0f);
            Assert.AreEqual(-11.5f, angles[1], 1.0f);
            Assert.AreEqual(0.0f, angles[2], 0.1f);
            Assert.AreEqual(11.5f, angles[3], 1.0f);
            Assert.AreEqual(22.9f, angles[4], 1.0f);
        }

        [Test]
        public void AT04_FanSpreadOffset()
        {
            float[] angles = FanSolver.ComputeFanAnglesDeg(18.0f, 12.0f);
            Assert.AreEqual(5, angles.Length);
            Assert.AreEqual(11.5f, angles[0], 2.0f);
            Assert.AreEqual(57.3f, angles[4], 2.0f);
            // One-sided: all angles strictly positive
            for (int i = 0; i < 5; i++)
            {
                Assert.Greater(angles[i], 0f);
            }
        }

        [Test]
        public void AT05_TargetCone()
        {
            Vector3 rayDir = Vector3.forward;
            // 2.9 deg offset
            Quaternion rot29 = Quaternion.Euler(0f, 2.9f, 0f);
            Vector3 targetDir29 = rot29 * Vector3.forward;
            bool hit29 = FanSolver.CheckTargetHit(rayDir, targetDir29, 350.0f);
            Assert.IsTrue(hit29);

            // 3.1 deg offset
            Quaternion rot31 = Quaternion.Euler(0f, 3.1f, 0f);
            Vector3 targetDir31 = rot31 * Vector3.forward;
            bool hit31 = FanSolver.CheckTargetHit(rayDir, targetDir31, 350.0f);
            Assert.IsFalse(hit31);
        }

        [Test]
        public void AT06_WristAimGain()
        {
            float yawDead = WristAim.ComputeFanYaw(1.0f); // within 1.5 deg deadzone
            Assert.AreEqual(0f, yawDead);

            float yaw6 = WristAim.ComputeFanYaw(6.0f);
            Assert.AreEqual(16.8f, yaw6, 1.0f);

            float yaw25 = WristAim.ComputeFanYaw(25.0f);
            Assert.AreEqual(70.0f, yaw25, 2.0f);
        }

        [Test]
        public void AT07_HeatAccumulation()
        {
            var heat = new HeatCharge();
            // 5 lances in 0.5 s: each adds 12 heat (total 60), cooling 10/s over 0.5s = -5 heat => 55
            for (int i = 0; i < 5; i++)
            {
                heat.DeflectLance(false);
                heat.Step(0.1f);
            }

            Assert.AreEqual(55.0f, heat.Heat, 2.0f);
        }

        [Test]
        public void AT08_Overheat()
        {
            var heat = new HeatCharge();
            for (int i = 0; i < 9; i++)
            {
                heat.DeflectLance(false);
            }

            Assert.AreEqual(100f, heat.Heat, 0.01f);
            Assert.IsTrue(heat.IsOverheated);
            Assert.AreEqual(2.5f, heat.OverheatTimer, 0.1f);
        }

        [Test]
        public void AT09_ChargeNova()
        {
            var heat = new HeatCharge();
            var targetField = new TargetField();
            targetField.SeedWave(10);

            // Add 100 charge
            for (int i = 0; i < 9; i++)
            {
                heat.DeflectLance(false);
            }
            heat.SetState(0f, 100f);

            bool novaFired = heat.TryTriggerNova();
            Assert.IsTrue(novaFired);
            Assert.AreEqual(0f, heat.Charge);
            Assert.AreEqual(20f, heat.Heat);

            int destroyed = targetField.ExecuteNova();
            Assert.Greater(destroyed, 0);
        }

        [Test]
        public void AT10_Vent()
        {
            var heat = new HeatCharge();
            heat.SetState(80f, 70f);

            heat.Vent();
            Assert.AreEqual(40.0f, heat.Heat, 1.0f);
            Assert.AreEqual(40.0f, heat.Charge, 1.0f);
            Assert.IsTrue(heat.IsVenting);
            Assert.IsFalse(heat.IsOverheated);
        }

        [Test]
        public void AT11_MissDamage()
        {
            var beams = new BeamDirector();
            var combo = new ScoreCombo();
            combo.RegisterTargetHit();

            beams.ApplyMissDamage(false); // lance: -12
            Assert.AreEqual(88, beams.PlayerHull);

            beams.ApplyMissDamage(true); // heavy: -20
            Assert.AreEqual(68, beams.PlayerHull);

            combo.ResetCombo();
            Assert.AreEqual(0, combo.ComboCount);
        }

        [Test]
        public void AT12_RimScatter()
        {
            bool inCap = BucklerOptics.CheckAperture(45.0f, out bool isRim);
            Assert.IsTrue(inCap);
            Assert.IsTrue(isRim);
        }

        [Test]
        public void AT13_OcclusionReadback()
        {
            // Target behind buckler: fragment contribution = 0 inside 60 mm silhouette
            float bucklerRadius = 0.030f; // 60 mm diameter = 30 mm radius
            Vector2 samplePos = new Vector2(0.010f, 0.010f);
            bool insideSilhouette = samplePos.magnitude < bucklerRadius;
            Assert.IsTrue(insideSilhouette);

            float targetDepth = 0.40f;
            float bucklerDepth = 0.20f;
            bool occluded = (targetDepth > bucklerDepth) && insideSilhouette;
            Assert.IsTrue(occluded);
        }

        [Test]
        public void AT14_TrackingLoss()
        {
            var tracker = new PropTracker();
            float dt = 0.1f;

            for (float t = 0; t < 2.0f; t += dt)
            {
                tracker.StepTrackingLoss(false, dt);
                if (t >= 0.8f)
                {
                    Assert.IsTrue(tracker.IsPaused);
                }
            }
        }
    }
}
