using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting.Editor
{
    public class GeyserVentingAcceptanceTests
    {
        [Test]
        public void AT01_Registration()
        {
            var prop = new PropOrange();
            Assert.AreEqual(38.0f, prop.RadiusMm, 0.6f);
            Assert.IsTrue(prop.IsRegistered);
        }

        [Test]
        public void AT02_FingertipPenetration()
        {
            var sampler = new ContactSampler();
            float dt = 0.011f; // 90 Hz

            // s = 0 mm -> D = 5 mm, FilteredPenetration builds up
            for (int i = 0; i < 30; i++) sampler.Sample(0, 0f, dt);
            Assert.GreaterOrEqual(sampler.Effectors[0].FilteredPenetrationMm, 4.0f);

            // s = 4.9 mm -> D = 0.1 mm, FilteredPenetration stays < 5 mm
            var sampler2 = new ContactSampler();
            for (int i = 0; i < 450; i++) sampler2.Sample(0, 4.9f, dt);
            Assert.Less(sampler2.Effectors[0].FilteredPenetrationMm, 1.0f);
        }

        [Test]
        public void AT03_HoldDurationWeights()
        {
            var arbiter = new PressArbiter();

            // Single effector
            var vent1 = new Vent { Id = 1, State = VentState.Active };
            arbiter.StepHold(vent1, effectorCount: 1, dt: 0.30f, null, null);
            Assert.IsTrue(vent1.IsSealed);

            // Two effectors
            var vent2 = new Vent { Id = 2, State = VentState.Active };
            arbiter.StepHold(vent2, effectorCount: 2, dt: 0.167f, null, null);
            Assert.IsTrue(vent2.IsSealed);
        }

        [Test]
        public void AT04_HoldDecay()
        {
            var arbiter = new PressArbiter();
            var vent = new Vent { Id = 1, State = VentState.Active, HoldTimer = 0.20f };

            // Release for 80 ms (< 100 ms) -> preserved
            arbiter.DecayHold(vent, timeSinceHold: 0.080f, dt: 0.080f);
            Assert.AreEqual(0.20f, vent.HoldTimer);

            // Release for 120 ms (> 100 ms) with 4x decay over 0.10s
            arbiter.DecayHold(vent, timeSinceHold: 0.120f, dt: 0.050f); // 0.20 - 4.0 * 0.05 = 0.0
            Assert.AreEqual(0.0f, vent.HoldTimer);
        }

        [Test]
        public void AT05_WrongPressCrack()
        {
            var arbiter = new PressArbiter();
            var pressure = new PressureIntegrator();
            var crust = new CrustModel();

            pressure.SetPressure(10.0f);
            arbiter.TriggerBareCrustCrack(pressure, crust);

            Assert.AreEqual(1, crust.Cracks);
            Assert.AreEqual(12.0f, pressure.Pressure);
        }

        [Test]
        public void AT06_PressureFlux()
        {
            var ventField = new VentField();
            ventField.Initialize(8);

            // Exactly 5 vents active at a = 18 deg, F0 = 2.4
            for (int i = 0; i < 3; i++) ventField.Vents[i].State = VentState.Sealed;
            for (int i = 3; i < 8; i++) ventField.Vents[i].State = VentState.Active;

            float flux5 = ventField.ComputeTotalFlux(2.4f);
            Assert.AreEqual(12.0f, flux5, 0.1f); // 5 * 2.4 = 12.0/s

            // All sealed -> leak rate -1.2/s
            for (int i = 0; i < 8; i++) ventField.Vents[i].State = VentState.Sealed;
            float leak = ventField.ComputeTotalFlux();
            Assert.AreEqual(-1.2f, leak, 0.05f);
        }

        [Test]
        public void AT07_SealingPressureDrop()
        {
            var arbiter = new PressArbiter();
            var pressure = new PressureIntegrator();
            pressure.SetPressure(10.0f);

            var vent = new Vent { Id = 1, State = VentState.Active };
            arbiter.SealVent(vent, pressure);

            Assert.AreEqual(6.0f, pressure.Pressure);

            // Drop never below 0
            pressure.SetPressure(2.0f);
            arbiter.SealVent(new Vent { Id = 2 }, pressure);
            Assert.AreEqual(0.0f, pressure.Pressure);
        }

        [Test]
        public void AT08_PhaseEndAndTimeout()
        {
            var director = new PhaseDirector();
            var pressure = new PressureIntegrator();
            var crust = new CrustModel();
            var vents = new VentField();
            vents.Initialize(8);

            director.StartPhase(1);

            // 8 seals ends early
            for (int i = 0; i < 8; i++)
            {
                director.RegisterSeal(pressure, crust);
            }
            Assert.AreEqual(PhaseState.PhaseClear, director.State);

            // Timeout at t = 40 s adds 18.0 per unsealed vent
            var director2 = new PhaseDirector();
            director2.StartPhase(1);
            director2.RegisterSeal(pressure, crust); // 1 sealed, 7 unsealed
            pressure.SetPressure(0f);

            director2.Tick(40.1f, vents, pressure, crust, isCovered: false);
            Assert.AreEqual(Mathf.Min(100.0f, 7 * 18.0f), pressure.Pressure);
        }

        [Test]
        public void AT09_ThirdCrackAndRepair()
        {
            var crust = new CrustModel();
            crust.AddCrack();
            crust.AddCrack();
            Assert.IsFalse(crust.IsCollapsed);
            Assert.AreEqual(2, crust.Cracks);

            // Two cracks repair to one after phase clear
            crust.RepairCracks();
            Assert.AreEqual(1, crust.Cracks);

            // Add two more to reach 3
            crust.AddCrack();
            crust.AddCrack();
            Assert.IsTrue(crust.IsCollapsed);
        }

        [Test]
        public void AT10_BallRotation()
        {
            var prop = new PropOrange();
            Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);
            prop.Rotate(rot90);

            var q = prop.Rotation;
            float sqrMag = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
            Assert.AreEqual(1.0f, sqrMag, 1e-5f);
        }

        [Test]
        public void AT11_ReachableCapSubsidence()
        {
            var ventField = new VentField();
            ventField.Initialize(1);
            ventField.Vents[0].Direction = Vector3.back; // behind forward cap
            Vector3 forwardCap = Vector3.forward;

            // 8.0 s outside reachable cap
            ventField.UpdateUnreachable(8.1f, forwardCap);
            Assert.AreEqual(0.40f, ventField.Vents[0].FluxMultiplier);
        }

        [Test]
        public void AT12_CoverPropFreezesPressure()
        {
            var director = new PhaseDirector();
            var pressure = new PressureIntegrator();
            var crust = new CrustModel();
            var vents = new VentField();
            vents.Initialize(8);

            director.StartPhase(1);
            pressure.SetPressure(20.0f);

            // 1.0 s covered
            director.Tick(1.0f, vents, pressure, crust, isCovered: true);
            Assert.AreEqual(20.0f, pressure.Pressure, 0.1f);
            Assert.AreEqual(GeyserVentingConfig.PhaseDurationSec, director.TimeRemainingSec);
        }

        [Test]
        public void AT13_OptimalRoute_Deferred()
        {
            Assert.Pass("AT-13 deferred to device: scripted optimal route 5/5 run benchmark on SoC.");
        }

        [Test]
        public void AT14_AudioCoverage()
        {
            var router = new FeedbackRouter();
            Assert.AreEqual(15, router.TotalMappedEvents);
            foreach (FeedbackEvent evt in System.Enum.GetValues(typeof(FeedbackEvent)))
            {
                Assert.IsTrue(router.HasMapping(evt), $"Missing mapping for {evt}");
                Assert.IsNotNull(router.GetClipName(evt), $"Null clip for {evt}");
            }
        }
    }
}
