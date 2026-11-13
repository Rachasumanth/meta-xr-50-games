using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest.Editor
{
    public class KatamariHarvestAcceptanceTests
    {
        [Test]
        public void AT01_TwoSurfaceLaw()
        {
            var kin = new TwoSurfaceKinematics();
            Vector3 palmVel = new Vector3(100f, 0f, 0f); // 100 mm/s along +x
            Vector3 normal = Vector3.forward; // normal in +z (tray to palm)
            float r = 33.5f;

            Vector3 ballVel = kin.ComputeBallVelocity(palmVel);
            Vector3 omega = kin.ComputeRollingOmega(normal, palmVel, r);

            Assert.AreEqual(50f, ballVel.x, 1.0f); // v_C = 50 mm/s +- 2%
            Assert.AreEqual(1.49f, omega.magnitude, 0.05f); // |omega| = 1.49 rad/s +- 3%
            Assert.Greater(Vector3.Dot(omega.normalized, Vector3.up), 0.95f); // axis +y +- 2 deg
        }

        [Test]
        public void AT02_StrokeGeometry()
        {
            var stroke = new StrokeAnalyzer();
            float ballCircle = stroke.ComputeBallCircleDiameter(50.0f);
            Assert.AreEqual(25.0f, ballCircle, 3.0f);
            Assert.GreaterOrEqual(stroke.QualityQ, 0.9f);
        }

        [Test]
        public void AT03_GrowthLaw()
        {
            Assert.AreEqual(22.0f, StickySphere.ComputeRadius(100f), 0.3f);
            Assert.AreEqual(44.0f, StickySphere.ComputeRadius(800f), 0.3f);
            Assert.AreEqual(60.0f, StickySphere.ComputeRadius(6400f), 0.3f);
        }

        [Test]
        public void AT04_TierGate()
        {
            var sphere = new StickySphere();
            var barn = new HarvestEntity { Id = 1, Kind = HarvestKind.Barn, RadiusMm = 38.0f, Mass = 100f, Position = new Vector2(30f, 0f) };
            var resolver = new HarvestResolver();

            // Set R = 44.0 mm
            // 22 * (M/100)^(1/3) = 44.0 => M = 800
            sphere.SetMass(800f);
            Assert.AreEqual(44.0f, sphere.RadiusMm, 0.3f);
            bool capturedAt44 = resolver.CheckCapture(sphere, Vector2.zero, barn, 0f);
            Assert.IsFalse(capturedAt44, "Barn must not be captured at R = 44.0 mm");

            // At R = 44.8 mm: (44.8 / 22)^3 * 100 = 844.7f
            sphere.SetMass(845f);
            Assert.AreEqual(44.8f, sphere.RadiusMm, 0.3f);
            bool capturedAt44_8 = resolver.CheckCapture(sphere, Vector2.zero, barn, 0f);
            Assert.IsTrue(capturedAt44_8, "Barn must be captured at R = 44.8 mm");
        }

        [Test]
        public void AT05_CaptureBoundary()
        {
            var sphere = new StickySphere();
            sphere.SetMass(100f); // R = 22 mm
            var resolver = new HarvestResolver();

            // Sheep r = 9.0 mm. 0.3 * 9 = 2.7 mm
            // Distance 1: d = R + 2.7 mm = 24.7 mm -> passes
            var sheep1 = new HarvestEntity { Id = 1, Kind = HarvestKind.Sheep, RadiusMm = 9.0f, Mass = 10f, Position = new Vector2(24.7f, 0f) };
            bool cap1 = resolver.CheckCapture(sphere, Vector2.zero, sheep1, 0f);
            Assert.IsTrue(cap1, "Sheep at d = R + 2.7 mm must be captured");

            // Distance 2: d = R + 3.3 mm = 25.3 mm -> fails
            sphere.SetMass(100f); // Reset sphere to R = 22.0 mm
            var sheep2 = new HarvestEntity { Id = 2, Kind = HarvestKind.Sheep, RadiusMm = 9.0f, Mass = 10f, Position = new Vector2(25.3f, 0f) };
            bool cap2 = resolver.CheckCapture(sphere, Vector2.zero, sheep2, 0f);
            Assert.IsFalse(cap2, "Sheep at d = R + 3.3 mm must not be captured");
        }

        [Test]
        public void AT06_MineDamage()
        {
            var sphere = new StickySphere();
            sphere.SetMass(1000f);
            var resolver = new HarvestResolver();

            resolver.TriggerMine(sphere);

            Assert.AreEqual(880f, sphere.Mass, 1.0f);
            Assert.AreEqual(0.8f, resolver.StunTimer, 0.05f);
            Assert.AreEqual(0, resolver.Chain);
            Assert.IsTrue(resolver.IsStunned);

            // Capture should be disabled while stunned
            var sheep = new HarvestEntity { Id = 1, Kind = HarvestKind.Sheep, RadiusMm = 9.0f, Mass = 10f, Position = Vector2.zero };
            bool cap = resolver.CheckCapture(sphere, Vector2.zero, sheep, 0.1f);
            Assert.IsFalse(cap, "Capture must be disabled during stun");
        }

        [Test]
        public void AT07_MassFloor()
        {
            var sphere = new StickySphere();
            sphere.SetMass(200f);
            var resolver = new HarvestResolver();

            for (int i = 0; i < 10; i++)
            {
                resolver.TriggerMine(sphere);
            }

            Assert.AreEqual(60.0f, sphere.Mass, 0.001f);
            Assert.GreaterOrEqual(sphere.Mass, KatamariHarvestConfig.MassFloor);
        }

        [Test]
        public void AT08_ChainMultiplier()
        {
            var sphere = new StickySphere();
            var resolver = new HarvestResolver();

            float now = 0f;
            for (int i = 0; i < 5; i++)
            {
                var sheep = new HarvestEntity { Id = i + 1, Kind = HarvestKind.Sheep, RadiusMm = 9.0f, Mass = 10f, Position = Vector2.zero };
                resolver.CheckCapture(sphere, Vector2.zero, sheep, now);
                float expectedMult = 1.0f + 0.15f * i;
                Assert.AreEqual(expectedMult, resolver.Multiplier, 0.001f);
                now += 1.0f;
            }

            // Wait 1.5 s (> 1.2 s)
            now += 1.5f;
            var sheepAfterWait = new HarvestEntity { Id = 99, Kind = HarvestKind.Sheep, RadiusMm = 9.0f, Mass = 10f, Position = Vector2.zero };
            resolver.CheckCapture(sphere, Vector2.zero, sheepAfterWait, now);
            Assert.AreEqual(1.0f, resolver.Multiplier, 0.001f);
        }

        [Test]
        public void AT09_SeasonTiming()
        {
            var fsm = new SeasonStateMachine();
            fsm.StartSeason(700.0f);

            float dt = 1.0f / 90.0f; // 90 Hz ticks
            int ticks = 90 * 90; // 90 seconds
            for (int i = 0; i < ticks; i++)
            {
                fsm.Tick(dt, 750.0f);
            }

            Assert.AreEqual(0.0f, fsm.TimeRemainingSec, 0.02f);
            Assert.AreEqual(1, fsm.QuotaEvaluatedCount);
            Assert.IsTrue(fsm.QuotaMet);
        }

        [Test]
        public void AT10_RespawnFloor()
        {
            var meadow = new MeadowDirector();
            meadow.Initialize(sheepCount: 60);

            // Capture 40 sheep
            for (int i = 0; i < 40; i++)
            {
                meadow.Entities[i].Captured = true;
            }
            Assert.AreEqual(20, meadow.ActiveSheepCount);

            // Advance 1.5 seconds
            Vector2 spherePos = Vector2.zero;
            meadow.Update(1.5f, spherePos);

            Assert.GreaterOrEqual(meadow.ActiveSheepCount, 45);

            // Check that all newly added sheep are >= 60 mm from sphere
            var sheepList = meadow.Entities.FindAll(e => e.Kind == HarvestKind.Sheep);
            for (int i = 60; i < sheepList.Count; i++)
            {
                float dist = Vector2.Distance(sheepList[i].Position, spherePos);
                Assert.GreaterOrEqual(dist, 59.9f);
            }
        }

        [Test]
        public void AT11_BallOcclusion_Deferred()
        {
            // Depth-buffer test requiring physical MR scene mesh / camera occlusion rasterization
            Assert.Pass("AT-11 deferred to device: depth buffer rasterization verification requires MR compositor.");
        }

        [Test]
        public void AT12_OffTrayRecovery()
        {
            var fsm = new SeasonStateMachine();
            fsm.StartSeason(700.0f);

            float timeBefore = fsm.TimeRemainingSec;
            fsm.SetOffTray(true);

            // Simulate 3 s off-tray
            fsm.Tick(3.0f, 200.0f);
            Assert.AreEqual(timeBefore, fsm.TimeRemainingSec, "Clock must pause while off-tray");

            fsm.SetOffTray(false);
            fsm.Tick(1.0f, 200.0f);
            Assert.Less(fsm.TimeRemainingSec, timeBefore, "Clock must resume when on-tray");
        }

        [Test]
        public void AT13_Determinism_Deferred()
        {
            // Device trace playback verification
            Assert.Pass("AT-13 deferred to device: 90s recorded ball trace replay and state hashing.");
        }

        [Test]
        public void AT14_AudioCoverage()
        {
            var router = new FeedbackRouter();
            Assert.AreEqual(13, router.TotalMappedEvents);
            foreach (FeedbackEvent evt in System.Enum.GetValues(typeof(FeedbackEvent)))
            {
                Assert.IsTrue(router.HasMapping(evt), $"Missing mapping for {evt}");
                Assert.IsNotNull(router.GetClipName(evt), $"Null clip for {evt}");
            }
        }
    }
}
