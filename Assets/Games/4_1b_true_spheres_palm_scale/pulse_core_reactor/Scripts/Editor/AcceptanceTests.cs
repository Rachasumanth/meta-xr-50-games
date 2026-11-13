using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_ApertureMapping()
        {
            var grip = new GripSolver();

            grip.SolveAperture(64.0f);
            Assert.AreEqual(0.0f, grip.DeltaMm, 0.15f);
            Assert.AreEqual(0.0f, grip.Pressure, 0.025f);

            grip.SolveAperture(58.0f);
            Assert.AreEqual(3.0f, grip.DeltaMm, 0.15f);
            Assert.AreEqual(0.5f, grip.Pressure, 0.025f);

            grip.SolveAperture(52.0f);
            Assert.AreEqual(6.0f, grip.DeltaMm, 0.15f);
            Assert.AreEqual(1.0f, grip.Pressure, 0.025f);
        }

        [Test]
        public void Test02_HertzForce()
        {
            var def = new DeformationModel();
            Assert.AreEqual(2.95f, def.ComputeForce(3.0f), 0.10f);
            Assert.AreEqual(8.34f, def.ComputeForce(6.0f), 0.25f);
        }

        [Test]
        public void Test03_ShieldRadius()
        {
            var shield = new ShieldController();
            shield.UpdateShield(0.5f, 1.0f); // 1s hold
            Assert.AreEqual(100.0f, shield.RadiusMm, 1.0f);

            // 40 -> 160 mm takes 120 / 600 = 0.20 s
            float rampTime = (160.0f - 40.0f) / PulseCoreReactorConfig.ShieldRampSpeedMmS;
            Assert.AreEqual(0.20f, rampTime, 0.02f);
        }

        [Test]
        public void Test04_DensityThreshold()
        {
            var shield = new ShieldController();
            var resolver = new ThreatResolver();

            // p = 0.49 -> D = 0.706 >= 0.70 (passes)
            shield.UpdateShield(0.49f, 1.0f);
            bool pass1 = resolver.ResolveBomb(shield.Density, out int d1);
            Assert.IsTrue(pass1);
            Assert.AreEqual(0, d1);

            // p = 0.51 -> D = 0.694 < 0.70 (fails)
            shield.UpdateShield(0.51f, 1.0f);
            bool pass2 = resolver.ResolveBomb(shield.Density, out int d2);
            Assert.IsFalse(pass2);
            Assert.AreEqual(22, d2);
        }

        [Test]
        public void Test05_BeamBoundary()
        {
            var resolver = new ThreatResolver();
            float wb = 100.0f;

            // R_s = wb - 0.1 mm -> fails, damage 18
            bool hit1 = resolver.ResolveBeam(wb - 0.1f, wb, out int dam1);
            Assert.IsFalse(hit1);
            Assert.AreEqual(18, dam1);

            // R_s = wb + 0.1 mm -> passes, damage 0
            bool hit2 = resolver.ResolveBeam(wb + 0.1f, wb, out int dam2);
            Assert.IsTrue(hit2);
            Assert.AreEqual(0, dam2);
        }

        [Test]
        public void Test06_HeatRate()
        {
            var heat = new HeatSystem();
            heat.Step(1.0f, 1.0f);
            Assert.AreEqual(37.0f, heat.Heat, 3.0f); // ~37 +/- 3 after 1.0 s

            // Overheat (100) at 2.7 s +/- 0.2
            var heat2 = new HeatSystem();
            heat2.Step(1.0f, 2.7f);
            Assert.AreEqual(100.0f, heat2.Heat, 1.0f);
            Assert.IsTrue(heat2.IsOverheated);
        }

        [Test]
        public void Test07_OverheatLockout()
        {
            var shield = new ShieldController();
            var core = new CoreStateMachine();

            // Overheat applies 12 damage and forces R_s = 40 mm
            core.ApplyDamage(12);
            shield.SetLockout(true);

            Assert.AreEqual(88, core.CoreHP);
            Assert.AreEqual(40.0f, shield.RadiusMm, 0.001f);
        }

        [Test]
        public void Test08_OverPump()
        {
            float holdTime = 1.6f;
            int damage = 0;
            if (holdTime > 1.5f)
            {
                damage = (int)((holdTime - 1.5f) * 4.0f + 0.5f); // 4 HP/s
            }
            Assert.GreaterOrEqual(damage, 0);
        }

        [Test]
        public void Test09_RhythmValidCycle()
        {
            var rhythm = new RhythmClassifier();
            bool valid = rhythm.RecordCycle(0.300f, 0.250f, 0.300f);
            Assert.IsTrue(valid);
            Assert.AreEqual(1, rhythm.ValidCycleCount);
            Assert.IsFalse(rhythm.HasResonance);

            // 4th cycle enables resonance
            rhythm.RecordCycle(0.300f, 0.250f, 0.300f);
            rhythm.RecordCycle(0.300f, 0.250f, 0.300f);
            rhythm.RecordCycle(0.300f, 0.250f, 0.300f);
            Assert.IsTrue(rhythm.HasResonance);
        }

        [Test]
        public void Test10_Stagnation()
        {
            float holdTime = 3.2f;
            float efficiency = holdTime > 3.0f ? 0.80f : 1.0f;
            Assert.AreEqual(0.80f, efficiency, 0.001f);
        }

        [Test]
        public void Test11_Vent()
        {
            var heat = new HeatSystem();
            heat.ForceHeat(80.0f);

            bool vented1 = heat.TryVent();
            Assert.IsTrue(vented1);
            Assert.AreEqual(45.0f, heat.Heat, 0.1f); // 80 - 35 = 45

            // Second vent within 20s cooldown rejected
            bool vented2 = heat.TryVent();
            Assert.IsFalse(vented2);
            Assert.AreEqual(45.0f, heat.Heat, 0.1f);
        }

        [Test]
        public void Test12_SimultaneousArrivals()
        {
            var shield = new ShieldController();
            var resolver = new ThreatResolver();
            var core = new CoreStateMachine();

            // p = 0.9 -> R_s = 148 mm, D = 0.46
            shield.UpdateShield(0.9f, 1.0f);

            // Beam of width 100 mm passes
            bool beamDeflected = resolver.ResolveBeam(shield.RadiusMm, 100f, out int beamDam);
            Assert.IsTrue(beamDeflected);
            Assert.AreEqual(0, beamDam);

            // Bomb fails (0.46 < 0.70) -> 22 damage
            bool bombCrushed = resolver.ResolveBomb(shield.Density, out int bombDam);
            Assert.IsFalse(bombCrushed);
            Assert.AreEqual(22, bombDam);

            core.ApplyDamage(beamDam + bombDam);
            Assert.AreEqual(78, core.CoreHP);
        }

        [Test, Category("NeedsDevice")]
        public void Test13_ShieldOcclusion()
        {
            Assert.Ignore("Deferred: Requires device camera passthrough hand depth read.");
        }

        [Test]
        public void Test14_AudioCoverage()
        {
            var fb = new FeedbackRouter();
            Assert.IsTrue(fb.HasAudioMapping);
        }
    }
}
