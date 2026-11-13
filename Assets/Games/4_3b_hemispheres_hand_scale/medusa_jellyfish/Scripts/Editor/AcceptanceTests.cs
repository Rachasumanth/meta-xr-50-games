using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.MedusaJellyfish.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        // 1. Contraction mapping: an 8 N combined press yields kappa = 1.0 and c_b = 6 mm.
        [Test]
        public void AT01_ContractionMapping()
        {
            var solver = new PalmPressSolver();
            float kappa = solver.ComputeKappa(4.0f, 4.0f); // 8 N combined
            float compression = solver.ComputeCompression(kappa);

            Assert.AreEqual(1.0f, kappa, 1e-4f, "Combined 8N press must yield kappa = 1.0");
            Assert.AreEqual(6.0f, compression, 1e-4f, "Combined 8N press must yield compression = 6 mm");
        }

        // 2. Jet impulse: a full press-release raises v_y by 0.46 +/- 0.05 m/s.
        [Test]
        public void AT02_JetImpulse()
        {
            var jet = new JetModel();
            float deltaV = jet.ApplyJetImpulse(1.0f);

            Assert.AreEqual(0.46f, deltaV, 0.05f, "Full jet impulse deltaV must be 0.46 +/- 0.05 m/s");
        }

        // 3. Jet height: a full jet gains 40-90 mm of virtual depth.
        [Test]
        public void AT03_JetHeight()
        {
            var jet = new JetModel();
            float initialDepth = jet.DepthMm;
            jet.ApplyJetImpulse(1.0f);
            float gainedHeight = initialDepth - jet.DepthMm;

            Assert.GreaterOrEqual(gainedHeight, 40.0f, "Jet height must be at least 40 mm");
            Assert.LessOrEqual(gainedHeight, 90.0f, "Jet height must be at most 90 mm");
        }

        // 4. Drift: with no input the bell sinks at 8 +/- 1 mm/s.
        [Test]
        public void AT04_Drift()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            float initialDepth = sim.Jet.DepthMm;
            sim.Step(1.0f, 0f); // 1 second of drift
            float sinkAmount = sim.Jet.DepthMm - initialDepth;

            Assert.AreEqual(8.0f, sinkAmount, 1.0f, "Bell must sink at 8 +/- 1 mm/s during drift");
        }

        // 5. Steering: a 20 deg tilt produces 41 +/- 5 mm/s lateral speed.
        [Test]
        public void AT05_Steering()
        {
            var jet = new JetModel();
            float lateralSpeed = jet.ComputeSteerSpeed(20.0f);

            Assert.AreEqual(41.0f, lateralSpeed, 5.0f, "20 deg tilt must produce 41 +/- 5 mm/s lateral speed");
        }

        // 6. Oxygen: a jet costs exactly 2 units; drifting costs 0.6/s.
        [Test]
        public void AT06_Oxygen()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            Assert.AreEqual(100.0f, sim.Oxygen.CurrentOxygen, 1e-4f);

            bool jetSuccess = sim.TryExecuteJet(4.0f, 4.0f, out _);
            Assert.IsTrue(jetSuccess);
            Assert.AreEqual(98.0f, sim.Oxygen.CurrentOxygen, 1e-4f, "Jet must cost exactly 2 oxygen units");

            sim.SetState(GameState.Drift);
            sim.Step(2.0f, 0f); // 2 seconds of drift: 2 * 0.6 = 1.2
            Assert.AreEqual(96.8f, sim.Oxygen.CurrentOxygen, 0.05f, "Drift must drain 0.6 oxygen/s");
        }

        // 7. Mine strike: a tentacle within 6 mm detonates and costs 18 oxygen.
        [Test]
        public void AT07_MineStrike()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            Vector3 firstSegPos = sim.Tentacles.Segments[0].Position;
            Vector3 nearMinePos = firstSegPos + new Vector3(3.0f, 0, 0); // 3 mm away (within 6 mm)

            bool hit = sim.CheckMineCollision(nearMinePos, 6.0f);
            Assert.IsTrue(hit, "Tentacle within 6 mm must trigger mine collision");
            Assert.AreEqual(82.0f, sim.Oxygen.CurrentOxygen, 1e-4f, "Mine strike must cost 18 oxygen");
            Assert.AreEqual(1, sim.MineStrikes);
        }

        // 8. Stun: after a strike, jets are blocked for 1.0 s.
        [Test]
        public void AT08_Stun()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            sim.OnMineDetonation();

            Assert.AreEqual(1.0f, sim.StunTimer, 1e-4f);
            bool jetBlocked = sim.TryExecuteJet(4.0f, 4.0f, out _);
            Assert.IsFalse(jetBlocked, "Jets must be blocked during stun");

            sim.Step(1.05f, 0f);
            Assert.AreEqual(0f, sim.StunTimer, 1e-4f);
            bool jetAllowed = sim.TryExecuteJet(4.0f, 4.0f, out _);
            Assert.IsTrue(jetAllowed, "Jets must be permitted after stun expires");
        }

        // 9. Bloom: five clean contractions grant a +50% jet height.
        [Test]
        public void AT09_Bloom()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            for (int i = 0; i < 4; i++)
            {
                sim.TryExecuteJet(4.0f, 4.0f, out _); // clean (kappa = 1.0 >= 0.7)
                sim.SetState(GameState.Drift);
            }
            Assert.IsFalse(sim.BloomActive, "Bloom should not be active after 4 clean contractions");

            sim.TryExecuteJet(4.0f, 4.0f, out _); // 5th clean contraction
            Assert.IsTrue(sim.BloomActive, "Bloom must be active after 5 clean contractions");

            sim.SetState(GameState.Drift);
            float depthBeforeBloomJet = sim.Jet.DepthMm;
            sim.TryExecuteJet(4.0f, 4.0f, out _); // Bloom jet executes
            float bloomHeight = depthBeforeBloomJet - sim.Jet.DepthMm;

            Assert.AreEqual(70.0f * 1.5f, bloomHeight, 1.0f, "Bloom jet must grant +50% height");
        }

        // 10. Surface: reaching z <= 0 clears the dive exactly once.
        [Test]
        public void AT10_Surface()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            sim.Jet.DepthMm = 1.0f;

            sim.TryExecuteJet(4.0f, 4.0f, out _); // brings depth below 0
            sim.Step(0.016f);

            Assert.AreEqual(GameState.DiveClear, sim.State);
            Assert.AreEqual(1, sim.DivesCleared);

            // Step again at surface
            sim.Step(0.016f);
            Assert.AreEqual(1, sim.DivesCleared, "DivesCleared must increment exactly once");
        }

        // 11. Lose: oxygen 0 or three strikes end the dive.
        [Test]
        public void AT11_Lose()
        {
            var simA = new JellyfishSimulation(Vector3.zero);
            simA.Oxygen.CurrentOxygen = 1.0f;
            simA.Step(2.0f); // drains oxygen below 0
            Assert.AreEqual(GameState.Score, simA.State, "Oxygen reaching 0 must transition to Score");

            var simB = new JellyfishSimulation(Vector3.zero);
            simB.OnMineDetonation();
            simB.OnMineDetonation();
            simB.OnMineDetonation();
            Assert.AreEqual(3, simB.MineStrikes);
            Assert.AreEqual(GameState.Score, simB.State, "Three mine strikes must transition to Score");
        }

        // 12. One palm: half-strength contraction still produces a jet (reduced).
        [Test]
        public void AT12_OnePalm()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            bool executed = sim.TryExecuteJet(4.0f, 0f, out float deltaV); // Left palm 4N, right palm 0N -> kappa = 0.5

            Assert.IsTrue(executed, "One palm at 4N (kappa = 0.5 >= 0.35) must trigger a jet");
            Assert.AreEqual(0.23f, deltaV, 0.05f, "Half-strength contraction deltaV must be approx 0.23 m/s");
            Assert.AreEqual(35.0f, sim.Jet.LastJetHeightMm, 1.0f, "Half-strength contraction height must be 35 mm");
        }

        // 13. Occlusion: a mine behind the bell is dimmed but visible.
        [Test]
        public void AT13_Occlusion()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            Vector3 bellPos = Vector3.zero;
            Vector3 behindMine = new Vector3(10.0f, 0, 50.0f); // Behind bell in Z
            Vector3 frontMine = new Vector3(10.0f, 0, -50.0f); // In front of bell

            OcclusionState stateBehind = sim.EvaluateOcclusion(behindMine, bellPos);
            OcclusionState stateFront = sim.EvaluateOcclusion(frontMine, bellPos);

            Assert.AreEqual(OcclusionState.DimmedVisible, stateBehind, "Mine behind bell must be dimmed visible");
            Assert.AreEqual(OcclusionState.DirectVisible, stateFront, "Mine in front of bell must be directly visible");
        }

        // 14. Performance: bell + 6 tentacles + 8 mines sustain 90 fps.
        [Test]
        public void AT14_Performance()
        {
            var sim = new JellyfishSimulation(Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                sim.Mines.Add(new SeaMine(new Vector3(i * 10.0f, -i * 20.0f, 10.0f)));
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 90; i++)
            {
                sim.Step(0.011f, 10.0f);
            }
            sw.Stop();

            Assert.Less(sw.ElapsedMilliseconds, 100, "90 simulation steps with 6 tentacles and 8 mines must complete well under 100 ms");
        }
    }
}
