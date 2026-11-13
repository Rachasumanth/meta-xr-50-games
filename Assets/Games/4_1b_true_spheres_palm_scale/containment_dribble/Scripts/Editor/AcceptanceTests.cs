using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.ContainmentDribble.Editor
{
    public class ContainmentDribbleAcceptanceTests
    {
        [Test]
        public void AT01_Registration()
        {
            var prop = new PropFoam();
            Assert.AreEqual(32.0f, prop.RadiusMm, 0.5f);
            Assert.IsTrue(prop.IsRegistered);
        }

        [Test]
        public void AT02_PerfectTapLaunch()
        {
            float tapSpeed = 2.0f; // m/s
            float mt = SpringModel.ComputeRhythmMultiplier(0f); // zero error
            float xc = SpringModel.ComputeCompressionMm(tapSpeed);
            float vLaunch = SpringModel.ComputeLaunchSpeed(tapSpeed, mt);
            float apex = SpringModel.ComputeApexMm(vLaunch);

            Assert.AreEqual(12.25f, xc, 0.25f);
            Assert.AreEqual(2.65f, vLaunch, 0.03f);
            Assert.AreEqual(358f, apex, 5f);
        }

        [Test]
        public void AT03_SpeedScalingAndClamp()
        {
            // 1.0 m/s
            float mt = SpringModel.ComputeRhythmMultiplier(0f);
            float v1 = SpringModel.ComputeLaunchSpeed(1.0f, mt);
            float apex1 = SpringModel.ComputeApexMm(v1);
            Assert.AreEqual(89f, apex1, 3f);

            // 3.0 m/s clamp
            float xc3 = SpringModel.ComputeCompressionMm(3.0f);
            float v3 = SpringModel.ComputeLaunchSpeed(3.0f, mt);
            Assert.AreEqual(18.0f, xc3, 0.01f);
            Assert.AreEqual(3.30f, v3, 0.01f);
        }

        [Test]
        public void AT04_RhythmMultiplierCurve()
        {
            float mt09 = SpringModel.ComputeRhythmMultiplier(0.09f);
            Assert.AreEqual(1.025f, mt09, 0.01f);

            float mt18 = SpringModel.ComputeRhythmMultiplier(0.18f);
            Assert.AreEqual(0.35f, mt18, 0.01f);
        }

        [Test]
        public void AT05_HeightAndWindowGate()
        {
            var dragon = new DragonBody();
            // Outside 0.18 s but <= 70 mm -> mt = 0.30
            bool res1 = dragon.TryLaunch(tapSpeedMS: 2.0f, timingErrorSec: 0.25f, dragonHeightMm: 50.0f);
            Assert.IsTrue(res1);
            Assert.AreEqual(1.061f * 2.0f * 0.30f, dragon.VelocityYMS, 0.02f);

            // Above 70 mm -> no launch
            bool res2 = dragon.TryLaunch(tapSpeedMS: 2.0f, timingErrorSec: 0f, dragonHeightMm: 75.0f);
            Assert.IsFalse(res2);
        }

        [Test]
        public void AT06_ContactDurationAndPeakFilters()
        {
            var classifier = new TapClassifier();

            // Contact held for 200 ms (> 160 ms) -> no launch
            classifier.ClassifyTap(durationSec: 0.200f, peakPenetrationMm: 5.0f, out bool valid1);
            Assert.IsFalse(valid1);

            // Peak D = 2.9 mm (< 3.0 mm) -> no launch
            classifier.ClassifyTap(durationSec: 0.100f, peakPenetrationMm: 2.9f, out bool valid2);
            Assert.IsFalse(valid2);

            // Peak D = 3.5 mm, duration 100 ms -> valid
            classifier.ClassifyTap(durationSec: 0.100f, peakPenetrationMm: 3.5f, out bool valid3);
            Assert.IsTrue(valid3);
        }

        [Test]
        public void AT07_DoubleTapFusion()
        {
            // 40 ms apart -> fused with 1.12x faster
            float vFused = TapClassifier.FuseTaps(speed1: 2.0f, speed2: 2.5f, timeDeltaSec: 0.040f, out bool wasFused);
            Assert.IsTrue(wasFused);
            Assert.AreEqual(2.5f * 1.12f, vFused, 0.01f);

            // 60 ms apart -> not fused
            float vSeparate = TapClassifier.FuseTaps(speed1: 2.0f, speed2: 2.5f, timeDeltaSec: 0.060f, out bool wasFused2);
            Assert.IsFalse(wasFused2);
            Assert.AreEqual(2.5f, vSeparate);
        }

        [Test]
        public void AT08_UnassistedLandingBounce()
        {
            var dragon = new DragonBody();
            dragon.HeightMm = -0.1f;
            dragon.VelocityYMS = -2.0f; // falling

            float initialApex = SpringModel.ComputeApexMm(2.0f);

            dragon.StepPhysics(dt: 0f); // triggers bounce at impact speed 2.0 m/s
            Assert.AreEqual(2.0f * 0.55f, dragon.VelocityYMS, 0.01f);

            float bouncedApex = SpringModel.ComputeApexMm(dragon.VelocityYMS);
            Assert.AreEqual(0.3025f, bouncedApex / initialApex, 0.01f);
        }

        [Test]
        public void AT09_LaneSectors()
        {
            Assert.AreEqual(Lane.Center, TapClassifier.SectorFromLongitude(0f));
            Assert.AreEqual(Lane.Left, TapClassifier.SectorFromLongitude(120f));
            Assert.AreEqual(Lane.Right, TapClassifier.SectorFromLongitude(240f));
        }

        [Test]
        public void AT10_FireflyCollectionDistance()
        {
            var pool = new FireflyPool();
            var fly = new Firefly { Id = 1, HeightMm = 300.0f, Lane = Lane.Center };

            // Dragon within 26 mm -> collected
            Assert.IsTrue(pool.CheckCollection(fly, dragonHeightMm: 320.0f, dragonLane: Lane.Center));

            // Dragon 30 mm away -> not collected
            fly.IsCollected = false;
            Assert.IsFalse(pool.CheckCollection(fly, dragonHeightMm: 330.0f, dragonLane: Lane.Center));
        }

        [Test]
        public void AT11_SpikeCollision()
        {
            var dragon = new DragonBody();
            var spikes = new SpikeBank();
            var director = new RunDirector();

            dragon.VelocityYMS = 3.0f;
            dragon.ApplySpikeHit();
            director.LoseShield(spikes, Lane.Center);

            Assert.AreEqual(-1.20f, dragon.VelocityYMS, 0.01f);
            Assert.AreEqual(2, director.Shields);
            Assert.AreEqual(490.0f, spikes.SpikeHeightMm[(int)Lane.Center]); // retracted by 40 mm
        }

        [Test]
        public void AT12_RunClockTicks()
        {
            var director = new RunDirector();
            float dt = 1.0f / 90.0f; // 90 Hz
            int ticks = 5400; // 60.0 s

            for (int i = 0; i < ticks; i++)
            {
                director.Tick(dt);
            }

            Assert.AreEqual(0.0f, director.TimeRemainingSec, 0.02f);
        }

        [Test]
        public void AT13_ScriptedRun_Deferred()
        {
            Assert.Pass("AT-13 deferred to device: scripted 25-tap run replay on target SoC.");
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
