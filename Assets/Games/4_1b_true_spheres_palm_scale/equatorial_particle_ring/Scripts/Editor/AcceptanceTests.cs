using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing.Editor
{
    public class EquatorialParticleRingAcceptanceTests
    {
        [Test]
        public void AT01_TraceSpeed()
        {
            var tracer = new EquatorTracer();
            tracer.ProcessSweep(speedMmS: 100.0f, dt: 0.5f, radiusMm: 37.0f);

            Assert.AreEqual(2.70f, tracer.OmegaRadS, 0.06f); // 2.70 rad/s +- 2%
            Assert.AreEqual(50.0f, tracer.ArcLengthMm, 1.0f);
        }

        [Test]
        public void AT02_ArcAccumulation()
        {
            var tracer = new EquatorTracer();
            // Forward 40 mm
            tracer.ProcessDisplacement(40.0f, 0.080f);
            Assert.AreEqual(40.0f, tracer.ArcLengthMm, 0.1f);
            Assert.AreEqual(1.0f, tracer.DirectionSign);

            // Reverse 20 mm with 80 ms (> 60 ms settle)
            tracer.ProcessDisplacement(-20.0f, 0.080f);
            Assert.AreEqual(60.0f, tracer.ArcLengthMm, 0.1f);
            Assert.AreEqual(-1.0f, tracer.DirectionSign);
        }

        [Test]
        public void AT03_EnergyMapping()
        {
            float e1 = RingCharge.ComputeEnergy(arcLengthMm: 120.0f, omegaPeakRadS: 10.0f);
            Assert.AreEqual(100.0f, e1, 0.5f);

            float e2 = RingCharge.ComputeEnergy(arcLengthMm: 60.0f, omegaPeakRadS: 5.0f);
            Assert.AreEqual(65.0f, e2, 0.5f);
        }

        [Test]
        public void AT04_FireGate()
        {
            var launch = new LaunchSolver();
            float playerBearing = 0f;
            float launchBearing = 90f; // well clear of singularity

            // L = 24.9, omega = 3 -> no shot
            Assert.IsFalse(launch.CanFire(24.9f, 3.0f, launchBearing, playerBearing));

            // L = 25.1, omega = 3 -> shot
            Assert.IsTrue(launch.CanFire(25.1f, 3.0f, launchBearing, playerBearing));

            // L = 40, omega = 2.4 -> no shot
            Assert.IsFalse(launch.CanFire(40.0f, 2.4f, launchBearing, playerBearing));
        }

        [Test]
        public void AT05_TangentBearing()
        {
            // lambda_R = 0 deg, positive trace (+1)
            float bearing1 = LaunchSolver.ComputeLaunchBearing(0f, 1.0f);
            Assert.AreEqual(90.0f, bearing1, 0.2f);

            // lambda_R = 30 deg, negative trace (-1)
            float bearing2 = LaunchSolver.ComputeLaunchBearing(30f, -1.0f);
            // 300 deg or -60 deg
            Assert.AreEqual(300.0f, bearing2, 0.2f);
        }

        [Test]
        public void AT06_SectorBoundary()
        {
            var target = new Target { DistanceMm = 400.0f, AzimuthDeg = 90.0f, DriftDegS = 0.0f, HalfWidthDeg = 7.0f, Toughness = 40.0f };
            float energy = 60.0f;

            // e = 7.3 deg <= 7 + 0.4 -> Hit
            var resHit = BoltFlight.EvaluateImpact(launchBearingDeg: 90.0f + 7.3f, energy, target);
            Assert.AreEqual(ImpactResult.Hit, resHit);

            // e = 7.5 deg <= 7 + 1.5 -> Graze
            var resGraze = BoltFlight.EvaluateImpact(launchBearingDeg: 90.0f + 7.5f, energy, target);
            Assert.AreEqual(ImpactResult.Graze, resGraze);
        }

        [Test]
        public void AT07_Toughness()
        {
            var target = new Target { DistanceMm = 400.0f, AzimuthDeg = 90.0f, DriftDegS = 0.0f, HalfWidthDeg = 7.0f, Toughness = 55.0f };

            // E = 54 vs T = 55 -> Bounce
            var resBounce = BoltFlight.EvaluateImpact(launchBearingDeg: 90.0f, energy: 54.0f, target);
            Assert.AreEqual(ImpactResult.Bounce, resBounce);

            // E = 56 vs T = 55 -> Hit
            var resHit = BoltFlight.EvaluateImpact(launchBearingDeg: 90.0f, energy: 56.0f, target);
            Assert.AreEqual(ImpactResult.Hit, resHit);
        }

        [Test]
        public void AT08_DriftLead()
        {
            // Target at 400 mm, drift 3 deg/s, E = 60 (v = 492 mm/s, t_f = 0.813 s)
            var target = new Target { DistanceMm = 400.0f, AzimuthDeg = 90.0f, DriftDegS = 3.0f, HalfWidthDeg = 0.5f, Toughness = 40.0f };
            float energy = 60.0f;
            float t_f = BoltFlight.ComputeFlightTime(400.0f, energy);
            Assert.AreEqual(0.813f, t_f, 0.02f);

            // Aim at alpha_t0 (90 deg) -> target moves ~2.44 deg, resulting in Miss
            var resMiss = BoltFlight.EvaluateImpact(launchBearingDeg: 90.0f, energy, target);
            Assert.AreEqual(ImpactResult.Miss, resMiss);

            // Aim at predicted azimuth: 90 + 3 * t_f = ~92.44 deg -> Hit!
            float predictedAim = 90.0f + target.DriftDegS * t_f;
            var resHit = BoltFlight.EvaluateImpact(predictedAim, energy, target);
            Assert.AreEqual(ImpactResult.Hit, resHit);
        }

        [Test]
        public void AT09_HeatRate()
        {
            var charge = new RingCharge();
            float omega = 10.0f; // rad/s
            float dt = 0.1f;

            // 1.0 second integration
            for (int i = 0; i < 10; i++)
            {
                charge.IntegrateHeat(omega, dt);
            }
            Assert.AreEqual(26.0f, charge.Heat, 2.0f);

            // Jam at 3.8 s +- 0.2 s (total heat reaches 100)
            int stepsUntilJam = 0;
            while (!charge.IsJammed && stepsUntilJam < 50)
            {
                charge.IntegrateHeat(omega, dt);
                stepsUntilJam++;
            }
            float totalTime = 1.0f + stepsUntilJam * dt;
            Assert.IsTrue(charge.IsJammed);
            Assert.AreEqual(3.8f, totalTime, 0.3f);
        }

        [Test]
        public void AT10_JamLimit()
        {
            var charge = new RingCharge();
            var scoring = new RunScoring();
            var fsm = new RunStateMachine();
            fsm.StartRun();

            for (int i = 0; i < 3; i++)
            {
                charge.TriggerJam();
                Assert.AreEqual(45.0f, charge.Heat);
                Assert.AreEqual(2.0f, charge.JamTimer);
                fsm.Tick(0.1f, charge, scoring);
                Assert.AreEqual(RunState.Active, fsm.State);
            }

            // 4th Jam
            charge.TriggerJam();
            fsm.Tick(0.1f, charge, scoring);
            Assert.AreEqual(RunState.GameOver, fsm.State);
        }

        [Test]
        public void AT11_GripSlip()
        {
            var tracer = new EquatorTracer();
            tracer.ArcLengthMm = 100.0f;

            var charge = new RingCharge();
            var cradle = new CradleMonitor();
            cradle.UpdateGrip(propAngularSpeedDegS: 50.0f); // slip!
            Assert.AreEqual(CradleState.Slip, cradle.State);

            // 1.0 second decay: 100 * exp(-0.40 * 1) = 67.03 mm
            float dt = 0.05f;
            for (int i = 0; i < 20; i++)
            {
                charge.Update(dt, tracer, cradle.State);
            }

            Assert.AreEqual(67.0f, tracer.ArcLengthMm, 1.5f);
        }

        [Test]
        public void AT12_SingularityBlock()
        {
            var launch = new LaunchSolver();
            float playerBearing = 180.0f;

            // alpha_b within 12 deg of player bearing (e.g., 185 deg)
            bool canFire = launch.CanFire(arcLengthMm: 50.0f, omegaPeakRadS: 5.0f, alphaBవరిDeg: 185.0f, playerBearingDeg: playerBearing);
            Assert.IsFalse(canFire, "Must block launch within 12 deg singularity");

            // Outside 12 deg (e.g. 200 deg)
            bool canFireClear = launch.CanFire(arcLengthMm: 50.0f, omegaPeakRadS: 5.0f, alphaBవరిDeg: 200.0f, playerBearingDeg: playerBearing);
            Assert.IsTrue(canFireClear, "Launch allowed outside singularity");
        }

        [Test]
        public void AT13_RingOcclusion_Deferred()
        {
            Assert.Pass("AT-13 deferred to device: depth buffer rasterization verification of rear ring segment.");
        }

        [Test]
        public void AT14_DeterminismAndAudio()
        {
            var router = new FeedbackRouter();
            Assert.AreEqual(14, router.TotalMappedEvents);
            foreach (FeedbackEvent evt in System.Enum.GetValues(typeof(FeedbackEvent)))
            {
                Assert.IsTrue(router.HasMapping(evt), $"Missing mapping for {evt}");
                Assert.IsNotNull(router.GetClipName(evt), $"Null clip for {evt}");
            }
        }
    }
}
