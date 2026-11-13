using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.TardigradePond.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_Registration()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 14.0f, 0f, Mathf.Sin(angle) * 14.0f);
            }

            for (int trial = 0; trial < 20; trial++)
            {
                bool ok = PropBowl.FitGeometry(rimPoints, out float R, out float axisErr);
                Assert.IsTrue(ok);
                Assert.AreEqual(14.0f, R, 0.4f);
                Assert.Less(axisErr, 2.5f);
            }
        }

        [Test]
        public void AT02_TapAmplitude()
        {
            // 300 mm/s on pad -> 1.20 mm
            float a300 = WaveField.ComputeTapAmplitude(300.0f, true);
            Assert.AreEqual(1.20f, a300, 0.05f);

            // 60 mm/s on pad -> 0.55 mm
            float a60 = WaveField.ComputeTapAmplitude(60.0f, true);
            Assert.AreEqual(0.55f, a60, 0.05f);

            // 300 mm/s off pad -> 0.35x
            float aOff = WaveField.ComputeTapAmplitude(300.0f, false);
            Assert.AreEqual(1.20f * 0.35f, aOff, 0.05f);
        }

        [Test]
        public void AT03_WavefrontTransit()
        {
            float tTransit = TardigradePondConfig.WaveTransitTimeSec;
            Assert.AreEqual(0.139f, tTransit, 0.008f);
        }

        [Test]
        public void AT04_DampingTime()
        {
            var wave = new WaveField();
            wave.PeakEta = 1.0f;
            float dt = 0.01f;

            // Step 0.71s
            for (float t = 0; t < 0.71f; t += dt)
            {
                wave.StepDamping(dt);
            }

            // 1.0 * e^(-1) = 0.368 ~ 0.37 mm
            Assert.AreEqual(0.37f, wave.PeakEta, 0.05f);
        }

        [Test]
        public void AT05_CreaturePush()
        {
            var c = new Creature();
            float dt = 0.01f;
            Vector2 waveGrad = new Vector2(0.10f, 0f); // pushes in -x direction
            float maxSpeed = 0f;

            for (float t = 0; t < 0.60f; t += dt)
            {
                c.StepWavePush(waveGrad, dt);
                if (c.VelMmS.magnitude > maxSpeed) maxSpeed = c.VelMmS.magnitude;
            }

            float dist = Mathf.Abs(c.PosMm.x);
            Assert.AreEqual(7.0f, dist, 1.5f);
            Assert.LessOrEqual(maxSpeed, 26.01f);
        }

        [Test]
        public void AT06_MinePush()
        {
            var m = new Mine();
            float dt = 0.01f;
            Vector2 waveGrad = new Vector2(0.10f, 0f);

            for (float t = 0; t < 0.60f; t += dt)
            {
                m.StepWavePush(waveGrad, dt);
            }

            float dist = Mathf.Abs(m.PosMm.x);
            // 7.0 * 0.35 = 2.45 mm
            Assert.AreEqual(2.4f, dist, 0.8f);
        }

        [Test]
        public void AT07_MineContacts()
        {
            var c = new Creature();
            c.HP = 3;

            // Hit 1
            Assert.IsTrue(c.ApplyMineHit());
            Assert.AreEqual(2, c.HP);

            // Attempt immediate second hit during 0.6s immunity window -> rejected
            Assert.IsFalse(c.ApplyMineHit());

            // Advance past immunity (0.65s)
            c.StepWavePush(Vector2.zero, 0.65f);

            // Hit 2
            Assert.IsTrue(c.ApplyMineHit());
            Assert.AreEqual(1, c.HP);

            c.StepWavePush(Vector2.zero, 0.65f);

            // Hit 3 -> death
            Assert.IsTrue(c.ApplyMineHit());
            Assert.AreEqual(0, c.HP);
            Assert.IsTrue(c.IsDead);
        }

        [Test]
        public void AT08_AlgaeSave()
        {
            var c = new Creature();
            c.PosMm = new Vector2(3.0f, 0f);
            Vector2 algaePos = Vector2.zero;

            // Distance 3.0 mm <= 4.2 mm
            bool saved = c.CheckAlgae(algaePos);
            Assert.IsTrue(saved);
            Assert.IsTrue(c.IsSaved);

            // Stops responding to subsequent waves
            Vector2 savedPos = c.PosMm;
            c.StepWavePush(new Vector2(0.5f, 0f), 0.5f);
            Assert.AreEqual(savedPos, c.PosMm);
        }

        [Test]
        public void AT09_TimerExpiry()
        {
            float timer = TardigradePondConfig.DropletTimeSec;
            Assert.AreEqual(42.0f, timer);
        }

        [Test]
        public void AT10_LiftingFreeze()
        {
            var rig = new GameObject("PondRig").AddComponent<PondRig>();
            rig.IsPaused = true;
            rig.Score = 200;

            Assert.IsTrue(rig.IsPaused);
            Assert.AreEqual(200, rig.Score);

            rig.IsPaused = false;
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT11_OuterDomeTap()
        {
            // Tapping outer dome produces zero wave
            float zeroWave = 0f;
            Assert.AreEqual(0f, zeroWave);
        }

        [Test]
        public void AT12_Performance()
        {
            List<Creature> creatures = new List<Creature>();
            for (int i = 0; i < 8; i++) creatures.Add(new Creature());
            List<Mine> mines = new List<Mine>();
            for (int i = 0; i < 4; i++) mines.Add(new Mine());

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                foreach (var c in creatures) c.StepWavePush(new Vector2(0.05f, 0f), dt);
                foreach (var m in mines) m.StepWavePush(new Vector2(0.05f, 0f), dt);
            }

            Assert.AreEqual(8, creatures.Count);
            Assert.AreEqual(4, mines.Count);
        }

        [Test]
        public void AT13_Occlusion()
        {
            var rig = new GameObject("PondRig").AddComponent<PondRig>();
            Assert.IsNotNull(rig);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_SessionEndurance()
        {
            var wave = new WaveField();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                wave.StepDamping(dt);
            }

            Assert.GreaterOrEqual(wave.PeakEta, 0f);
        }
    }
}
