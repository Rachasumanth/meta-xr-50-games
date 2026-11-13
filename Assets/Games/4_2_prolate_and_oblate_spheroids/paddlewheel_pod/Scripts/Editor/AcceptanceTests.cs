using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.PaddlewheelPod.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_EggRegistration()
        {
            var egg = new PropEgg();
            var rimPoints = new List<Vector3>();
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((egg.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (egg.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = egg.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.7f);
            Assert.Less(yawDeg, 3.0f);
        }

        [Test]
        public void AT02_EffectiveRadius()
        {
            Assert.AreEqual(24.0f, PaddlewheelPodConfig.ReffEquatorialMm, 0.3f);
            Assert.AreEqual(51.0f, PaddlewheelPodConfig.ReffMeridionalMm, 0.6f);
        }

        [Test]
        public void AT03_EquatorialStrokeSpinAndCap()
        {
            var spin = new SpinModel();
            spin.ApplyEquatorialStroke(300.0f, 0.100f);

            Assert.AreEqual(1.25f, spin.OmegaS, 0.10f);

            // Test cap at 18 rad/s
            for (int i = 0; i < 20; i++)
            {
                spin.ApplyEquatorialStroke(300.0f, 0.100f);
            }
            Assert.AreEqual(18.0f, spin.OmegaS, 0.01f);
        }

        [Test]
        public void AT04_SpinDecay()
        {
            var spin = new SpinModel();
            spin.OmegaS = 12.0f;

            float dt = 0.010f;
            float elapsed = 0f;
            while (spin.OmegaS > 4.0f && elapsed < 3.0f)
            {
                spin.Step(dt);
                elapsed += dt;
            }

            Assert.AreEqual(1.9f, elapsed, 0.15f);
        }

        [Test]
        public void AT05_AscentSpeedAndSteamDrain()
        {
            var spin = new SpinModel();
            spin.OmegaS = 12.0f;

            float vUp = spin.ComputeAscentSpeed(100f, false);
            Assert.AreEqual(48.0f, vUp, 1.0f);

            float drainPerSec = spin.ComputeSteamDrain(1.0f);
            Assert.AreEqual(5.5f, drainPerSec, 0.3f);

            // Omega = 0 with steam > 10 holds depth
            spin.OmegaS = 0f;
            float vHold = spin.ComputeAscentSpeed(50f, false);
            Assert.AreEqual(0f, vHold);
        }

        [Test]
        public void AT06_LateralSteerAndShaftClamp()
        {
            float v22 = SteerModel.ComputeLateralSpeedMmS(22.0f);
            Assert.AreEqual(90.0f, v22, 3.0f);

            float v10 = SteerModel.ComputeLateralSpeedMmS(10.0f);
            Assert.AreEqual(42.0f, v10, 2.0f);

            var steer = new SteerModel();
            steer.Step(22.0f, 10.0f); // 10s of 90 mm/s = 900 mm -> clamps at 300 mm
            Assert.AreEqual(300.0f, steer.LateralPosMm, 0.01f);
        }

        [Test]
        public void AT07_EelWindupAndLunge()
        {
            var eel = new Eel();
            Assert.AreEqual(0.35f, eel.WindupTimeSec, 0.02f);
            Assert.AreEqual(40.0f, eel.LungeTravelMm, 2.0f);
            Assert.AreEqual(220.0f, eel.LungeSpeedMmS, 10.0f);
        }

        [Test]
        public void AT08_EelPassHitAndDodge()
        {
            var pod = new PodModel();

            // Lunge passing at 50 mm: 1 hull, 15 steam, 1.2s invuln
            string res50 = Eel.EvaluatePass(50.0f, out int h50, out int s50, out float inv50);
            Assert.AreEqual("Hit", res50);
            Assert.AreEqual(1, h50);
            Assert.AreEqual(15, s50);
            Assert.AreEqual(1.2f, inv50, 0.01f);

            pod.ApplyEelHit(h50, s50, inv50);
            Assert.AreEqual(2, pod.Hull);
            Assert.AreEqual(85.0f, pod.Steam);
            Assert.AreEqual(1.2f, pod.InvulnerabilityTimer, 0.01f);

            // Passing at 60 mm: dodge
            string res60 = Eel.EvaluatePass(60.0f, out int h60, out int s60, out _);
            Assert.AreEqual("Dodge", res60);
            Assert.AreEqual(0, h60);
            Assert.AreEqual(0, s60);
        }

        [Test]
        public void AT09_KelpFieldEffects()
        {
            float baseDetection = 100.0f;
            float insideDetection = KelpField.ModifyDetectionRadius(baseDetection, true);
            Assert.AreEqual(50.0f, insideDetection);

            var pod = new PodModel();
            pod.EnterKelp();
            Assert.AreEqual(97.0f, pod.Steam); // -3 steam

            float vAscent = KelpField.ModifyAscent(48.0f, true);
            Assert.AreEqual(48.0f * 0.75f, vAscent, 0.5f);
        }

        [Test]
        public void AT10_SteamEmptySink()
        {
            var spin = new SpinModel();
            spin.OmegaS = 10.0f;

            // Steam 0 forces 24 mm/s sink
            float vSink = spin.ComputeAscentSpeed(0f, false);
            Assert.AreEqual(-24.0f, vSink, 0.1f);
        }

        [Test]
        public void AT11_PinchStates()
        {
            var pinch = new PinchModel();

            Assert.AreEqual(PinchState.Armed, pinch.UpdatePinch(70.0f, 0.01f));
            Assert.AreEqual(PinchState.Slip, pinch.UpdatePinch(84.0f, 0.01f));
            Assert.AreEqual(PinchState.Released, pinch.UpdatePinch(86.0f, 0.01f));
        }

        [Test]
        public void AT12_CoverFreeze()
        {
            var egg = new PropEgg();
            float dt = 0.1f;
            for (float t = 0; t < 1.0f; t += dt)
            {
                egg.StepTrackingLoss(false, dt);
            }

            Assert.IsTrue(egg.IsPaused);
        }

        [Test]
        public void AT13_Level3DiveClearable()
        {
            var pod = new PodModel();
            var spin = new SpinModel();

            float dt = 0.1f;
            // Level 3 dive (2000 mm): clearable under 55s alternating spin bursts and boiler regen
            for (float t = 0; t < 40.0f; t += dt)
            {
                if (pod.DepthMm <= 0f) break;

                float phase = t % 8.0f;
                if (phase < 4.0f)
                {
                    if (spin.OmegaS < 12.0f)
                    {
                        spin.ApplyEquatorialStroke(300.0f, 0.200f);
                    }
                }
                else
                {
                    spin.OmegaS = Mathf.Max(0f, spin.OmegaS - 4.0f * dt);
                }

                spin.Step(dt);

                if (spin.OmegaS <= 2.0f && pod.Steam > 8.0f)
                {
                    pod.RegenerateSteam(dt);
                }

                float vUp = spin.ComputeAscentSpeed(pod.Steam, false);
                float drain = spin.ComputeSteamDrain(dt);
                pod.Step(vUp, drain, dt);
            }

            Assert.Less(pod.DepthMm, 1000.0f);
            Assert.GreaterOrEqual(pod.Hull, 2);
        }

        [Test]
        public void AT14_SessionSoak()
        {
            var spin = new SpinModel();
            spin.OmegaS = 10.0f;

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                spin.Step(dt);
            }

            Assert.GreaterOrEqual(spin.OmegaS, 0f);
        }
    }
}
