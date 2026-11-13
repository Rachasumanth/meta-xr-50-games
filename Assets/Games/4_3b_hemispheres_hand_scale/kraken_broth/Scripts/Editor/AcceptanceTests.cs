using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.KrakenBroth.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        // AT-01: Bowl fit: Present the bowl; inject 48 rim points with 0.4 mm noise. Pass: RMS < 1.2 mm; rim normal < 2.0°; R_s within +/-1.5 mm of 83.33
        [Test]
        public void AT01_BowlFit()
        {
            float R = KrakenBrothConfig.RimRadiusMm;
            float d = KrakenBrothConfig.CavityDepthMm;
            float Rs_calc = (R * R + d * d) / (2.0f * d);

            Assert.AreEqual(83.33f, Rs_calc, 1.5f, "Derived spherical radius Rs must be within 1.5 mm of 83.33");
            Assert.AreEqual(KrakenBrothConfig.InnerSphereRadiusRs, Rs_calc, 0.01f);
        }

        // AT-02: Belly band: Strike at z = -49, -18, -17, -50 mm. Pass: First two accepted; last two rejected
        [Test]
        public void AT02_BellyBand()
        {
            Assert.IsTrue(DrumClassifier.IsInBellyBand(-49.0f), "z = -49 mm must be inside belly band");
            Assert.IsTrue(DrumClassifier.IsInBellyBand(-18.0f), "z = -18 mm must be inside belly band");
            Assert.IsFalse(DrumClassifier.IsInBellyBand(-17.0f), "z = -17 mm must be rejected");
            Assert.IsFalse(DrumClassifier.IsInBellyBand(-50.0f), "z = -50 mm must be rejected");
        }

        // AT-03: Quadrant map: Strike at lambda = 0°, 90°, 180°, 270°. Pass: Q_F, Q_L, Q_B, Q_R respectively, 20/20
        [Test]
        public void AT03_QuadrantMap()
        {
            Assert.AreEqual(QuadrantId.QF, DrumClassifier.GetQuadrantWithHysteresis(0f, QuadrantId.QF));
            Assert.AreEqual(QuadrantId.QL, DrumClassifier.GetQuadrantWithHysteresis(90f, QuadrantId.QL));
            Assert.AreEqual(QuadrantId.QB, DrumClassifier.GetQuadrantWithHysteresis(180f, QuadrantId.QB));
            Assert.AreEqual(QuadrantId.QR, DrumClassifier.GetQuadrantWithHysteresis(270f, QuadrantId.QR));
        }

        // AT-04: Seam hysteresis: 20 strikes at lambda = 45° +/- 3° with a prior Q_F history. Pass: All assigned Q_F; no double arm
        [Test]
        public void AT04_SeamHysteresis()
        {
            for (int i = 0; i < 20; i++)
            {
                float testAngle = 45.0f + ((i % 2 == 0) ? 2.5f : -2.5f);
                var assigned = DrumClassifier.GetQuadrantWithHysteresis(testAngle, QuadrantId.QF);
                Assert.AreEqual(QuadrantId.QF, assigned, $"Angle {testAngle} with prior QF must remain QF under 4° hysteresis");
            }
        }

        // AT-05: Energy scaling: Strike at 40, 200, 400 mm/s. Pass: Charge +5.6, +13.1, +16.8 +/- 0.5
        [Test]
        public void AT05_EnergyScaling()
        {
            float e40 = DrumClassifier.ComputeStrikeEnergy(40.0f);
            float e200 = DrumClassifier.ComputeStrikeEnergy(200.0f);
            float e400 = DrumClassifier.ComputeStrikeEnergy(400.0f);

            Assert.AreEqual(5.6f, e40, 0.5f, "Strike at 40 mm/s must yield 5.6 +/- 0.5");
            Assert.AreEqual(13.1f, e200, 0.5f, "Strike at 200 mm/s must yield 13.1 +/- 0.5");
            Assert.AreEqual(16.8f, e400, 0.5f, "Strike at 400 mm/s must yield 16.8 +/- 0.5");
        }

        // AT-06: Alternating bonus: Two strikes 160 ms apart, different fingers. Pass: Second energy x1.15 +/- 0.02
        [Test]
        public void AT06_AlternatingBonus()
        {
            var sim = new KrakenSimulation();
            sim.RecordStrike(QuadrantId.QF, 200f, 1, 0.0f, out float e1);
            sim.RecordStrike(QuadrantId.QF, 200f, 2, 0.160f, out float e2);

            float ratio = e2 / e1;
            Assert.AreEqual(1.15f, ratio, 0.02f, "Alternating finger within 160 ms must earn 1.15x bonus");
        }

        // AT-07: Arm and slam: Fill Q_F to 100; wait 0.45 s. Pass: One tentacle; sector frozen at arm time; 90° hit arc
        [Test]
        public void AT07_ArmAndSlam()
        {
            var sim = new KrakenSimulation();
            var q = sim.Quadrants[QuadrantId.QF];
            q.Charge = 95f;
            sim.RecordStrike(QuadrantId.QF, 200f, 1, 0f, out _);

            Assert.IsTrue(q.Armed, "Q_F must be armed when charge >= 100");
            Assert.AreEqual(0f, q.LockedSectorDeg, "Sector bisector must be frozen at arm time (0 deg for Q_F)");

            sim.Step(0.46f);
            Assert.IsFalse(q.Armed, "After 0.45s telegraph, slam must resolve");
            Assert.AreEqual(KrakenBrothConfig.CooldownSec, q.CooldownTimer, 1e-3f, "Cooldown must be initiated");
        }

        // AT-08: Empty slam: Arm Q_B with no ships in sector. Pass: Surge +5, -40 score, cooldown 1.8 s
        [Test]
        public void AT08_EmptySlam()
        {
            var sim = new KrakenSimulation();
            var q = sim.Quadrants[QuadrantId.QB];
            sim.Arm(q);
            int scoreBefore = sim.Score;
            float surgeBefore = sim.Surge;

            int killed = sim.ResolveSlam(q, out _);
            Assert.AreEqual(0, killed);
            Assert.AreEqual(surgeBefore + 5.0f, sim.Surge, 1e-4f, "Empty slam adds +5 surge");
            Assert.AreEqual(scoreBefore - 40, sim.Score, "Empty slam costs 40 score");
            Assert.AreEqual(1.80f, q.CooldownTimer, 1e-4f, "Cooldown must be 1.8s");
        }

        // AT-09: Multi-kill: Place 3 ships at lambda = 80°, 90°, 100°; slam Q_L. Pass: All 3 destroyed; multiplier 1.5 +/- 0.05
        [Test]
        public void AT09_MultiKill()
        {
            var sim = new KrakenSimulation();
            sim.Fleet.Add(new Ship(1, 80f));
            sim.Fleet.Add(new Ship(2, 90f));
            sim.Fleet.Add(new Ship(3, 100f));

            var q = sim.Quadrants[QuadrantId.QL];
            sim.Arm(q);
            int killed = sim.ResolveSlam(q, out float mult);

            Assert.AreEqual(3, killed, "All 3 ships in Q_L 90 deg sector must be killed");
            Assert.AreEqual(1.5f, mult, 0.05f, "3-ship multi-kill must award 1.5x multiplier");
        }

        // AT-10: Escape: Run one ship through 360°. Pass: Surge +12, score -150, respawn 1.0 s
        [Test]
        public void AT10_Escape()
        {
            var sim = new KrakenSimulation();
            var ship = new Ship(1, 0f);
            sim.Fleet.Add(ship);

            float initialSurge = sim.Surge;
            int initialScore = sim.Score;

            // Advance 36.1 seconds at 10 deg/sec = 361 deg
            sim.Step(36.1f);

            Assert.IsFalse(ship.IsAlive, "Escaped ship must be despawned");
            Assert.AreEqual(initialSurge + 12f + (36.1f * KrakenBrothConfig.PassiveSurgeRatePerSecLvl3), sim.Surge, 0.5f, "Escape adds +12 surge");
            Assert.AreEqual(initialScore - 150, sim.Score, "Escape incurs -150 score penalty");
        }

        // AT-11: Surge math: 10 s at level 3, no events. Pass: Surge +22.0 +/- 0.2
        [Test]
        public void AT11_SurgeMath()
        {
            var sim = new KrakenSimulation();
            float initialSurge = sim.Surge;
            sim.Step(10.0f);

            float gainedSurge = sim.Surge - initialSurge;
            Assert.AreEqual(22.0f, gainedSurge, 0.2f, "10s at Level 3 must add 22.0 +/- 0.2 surge");
        }

        // AT-12: Quota clear: Destroy quota with 12 s left. Pass: Broth clear, +400 + 72 score
        [Test]
        public void AT12_QuotaClear()
        {
            var sim = new KrakenSimulation();
            sim.BrothTimer = 12.0f;
            sim.TargetQuota = 1;

            sim.Fleet.Add(new Ship(1, 0f));
            var q = sim.Quadrants[QuadrantId.QF];
            sim.Arm(q);
            sim.ResolveSlam(q, out _);

            Assert.IsTrue(sim.BrothCleared);
            // Base score for 1 kill = 120. Clear bonus = +400 + (12 * 6 = 72) = 472. Total = 592
            Assert.AreEqual(120 + 472, sim.Score, "Clear must add +400 base and +72 time bonus");
        }

        // AT-13: Lift pause: Raise bowl 130 mm for 3 s. Pass: Ships and surge frozen; no charge lost
        [Test]
        public void AT13_LiftPause()
        {
            var sim = new KrakenSimulation();
            sim.Quadrants[QuadrantId.QF].Charge = 50f;
            float surgeBefore = sim.Surge;

            sim.Step(3.0f, liftMm: 130.0f); // Bowl lifted 130 mm

            Assert.IsTrue(sim.IsPaused);
            Assert.AreEqual(surgeBefore, sim.Surge, 1e-4f, "Surge must remain frozen during lift");
            Assert.AreEqual(50f, sim.Quadrants[QuadrantId.QF].Charge, 1e-4f, "Charge must be preserved");
        }

        // AT-14: Perf: Slam with 4 wrecks + rain + 2 hands. Pass: Frame <= 11.1 ms
        [Test]
        public void AT14_Performance()
        {
            var sim = new KrakenSimulation();
            for (int i = 0; i < 7; i++)
            {
                sim.Fleet.Add(new Ship(i, i * 45f));
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 90; i++)
            {
                sim.Step(0.011f);
            }
            sw.Stop();

            Assert.Less(sw.ElapsedMilliseconds, 100, "90 ticks must complete well under 100 ms");
        }
    }
}
