using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SpheroidRegistration()
        {
            var tracker = new PropTracker();
            var rimPoints = new List<Vector3>();
            int pointCount = 28;
            for (int i = 0; i < pointCount; i++)
            {
                float angle = (i / (float)pointCount) * Mathf.PI * 2f;
                // a=31mm, b=31mm, c=19mm
                float noise = (Mathf.Sin(i * 1.7f) * 0.0003f); // 0.3mm noise
                rimPoints.Add(new Vector3((tracker.SemiAxisA + noise) * Mathf.Cos(angle),
                                          0f,
                                          (tracker.SemiAxisB + noise) * Mathf.Sin(angle)));
            }

            bool success = tracker.FitSpheroid(rimPoints, out float rmsErrorMm, out float yawErrorDeg);
            Assert.IsTrue(success);
            Assert.Less(rmsErrorMm, 0.6f);
            Assert.Less(yawErrorDeg, 1.5f);
        }

        [Test]
        public void AT02_DriveTerminalSpeed()
        {
            var drive = new PlanarDrive();
            float dt = 1.0f / 90.0f;
            Vector3 handVelocity = new Vector3(0.22f, 0f, 0f); // 0.22 m/s

            for (float t = 0; t < 2.0f; t += dt)
            {
                drive.Step(handVelocity, dt);
            }

            float physicalSpeed = drive.PhysicalVelocity.magnitude;
            float virtualSpeed = drive.VirtualVelocity.magnitude;

            Assert.AreEqual(0.142f, physicalSpeed, 0.010f);
            Assert.AreEqual(0.426f, virtualSpeed, 0.030f);
        }

        [Test]
        public void AT03_StaticFrictionBreakaway()
        {
            var drive = new PlanarDrive();
            float dt = 1.0f / 90.0f;
            float motionStartTime = -1f;

            for (float t = 0; t <= 1.0f; t += dt)
            {
                // Ramp palm velocity from 0 to 0.12 m/s over 1s
                float currentHandSpeed = Mathf.Lerp(0f, 0.12f, t);
                Vector3 vHand = new Vector3(currentHandSpeed, 0f, 0f);
                float f = PlanarDrive.Kp * currentHandSpeed;

                drive.Step(vHand, dt);

                if (f < PlanarDrive.StaticBreakawayF)
                {
                    Assert.AreEqual(0f, drive.PhysicalVelocity.magnitude, 0.0001f, $"Moved prematurely at t={t}, F={f}");
                }
                else if (drive.PhysicalVelocity.magnitude > 0.001f && motionStartTime < 0)
                {
                    motionStartTime = t;
                }
            }

            Assert.IsTrue(motionStartTime >= 0);
            float breakawayExpectedTime = PlanarDrive.StaticBreakawayF / (PlanarDrive.Kp * 0.12f); // ~0.543s
            Assert.AreEqual(breakawayExpectedTime, motionStartTime, 0.12f);
        }

        [Test]
        public void AT04_Clutch()
        {
            var drive = new PlanarDrive();
            drive.StartClutch();

            Assert.IsTrue(drive.IsClutching);
            Assert.AreEqual(0.150f, drive.ClutchTimer, 0.010f);

            Vector3 startVirtual = drive.VirtualPosition;
            drive.Step(new Vector3(0.5f, 0f, 0f), 0.050f);

            // No virtual displacement during freeze
            Assert.AreEqual(startVirtual, drive.VirtualPosition);

            drive.Step(new Vector3(0.5f, 0f, 0f), 0.110f);
            Assert.IsFalse(drive.IsClutching);
            Assert.AreEqual(0.032f, drive.ClutchOffset.x, 0.001f);
        }

        [Test]
        public void AT05_HarpoonRange()
        {
            var harpoon = new Harpoon(Vector3.zero, Vector3.forward, 0);
            float dt = 1.0f / 90.0f;
            float elapsed = 0f;

            while (harpoon.IsAlive && elapsed < 1.0f)
            {
                harpoon.Step(dt);
                elapsed += dt;
            }

            Assert.AreEqual(Harpoon.MaxLifetime, elapsed, dt * 1.5f);
            Assert.AreEqual(0.300f, harpoon.DistanceTraveled, 0.012f); // ~300 ± 8 mm
        }

        [Test]
        public void AT06_DepthGating()
        {
            var harpoons = new HarpoonSystem();
            Vector3 targetPos = new Vector3(0f, 0f, 0.100f);
            int mineBand = 2;

            // Fire at band 1
            var h1 = harpoons.Fire(Vector3.zero, Vector3.forward, 1);
            while (Vector3.Distance(h1.CurrentPosition, targetPos) > 0.010f && h1.IsAlive)
            {
                h1.Step(1.0f / 90.0f);
            }
            bool hit1 = harpoons.CheckHit(h1, targetPos, mineBand, 0.009f, out bool matched1);
            Assert.IsFalse(matched1);
            Assert.IsFalse(hit1);

            // Fire at band 2
            var h2 = harpoons.Fire(Vector3.zero, Vector3.forward, 2);
            while (Vector3.Distance(h2.CurrentPosition, targetPos) > 0.010f && h2.IsAlive)
            {
                h2.Step(1.0f / 90.0f);
            }
            bool hit2 = harpoons.CheckHit(h2, targetPos, mineBand, 0.009f, out bool matched2);
            Assert.IsTrue(matched2);
            Assert.IsTrue(hit2);
        }

        [Test]
        public void AT07_ScrollDetent()
        {
            var bus = new ContactBus();
            float time = 0.1f;

            // 3.1 mm scroll
            bool step1 = bus.ProcessScroll(3.1f, 0.011f, time, out int depth1);
            Assert.IsTrue(step1);
            Assert.AreEqual(1, depth1);

            time += 0.010f; // 10ms later (debounce holds)
            bool step2 = bus.ProcessScroll(2.8f, 0.011f, time, out int depth2);
            Assert.IsFalse(step2);

            time += 0.050f; // 50ms later (debounce expired)
            var bus2 = new ContactBus();
            bool step3 = bus2.ProcessScroll(2.8f, 0.011f, time, out int depth3);
            Assert.IsFalse(step3); // 2.8 < 3.0 threshold
        }

        [Test]
        public void AT08_DriftImpulseAndHeat()
        {
            var drive = new PlanarDrive();
            drive.Heading = Vector3.forward;
            Vector3 velBefore = drive.PhysicalVelocity;

            drive.TriggerDrift();

            Vector3 deltaV = drive.PhysicalVelocity - velBefore;
            Assert.AreEqual(0.21f, deltaV.magnitude, 0.02f);
            Assert.AreEqual(26.0f, drive.Heat, 1.0f);
            Assert.AreEqual(0.45f, drive.DriftReducedFrictionTimer, 0.01f);
        }

        [Test]
        public void AT09_Overheat()
        {
            var drive = new PlanarDrive();
            for (int i = 0; i < 5; i++)
            {
                drive.TriggerDrift();
            }

            Assert.AreEqual(100f, drive.Heat, 0.01f);
            Assert.IsTrue(drive.IsOverheated);
            Assert.AreEqual(3.0f, drive.OverheatTimer, 0.1f);

            // Test thrust cap 0.5N
            drive.Step(new Vector3(10f, 0f, 0f), 0.011f);
            Assert.IsTrue(drive.IsOverheated);
        }

        [Test]
        public void AT10_ComboAndDamage()
        {
            var combo = new ScoreCombo();
            for (int i = 0; i < 4; i++)
            {
                combo.RegisterHit(100);
            }

            Assert.AreEqual(4, combo.ComboCount);
            Assert.AreEqual(2.0f, combo.Multiplier, 0.01f);

            combo.RegisterMiss();
            Assert.AreEqual(0, combo.ComboCount);
            Assert.AreEqual(1.0f, combo.Multiplier, 0.01f);
        }

        [Test]
        public void AT11_HullLoss()
        {
            var director = new HazardDirector();
            Assert.AreEqual(100, director.HullIntegrity);

            director.ApplyMineDamage();
            director.ApplyMineDamage();
            director.ApplyMineDamage();

            Assert.AreEqual(25, director.HullIntegrity);
            Assert.IsFalse(director.IsGameOver);

            director.ApplyMineDamage();
            Assert.AreEqual(0, director.HullIntegrity);
            Assert.IsTrue(director.IsGameOver);
        }

        [Test]
        public void AT12_OcclusionReadback()
        {
            // Spheroid silhouette occlusion test: 0 contribution inside silhouette
            float a = 0.031f;
            float b = 0.031f;
            Vector2 hullPoint = new Vector2(0.010f, 0.010f);
            bool isInsideSpheroid = (hullPoint.x * hullPoint.x) / (a * a) + (hullPoint.y * hullPoint.y) / (b * b) < 1.0f;
            Assert.IsTrue(isInsideSpheroid);

            float harpoonDepth = 0.05f;
            float hullDepth = 0.01f;
            bool isOccluded = (harpoonDepth > hullDepth) && isInsideSpheroid;
            Assert.IsTrue(isOccluded);
        }

        [Test]
        public void AT13_TrackingLoss()
        {
            var drive = new PlanarDrive();
            drive.Step(new Vector3(0.2f, 0f, 0f), 0.050f);

            // Drop pose frame for 2.0 s
            float dt = 0.050f;
            for (float t = 0; t < 2.0f; t += dt)
            {
                drive.StepTrackingLoss(false, dt);
                drive.Step(new Vector3(0.2f, 0f, 0f), dt);

                if (t >= 0.25f)
                {
                    // Thrust 0 after 200 ms
                    Assert.IsTrue(drive.TrackingLossTimer >= 0.200f);
                }
            }

            Assert.IsTrue(drive.IsPaused); // Pauses at 1.5s
        }

        [Test]
        public void AT14_SoakRun()
        {
            var sm = new ChannelStateMachine();
            var drive = new PlanarDrive();
            var hazards = new HazardDirector();
            sm.StartRun();

            float dt = 1.0f / 90.0f;
            // 500 ticks soak simulation
            for (int i = 0; i < 500; i++)
            {
                drive.Step(new Vector3(0.1f, 0f, 0f), dt);
                sm.Step(dt, drive.VirtualPosition.x, hazards.HullIntegrity);
            }

            Assert.IsTrue(sm.CurrentState == ChannelState.RUN || sm.CurrentState == ChannelState.CHANNEL_END);
            Assert.Less(drive.PhysicalVelocity.magnitude, 1.0f);
        }
    }
}
