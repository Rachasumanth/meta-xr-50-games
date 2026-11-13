using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator.Editor
{
    public class AcceptanceTests
    {
        [Test]
        public void Test01_SphereFit()
        {
            var reg = new BallRegistration();
            for (int i = 0; i < 120; i++)
            {
                reg.AddSample(new Vector3(0.02f * Mathf.Cos(i), 1.0f + 0.001f * Mathf.Sin(i), 0.02f * Mathf.Sin(i)), Vector3.up);
            }
            Assert.IsTrue(reg.IsRegistered);
            Assert.That(reg.MeasuredRadius, Is.EqualTo(BernoulliLevitatorConfig.BallRadiusMm).Within(0.25f));
            Assert.That(reg.RMS, Is.LessThan(BernoulliLevitatorConfig.MaxRmsMm));
        }

        [Test]
        public void Test02_PinchThresholds()
        {
            var tracker = new PinchTracker();
            // Test enter threshold at 25, 26, 27
            tracker.Update(0.025f, 0.035f);
            Assert.That(tracker.State, Is.EqualTo(PinchState.Pinched));

            // Reset
            tracker = new PinchTracker();
            tracker.Update(0.026f, 0.035f);
            Assert.That(tracker.State, Is.EqualTo(PinchState.Pinched));

            tracker = new PinchTracker();
            tracker.Update(0.027f, 0.035f);
            Assert.That(tracker.State, Is.EqualTo(PinchState.Released));

            // Release thresholds at 32, 33
            tracker = new PinchTracker();
            tracker.Update(0.025f, 0.035f); // pinch first
            tracker.Update(0.032f, 0.090f);
            Assert.That(tracker.State, Is.EqualTo(PinchState.Released));
        }

        [Test]
        public void Test03_HeightMapping()
        {
            var controller = new HeightController();
            controller.SetTarget(160f);
            for (int i = 0; i < 20; i++)
            {
                controller.Step(0.025f);
            }
            Assert.That(controller.TargetHeight, Is.EqualTo(160f).Within(0.5f));
        }

        [Test]
        public void Test05_NoTeleport()
        {
            var controller = new HeightController();
            controller.SetTarget(BernoulliLevitatorConfig.MaxHeightMm);
            for (int i = 0; i < 10; i++)
            {
                controller.Step(0.016f);
                Assert.That(Mathf.Abs(controller.Velocity), Is.LessThanOrEqualTo(BernoulliLevitatorConfig.MaxVelocityMps * 1000f + 1f));
            }
        }

        [Test]
        public void Test06_Spin()
        {
            var spin = new SpinFusion();
            spin.Step(new Vector3(0f, 0f, 360f), 0.1f);
            Assert.That(spin.AngularVelocity.z, Is.EqualTo(360f).Within(18f));
        }

        [Test]
        public void Test07_SlipClamp()
        {
            var spin = new SpinFusion();
            spin.Step(new Vector3(0f, 0f, 1000f), 0.1f);
            Assert.That(spin.AngularVelocity.magnitude, Is.LessThanOrEqualTo(BernoulliLevitatorConfig.MaxSlipAngularSpeedDeg));
        }

        [Test]
        public void Test08_RingPass()
        {
            var ring = new RingField();
            Assert.IsTrue(ring.CheckPass(44f));
            Assert.IsFalse(ring.CheckPass(46f));
        }

        [Test]
        public void Test09_PerfectBonus()
        {
            var ring = new RingField();
            Assert.IsTrue(ring.CheckPerfect(14f));
            Assert.IsFalse(ring.CheckPerfect(16f));
        }

        [Test]
        public void Test10_ReleaseAndCatch()
        {
            var orb = new OrbBody();
            orb.SetTargetHeight(120f);
            orb.SetFalling(true);
            for (int i = 0; i < 10; i++) orb.Step(0.05f, Vector3.zero);
            orb.SetFalling(false);
            orb.SetTargetHeight(120f);
            for (int i = 0; i < 20; i++) orb.Step(0.025f, Vector3.zero);
            Assert.That(orb.Position.y * 1000f, Is.EqualTo(120f).Within(10f));
        }

        [Test]
        public void Test11_ReleaseFailure()
        {
            var orb = new OrbBody();
            var pressure = new PressureState();
            orb.SetFalling(true);
            float t = 0f;
            while (orb.Position.y > 0f && t < 3.0f)
            {
                orb.Step(0.05f, Vector3.zero);
                t += 0.05f;
            }
            if (orb.Position.y <= 0f)
            {
                pressure.Penalty();
            }
            Assert.That(pressure.Pressure, Is.EqualTo(2));
        }

        [Test]
        public void Test13_FrameBudget()
        {
            var feedback = new FeedbackDirector();
            Assert.That(feedback.ParticleBudget, Is.LessThanOrEqualTo(650));
        }
    }
}
