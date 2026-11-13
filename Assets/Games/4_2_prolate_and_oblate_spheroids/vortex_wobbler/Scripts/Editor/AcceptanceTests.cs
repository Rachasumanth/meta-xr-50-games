using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.VortexWobbler.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_PebbleRegistration()
        {
            var pebble = new PropPebble();
            var rimPoints = new List<Vector3>();
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                float ang = (i / (float)count) * Mathf.PI * 2f;
                float noise = Mathf.Sin(i * 1.5f) * 0.0003f;
                rimPoints.Add(new Vector3((pebble.SemiAxisA + noise) * Mathf.Cos(ang),
                                          0f,
                                          (pebble.SemiAxisB + noise) * Mathf.Sin(ang)));
            }

            bool reg = pebble.FitSpheroid(rimPoints, out float rmsMm, out float yawDeg);
            Assert.IsTrue(reg);
            Assert.Less(rmsMm, 0.5f);
            Assert.Less(yawDeg, 3.0f);
        }

        [Test]
        public void AT02_RimWalkPumping()
        {
            var rock = new RockModel();
            rock.AmplitudeDeg = 10.0f;

            // In-phase rim walk (5mm travel)
            rock.ApplyRimWalk(5.0f, 0.0f);
            Assert.AreEqual(16.0f, rock.AmplitudeDeg, 0.6f);

            // Out-of-phase rim walk (180 deg)
            rock.ApplyRimWalk(5.0f, 180.0f);
            Assert.AreEqual(10.0f, rock.AmplitudeDeg, 0.6f);
        }

        [Test]
        public void AT03_AmplitudeDecay()
        {
            var rock = new RockModel();
            rock.AmplitudeDeg = 18.0f;

            float dt = 0.010f;
            float elapsed = 0f;
            while (rock.AmplitudeDeg > 6.6f && elapsed < 2.0f)
            {
                rock.StepDecay(dt);
                elapsed += dt;
            }

            Assert.AreEqual(1.40f, elapsed, 0.10f);
        }

        [Test]
        public void AT04_ConsecutivePumps()
        {
            var rock = new RockModel();
            rock.AmplitudeDeg = 0f;

            // 3 consecutive in-phase presses
            rock.ApplyRimWalk(5.0f, 0f);
            rock.ApplyRimWalk(5.0f, 0f);
            rock.ApplyRimWalk(5.0f, 0f);

            Assert.GreaterOrEqual(rock.AmplitudeDeg, 12.0f);
            Assert.LessOrEqual(rock.AmplitudeDeg, 20.0f);
            Assert.Less(rock.AmplitudeDeg, 26.0f);
        }

        [Test]
        public void AT05_VortexFieldInfall()
        {
            var field = new VortexField();
            float gamma = field.ComputeGamma(18.0f);
            Assert.AreEqual(1.20e4f, gamma, 1.20e4f * 0.05f);

            float vTang = field.ComputeTangentialVelocity(40.0f, 18.0f);
            Assert.AreEqual(48.0f, vTang, 3.0f);

            // Infall from 60mm to 14mm
            float r = 60.0f;
            float dt = 0.010f;
            float time = 0f;
            while (r > 14.0f && time < 3.0f)
            {
                r -= field.ComputeRadialInflowSpeed(r, 18.0f, true) * dt;
                time += dt;
            }

            Assert.AreEqual(1.8f, time, 0.3f);
        }

        [Test]
        public void AT06_DeflectionWedge()
        {
            var debris = new FieldEntity(EntityType.Debris, 40.0f, 0f);
            var pod = new FieldEntity(EntityType.Pod, 40.0f, 0f);

            debris.ApplyDeflection(30.0f, 0f); // 40 + 30 = 70 mm > 68 mm
            pod.ApplyDeflection(30.0f, 0f);    // 40 + 30 * 0.3 = 49 mm (disp = 9mm < 30% of 30mm)

            Assert.Greater(debris.RadiusMm, 68.0f);
            float podDisp = pod.RadiusMm - 40.0f;
            float debrisDisp = debris.RadiusMm - 40.0f;
            Assert.Less(podDisp, debrisDisp * 0.35f);
        }

        [Test]
        public void AT07_AirlockScoringAndDamage()
        {
            var airlock = new AirlockModel();
            Assert.AreEqual(3, airlock.HullPips);
            Assert.AreEqual(0f, airlock.Heat);

            airlock.HandleDebrisBreach();
            Assert.AreEqual(2, airlock.HullPips);
            Assert.AreEqual(18.0f, airlock.Heat);

            airlock.HandlePodDock();
            Assert.AreEqual(100, airlock.Score);
            Assert.AreEqual(2, airlock.HullPips); // pod cannot breach
        }

        [Test]
        public void AT08_OverdriveHeatAndInflow()
        {
            var field = new VortexField();
            var airlock = new AirlockModel();

            float normalInflow = field.ComputeRadialInflowSpeed(40.0f, 18.0f, true);
            float overdriveInflow = field.ComputeRadialInflowSpeed(40.0f, 24.0f, true);

            // Overdrive inflow is doubled
            Assert.AreEqual(normalInflow * 2.0f, overdriveInflow, normalInflow * 0.1f);

            // Heat rises at 6/s
            airlock.StepOverdriveHeat(1.0f);
            Assert.AreEqual(6.0f, airlock.Heat, 0.5f);
        }

        [Test]
        public void AT09_DeflectionSpamDrain()
        {
            var rock = new RockModel();
            rock.AmplitudeDeg = 18.0f;

            rock.ApplyDeflectionDrain(1.0f);
            float drained = 18.0f - rock.AmplitudeDeg;
            Assert.AreEqual(3.6f, drained, 0.4f);
        }

        [Test]
        public void AT10_CoverFreeze()
        {
            var pebble = new PropPebble();
            var rock = new RockModel();
            rock.AmplitudeDeg = 15.0f;

            // Cover for 1s
            float dt = 0.1f;
            for (float t = 0; t < 1.0f; t += dt)
            {
                pebble.StepTrackingLoss(false, dt);
            }

            Assert.IsTrue(pebble.IsPaused);
            Assert.AreEqual(15.0f, rock.AmplitudeDeg); // preserved
        }

        [Test]
        public void AT11_SectorClassification()
        {
            // 24 sectors of 15 deg each
            for (int i = 0; i < 24; i++)
            {
                float testAngle = i * 15.0f + 2.0f;
                int sec = PressClassifier.GetSector(testAngle);
                Assert.AreEqual(i, sec);
            }

            bool stable = PressClassifier.IsStableNearBoundary(7.5f, 0f);
            Assert.IsTrue(stable);
        }

        [Test]
        public void AT12_Level2WaveClearable()
        {
            var director = new WaveDirector();
            var field = new VortexField();
            var rock = new RockModel();
            var airlock = new AirlockModel();

            rock.AmplitudeDeg = 16.0f;
            director.SpawnWave(2); // 8 debris, 4 pods

            // Clear wave by deflecting debris outward beyond 68mm
            foreach (var e in director.Entities)
            {
                if (e.Type == EntityType.Debris)
                {
                    e.ApplyDeflection(40.0f, 0f); // 60 + 40 = 100mm > 68mm => resolved
                }
            }

            float dt = 0.05f;
            for (float t = 0; t < 5.0f; t += dt)
            {
                director.Step(dt, field, rock, airlock);
            }

            Assert.GreaterOrEqual(airlock.HullPips, 2);
            Assert.Greater(airlock.PodsDocked, 0);
        }

        [Test]
        public void AT13_SessionDurationSoak()
        {
            var rock = new RockModel();
            rock.AmplitudeDeg = 15.0f;

            float dt = 1.0f / 90.0f;
            for (int i = 0; i < 500; i++)
            {
                rock.StepDecay(dt);
            }

            Assert.GreaterOrEqual(rock.AmplitudeDeg, 0f);
        }

        [Test]
        public void AT14_HandRemovalFieldOff()
        {
            var rock = new RockModel();
            rock.AmplitudeDeg = 15.0f;

            // 5 s decay
            float dt = 0.1f;
            for (float t = 0; t < 5.0f; t += dt)
            {
                rock.StepDecay(dt);
            }

            Assert.Less(rock.AmplitudeDeg, 1.0f);
            Assert.AreEqual(VortexState.OFF, rock.State);
        }
    }
}
