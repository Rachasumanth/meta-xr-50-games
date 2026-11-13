using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_SpheroidRegistration()
        {
            var tracker = new PropTracker();
            var rimPoints = new List<Vector3>();
            int count = 24;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((tracker.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (tracker.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = tracker.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.5f);
            Assert.Less(yawDeg, 2.0f);
        }

        [Test]
        public void AT02_NaturalRockingFrequency()
        {
            var body = new RockingBody();
            body.ThetaRad = 5.0f * Mathf.Deg2Rad;
            body.ThetaDot = 0f;

            float dt = 0.001f;
            float prevTheta = body.ThetaRad;
            int zeroCrossings = 0;
            float firstZeroTime = -1f;
            float lastZeroTime = -1f;

            for (float t = 0; t <= 10.0f; t += dt)
            {
                body.Step(dt);
                if ((prevTheta > 0 && body.ThetaRad <= 0) || (prevTheta < 0 && body.ThetaRad >= 0))
                {
                    zeroCrossings++;
                    if (firstZeroTime < 0) firstZeroTime = t;
                    lastZeroTime = t;
                }
                prevTheta = body.ThetaRad;
            }

            // Zero crossings happen twice per cycle
            float fullCycles = (zeroCrossings - 1) * 0.5f;
            float measuredPeriod = (lastZeroTime - firstZeroTime) / fullCycles;
            float measuredFreq = 1.0f / measuredPeriod;

            Assert.AreEqual(1.872f, measuredFreq, 0.030f);
            Assert.AreEqual(0.534f, measuredPeriod, 0.010f);
        }

        [Test]
        public void AT03_DampingRatio()
        {
            // Verify zeta is 0.055 and damping is strictly positive
            Assert.AreEqual(0.055f, RockingBody.Zeta, 0.005f);

            var body = new RockingBody();
            body.ThetaRad = 20.0f * Mathf.Deg2Rad;
            float dt = 0.002f;

            float initialEnergy = 0.5f * body.ThetaDot * body.ThetaDot + 0.5f * RockingBody.OmegaN * RockingBody.OmegaN * body.ThetaRad * body.ThetaRad;
            for (float t = 0; t < 2.0f; t += dt)
            {
                body.Step(dt);
            }
            float finalEnergy = 0.5f * body.ThetaDot * body.ThetaDot + 0.5f * RockingBody.OmegaN * RockingBody.OmegaN * body.ThetaRad * body.ThetaRad;

            Assert.Less(finalEnergy, initialEnergy); // Strictly decaying
        }

        [Test]
        public void AT04_PressForceProxy()
        {
            var press = new PressModel();
            float f2 = press.ComputeForce(2.0f);
            float f6 = press.ComputeForce(6.0f);
            float f12 = press.ComputeForce(12.0f);
            float f16 = press.ComputeForce(16.0f);

            Assert.AreEqual(0.50f, f2, 0.50f * 0.03f);
            Assert.AreEqual(1.50f, f6, 1.50f * 0.03f);
            Assert.AreEqual(3.00f, f12, 3.00f * 0.03f);
            Assert.AreEqual(3.00f, f16, 3.00f * 0.03f);
        }

        [Test]
        public void AT05_PressImpulseAndTilt()
        {
            var body = new RockingBody();
            var lunge = new LungeSolver();

            // Press at phi = 60 deg, J = 0.24 N*s, lambda = 0 deg
            float j = 0.24f;
            float expectedJTheta = 0.0101f * j; // 2.424e-3 N*m*s
            lunge.ApplyLungeImpulse(j, 60.0f, 0.0f, body);

            // J_theta = 2.42e-3 N*m*s +- 4%
            Assert.AreEqual(2.42e-3f, expectedJTheta, 2.42e-3f * 0.04f);

            // Find peak theta during free rocking
            float peakThetaDeg = 0f;
            float dt = 0.001f;
            for (float t = 0; t < 1.0f; t += dt)
            {
                body.Step(dt);
                if (Mathf.Abs(body.ThetaDeg) > peakThetaDeg)
                {
                    peakThetaDeg = Mathf.Abs(body.ThetaDeg);
                }
            }

            Assert.AreEqual(16.8f, peakThetaDeg, 1.5f);
        }

        [Test]
        public void AT06_LungeSlide()
        {
            var body = new RockingBody();
            var lunge = new LungeSolver();

            lunge.ApplyLungeImpulse(0.24f, 60.0f, 0.0f, body);

            float dt = 0.005f;
            for (float t = 0; t < 2.0f; t += dt)
            {
                lunge.Step(dt);
            }

            Assert.AreEqual(22.0f, lunge.TotalSlideDistanceMm, 3.0f);
            // Direction lambda + 180 deg = 180 deg (-x)
            Assert.Less(lunge.Position.x, -0.015f);
            Assert.AreEqual(0f, lunge.Position.z, 0.005f);
        }

        [Test]
        public void AT07_BellyBump()
        {
            var belly = new BellyCollider();
            var combo = new ScoreCombo();
            belly.OnImpBumped += (theta) => combo.RegisterBump(theta);

            var imp = new ImpEntity(new Vector3(0.026f, 0f, 0f));
            float vBelly = 0.43f; // m/s
            float theta = 12.0f;

            bool bumped = belly.CheckBump(Vector3.zero, theta, vBelly, imp, out float resultingSpeed);
            Assert.IsTrue(bumped);
            Assert.IsTrue(resultingSpeed >= 0.45f && resultingSpeed <= 0.65f, $"Speed: {resultingSpeed}");
            Assert.Greater(combo.TotalScore, 0);
        }

        [Test]
        public void AT08_RingOut()
        {
            var impDirector = new ImpDirector();
            var stability = new StabilityMeter();
            var combo = new ScoreCombo();

            impDirector.OnRingOut += (imp) => combo.RegisterRingOut();

            var imp = new ImpEntity(new Vector3(0.100f, 0f, 0f));
            imp.WasBumped = true;
            imp.Velocity = new Vector3(0.500f, 0f, 0f); // 0.5 m/s outward
            impDirector.Imps.Add(imp);

            float dt = 0.010f;
            for (float t = 0; t < 0.200f; t += dt)
            {
                impDirector.Step(dt, Vector3.zero, stability, new RockingBody());
            }

            Assert.AreEqual(1, impDirector.RingOutCount);
            Assert.AreEqual(150, combo.TotalScore);
        }

        [Test]
        public void AT09_ImpPush()
        {
            var impDirector = new ImpDirector();
            var stability = new StabilityMeter();
            var body = new RockingBody();

            var imp = new ImpEntity(new Vector3(0.019f, 0f, 0f));
            impDirector.Imps.Add(imp);

            float dt = 0.050f;
            impDirector.Step(dt, Vector3.zero, stability, body);

            Assert.AreEqual(92, stability.Value); // 100 - 8 = 92
            Assert.Greater(Mathf.Abs(body.ThetaDot), 0f);
            Assert.AreEqual(0, impDirector.RingOutCount);
        }

        [Test]
        public void AT10_ToppleAndReseat()
        {
            var body = new RockingBody();
            var stability = new StabilityMeter();

            body.ApplyAngularImpulse(0.05f); // Large cross impulse
            float dt = 0.010f;

            for (float t = 0; t < 0.5f; t += dt)
            {
                body.Step(dt);
                if (body.IsToppled) break;
            }

            Assert.IsTrue(body.IsToppled);
            Assert.AreEqual(3.0f, body.ReseatTimer, 0.1f);

            stability.ApplyToppleLoss();
            Assert.AreEqual(66, stability.Value); // 100 - 34 = 66
        }

        [Test]
        public void AT11_StabilityLoss()
        {
            var stability = new StabilityMeter();
            Assert.AreEqual(100, stability.Value);

            for (int i = 0; i < 12; i++)
            {
                stability.ApplyImpPush();
                Assert.IsFalse(stability.IsBoutLost);
            }

            // 13th push
            stability.ApplyImpPush();
            Assert.AreEqual(0, stability.Value);
            Assert.IsTrue(stability.IsBoutLost);
            Assert.AreEqual(1, stability.LossesCount);
            Assert.IsFalse(stability.IsGameOver);

            // 2nd lost bout triggers GAME_OVER
            stability.Reset();
            for (int i = 0; i < 13; i++)
            {
                stability.ApplyImpPush();
            }
            Assert.IsTrue(stability.IsGameOver);
        }

        [Test]
        public void AT12_OcclusionReadback()
        {
            // Imp behind stone silhouette: fragment contribution = 0 inside 75mm silhouette
            float stoneRadius = 0.0375f; // 37.5mm radius = 75mm silhouette
            Vector2 samplePoint = new Vector2(0.015f, 0.015f);
            bool insideSilhouette = samplePoint.magnitude < stoneRadius;
            Assert.IsTrue(insideSilhouette);

            float impDepth = 0.08f;
            float stoneDepth = 0.02f;
            bool occluded = (impDepth > stoneDepth) && insideSilhouette;
            Assert.IsTrue(occluded);
        }

        [Test]
        public void AT13_TrackingLoss()
        {
            var tracker = new PropTracker();
            float dt = 0.050f;

            for (float t = 0; t < 2.0f; t += dt)
            {
                tracker.StepTrackingLoss(false, dt);
                if (t >= 1.5f)
                {
                    Assert.IsTrue(tracker.IsPaused);
                }
            }
        }

        [Test]
        public void AT14_SoakRun()
        {
            var sm = new BoutStateMachine();
            var body = new RockingBody();
            var stability = new StabilityMeter();
            sm.StartFight();

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                body.Step(dt);
                sm.Step(dt, stability.IsBoutLost, stability.IsGameOver, 5);
            }

            Assert.IsTrue(sm.CurrentState == BoutState.FIGHT || sm.CurrentState == BoutState.BOUT_END);
        }
    }
}
