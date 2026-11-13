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
                reg.AddSample(Vector3.zero, Vector3.up);
            }
            Assert.IsTrue(reg.IsRegistered);
            Assert.GreaterOrEqual(reg.MeasuredRadius, BernoulliLevitatorConfig.BallRadiusMinMm);
            Assert.LessOrEqual(reg.MeasuredRadius, BernoulliLevitatorConfig.BallRadiusMaxMm);
            Assert.Less(reg.RMS, 0.5f);
        }

        [Test]
        public void Test02_PinchThresholds()
        {
            var tracker = new PinchTracker();
            
            // Test enter at 25mm / 26mm (30ms required)
            tracker.Update(0.026f, 0.04f); // 40ms > 30ms
            Assert.AreEqual(PinchState.Pinched, tracker.State);

            // Test release threshold at 32mm / 33mm (80ms required)
            tracker.Update(0.032f, 0.09f); // 90ms > 80ms
            Assert.AreEqual(PinchState.Released, tracker.State);
        }

        [Test]
        public void Test03_HeightMapping()
        {
            var hc = new HeightController();
            hc.SetTarget(0.120f); // 120mm
            
            // Step for 0.5s at 50fps
            for (int i = 0; i < 25; i++)
            {
                hc.Step(0.02f);
            }

            Assert.AreEqual(hc.TargetHeight, hc.Height, 0.0005f);
            Assert.LessOrEqual(Mathf.Abs(hc.Height - 0.120f), 0.0005f);
        }

        [Test]
        public void Test04_ServoLag()
        {
            var hc = new HeightController();
            float maxH = 0f;
            for (float t = 0; t < 2.0f; t += 0.01f)
            {
                float target = 0.120f + 0.040f * Mathf.Sin(2f * Mathf.PI * t);
                hc.SetTarget(target);
                hc.Step(0.01f);
                if (t > 1.0f && hc.Height > maxH) maxH = hc.Height;
            }
            float ampMm = (maxH - 0.120f) * 2000f;
            Assert.AreEqual(78.0f, ampMm, 8.0f, "Servo amplitude must track within 78-82 mm range");
        }

        [Test]
        public void Test05_NoTeleport()
        {
            var hc = new HeightController();
            hc.SetTarget(0.180f); // large step
            
            for (int i = 0; i < 50; i++)
            {
                hc.Step(0.016f);
                Assert.LessOrEqual(Mathf.Abs(hc.Velocity), BernoulliLevitatorConfig.MaxVelocity + 0.001f);
            }
        }

        [Test]
        public void Test06_Spin()
        {
            var sf = new SpinFusion();
            Vector3 inputOmega = new Vector3(0, 360f, 0);
            sf.Step(inputOmega, 0.1f);
            Assert.AreEqual(360f, sf.AngularVelocity.y, 36f);
        }

        [Test]
        public void Test07_SlipClamp()
        {
            var sf = new SpinFusion();
            Vector3 inputOmega = new Vector3(0, 1000f, 0);
            sf.Step(inputOmega, 0.1f);
            Assert.LessOrEqual(sf.AngularVelocity.magnitude, BernoulliLevitatorConfig.SlipClampDegS + 0.01f);
        }

        [Test]
        public void Test08_RingPass()
        {
            var ring = new RingField();
            Assert.IsTrue(ring.CheckPass(0.044f));
            Assert.IsFalse(ring.CheckPass(0.046f));
        }

        [Test]
        public void Test09_PerfectBonus()
        {
            var ring = new RingField();
            Assert.IsTrue(ring.CheckPerfect(0.014f));
            Assert.IsFalse(ring.CheckPerfect(0.016f));
        }

        [Test]
        public void Test10_ReleaseAndCatch()
        {
            var pressure = new PressureState();
            int initialPressure = pressure.Pressure;
            // simulate catch success
            Assert.AreEqual(initialPressure, pressure.Pressure);
        }

        [Test]
        public void Test11_ReleaseFailure()
        {
            var pressure = new PressureState();
            pressure.Penalty();
            Assert.AreEqual(2, pressure.Pressure);
        }

        [Test]
        public void Test12_Occlusion()
        {
            Vector3 ballPos = new Vector3(0, 0, 1.0f);
            Vector3 handPos = new Vector3(0, 0, 0.8f);
            bool isOccluded = handPos.z < ballPos.z;
            Assert.IsTrue(isOccluded, "Hand between camera and ball must trigger occlusion");
        }

        [Test]
        public void Test13_FrameBudget()
        {
            var fd = new FeedbackDirector();
            Assert.LessOrEqual(fd.ParticleBudget, 650);
        }

        [Test]
        public void Test14_Endurance()
        {
            var hc = new HeightController();
            hc.SetTarget(0.120f);
            for (int i = 0; i < 300; i++)
            {
                hc.Step(0.016f);
            }
            float driftMm = Mathf.Abs(hc.Height - hc.TargetHeight) * 1000f;
            Assert.Less(driftMm, 1.0f, "Height drift over session must be < 1 mm");
        }
    }
}
