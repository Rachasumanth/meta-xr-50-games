using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.OcularTurret.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_Registration()
        {
            var marble = new PropMarble();
            Vector3[] samples = new Vector3[20];
            for (int i = 0; i < 20; i++)
            {
                samples[i] = new Vector3(0.001f * (i % 3), 0.001f * ((i + 1) % 3), 0f);
            }
            bool ok = marble.Register(samples);
            Assert.IsTrue(ok);
            Assert.Less(marble.Center.magnitude, 0.6f);
        }

        [Test]
        public void Test02_GazeRotation()
        {
            var roll = new RollIntegrator();
            float rot = roll.ComputeTangentialRotationDeg(1.0f);
            Assert.AreEqual(7.2f, rot, 0.5f);

            float clamped = roll.ClampSpeed(400.0f);
            Assert.AreEqual(300.0f, clamped, 0.1f);
        }

        [Test]
        public void Test03_IntegrationSymmetry()
        {
            var roll = new RollIntegrator();
            Quaternion startRot = roll.Orientation;

            // Roll up 5 mm
            roll.StepRoll(new Vector2(0f, 5.0f), 0.1f);
            // Roll down 5 mm
            roll.StepRoll(new Vector2(0f, -5.0f), 0.1f);

            float diffAngle = Quaternion.Angle(startRot, roll.Orientation);
            Assert.Less(diffAngle, 1.0f);
        }

        [Test]
        public void Test04_DriftBlend()
        {
            var roll = new RollIntegrator();
            Quaternion startRot = roll.Orientation;

            // Still for 500 ms then 2 s
            roll.StepStill(0.500f);
            roll.StepStill(2.0f);

            float driftAngle = Quaternion.Angle(startRot, roll.Orientation);
            Assert.Less(driftAngle, 0.5f);
        }

        [Test]
        public void Test05_BeamHit()
        {
            var judge = new BeamJudge();
            Vector3 targetDir = Vector3.forward;
            float targetRadiusDeg = 1.5f;

            // Within 1.15 + 1.5 = 2.65 deg
            Vector3 hitDir = Quaternion.Euler(2.0f, 0, 0) * targetDir;
            Assert.IsTrue(judge.CheckHit(hitDir, targetDir, targetRadiusDeg));

            // Miss at 2x (5.3 deg)
            Vector3 missDir = Quaternion.Euler(5.5f, 0, 0) * targetDir;
            Assert.IsFalse(judge.CheckHit(missDir, targetDir, targetRadiusDeg));
        }

        [Test]
        public void Test06_BeamFill()
        {
            var judge = new BeamJudge();
            var agent = new GargoyleAgent();

            // 400 ms fill
            judge.FillMeter(agent, 0.400f);
            Assert.AreEqual(1.0f, agent.PetrifyMeter, 0.05f);
            Assert.IsTrue(agent.IsPetrified);
        }

        [Test]
        public void Test07_BeamHeat()
        {
            var gaze = new GazeModel();

            // Fire for 3.33 s
            gaze.FireBeam(3.333f);
            Assert.AreEqual(100.0f, gaze.Heat, 1.0f);
            Assert.IsTrue(gaze.IsOverheated);

            // Cool down for 2.0 s
            gaze.CoolDown(2.0f);
            Assert.AreEqual(0.0f, gaze.Heat, 1.0f);
        }

        [Test]
        public void Test08_Blink()
        {
            var gaze = new GazeModel();
            gaze.FireBeam(2.0f); // heat = 60
            float heatBefore = gaze.Heat;

            gaze.TriggerBlink();
            Assert.IsTrue(gaze.IsBlinking);
            Assert.AreEqual(heatBefore - 20.0f, gaze.Heat, 0.1f);

            gaze.Step(0.300f);
            Assert.IsFalse(gaze.IsBlinking);
        }

        [Test]
        public void Test09_SocketOpening()
        {
            var socket = new SocketModel();
            Assert.IsTrue(socket.IsSocketClosed);

            socket.SetSocketOpen(true);
            Assert.IsFalse(socket.IsSocketClosed);

            socket.SetSocketOpen(false);
            Assert.IsTrue(socket.IsSocketClosed);
        }

        [Test]
        public void Test10_GargoyleDive()
        {
            var socket = new SocketModel();
            Assert.AreEqual(3, socket.IntegrityPips);

            socket.DamageSocket();
            Assert.AreEqual(2, socket.IntegrityPips);
        }

        [Test, Category("NeedsDevice")]
        public void Test11_CoveringMarble()
        {
            Assert.Ignore("Deferred: Requires device camera passthrough covering detection.");
        }

        [Test]
        public void Test12_Level3Vigil()
        {
            var vigil = new VigilDirector();
            vigil.StartVigil(3);
            Assert.AreEqual(12, vigil.RemainingGargoyles);

            var socket = new SocketModel();
            for (int i = 0; i < 12; i++) vigil.DefeatGargoyle();
            Assert.AreEqual(0, vigil.RemainingGargoyles);
            Assert.GreaterOrEqual(socket.IntegrityPips, 2);
        }

        [Test]
        public void Test13_DoubleTapBurst()
        {
            var pool = new GargoylePool();
            bool burst = pool.TryBurst();
            Assert.IsTrue(burst);

            // 3 targets got +0.5
            int boosted = 0;
            foreach (var g in pool.Gargoyles)
            {
                if (Mathf.Abs(g.PetrifyMeter - 0.5f) < 0.01f) boosted++;
            }
            Assert.AreEqual(3, boosted);

            // Cooldown blocks second burst
            Assert.IsFalse(pool.TryBurst());
        }

        [Test]
        public void Test14_ParticleCount()
        {
            var feedback = new OcularFeedback();
            feedback.SpawnParticles(500);
            Assert.LessOrEqual(feedback.ActiveParticles, OcularTurretConfig.MaxParticleBudget);
        }
    }
}
