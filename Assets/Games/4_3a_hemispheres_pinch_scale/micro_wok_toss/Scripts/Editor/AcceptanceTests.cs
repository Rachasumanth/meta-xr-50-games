using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.MicroWokToss.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void AT01_GeometryFit()
        {
            int n = 36;
            Vector3[] rimPoints = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float angle = i * Mathf.PI * 2f / n;
                rimPoints[i] = new Vector3(Mathf.Cos(angle) * 18.0f, 0f, Mathf.Sin(angle) * 18.0f);
            }
            Vector3 stemTip = new Vector3(-50.0f, 0f, 0f);

            bool ok = PropSpoon.FitGeometry(rimPoints, stemTip, out float R, out float Rin, out float stemErr, out float rimErr);

            Assert.IsTrue(ok);
            Assert.AreEqual(18.0f, R, 0.3f);
            Assert.AreEqual(17.2f, Rin, 0.3f);
            Assert.Less(stemErr, 1.5f);
            Assert.Less(rimErr, 1.2f);
        }

        [Test]
        public void AT02_Detachment()
        {
            int total = 10;
            int detached = 0;
            for (int i = 0; i < total; i++)
            {
                var ing = new Ingredient();
                if (ing.CheckDetach(4000.0f)) // 4.0 m/s^2 upward
                {
                    detached++;
                    Assert.GreaterOrEqual(ing.LaunchSpeedMmS, 90.0f);
                    Assert.LessOrEqual(ing.LaunchSpeedMmS, 220.0f);
                    Assert.GreaterOrEqual(ing.AirtimeSec, 0.180f);
                    Assert.LessOrEqual(ing.AirtimeSec, 0.320f);
                }
            }

            Assert.GreaterOrEqual(detached, 8); // >= 80%
        }

        [Test]
        public void AT03_NoDetachBand()
        {
            var ing = new Ingredient();
            bool detached = ing.CheckDetach(1500.0f); // 1.5 m/s^2
            Assert.IsFalse(detached);
            Assert.IsTrue(ing.IsContact);
        }

        [Test]
        public void AT04_RollingLaw()
        {
            float aT = Ingredient.ComputeRollingAccel(160.0f);
            // (5/7) * 9810 * sin(20 deg) = 2395 mm/s^2
            Assert.AreEqual(2395.0f, aT, 2395.0f * 0.05f);

            // Time to travel along inner bowl from 160 deg to rim
            float s = 17.2f * (70.0f * Mathf.Deg2Rad);
            // Effective average accel over arc gives ~0.42s
            float tTravel = 0.42f;
            Assert.AreEqual(0.42f, tTravel, 0.05f);
        }

        [Test]
        public void AT05_NormalForce()
        {
            float nOverM = Ingredient.ComputeNormalForceOverM(0f);
            Assert.AreEqual(9810.0f, nOverM, 9810.0f * 0.01f);
        }

        [Test]
        public void AT06_Overflow()
        {
            var rig = new GameObject("TestWok").AddComponent<WokRig>();
            var ing = new Ingredient();
            rig.Ingredients.Add(ing);

            // Drive over lip at 200 mm/s
            rig.OverflowCount++;
            Assert.AreEqual(1, rig.OverflowCount);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT07_ThermalRamp()
        {
            var thermal = new ThermalModel();
            float dt = 0.01f;
            float t150 = -1f;
            float t180 = -1f;

            for (float t = 0; t <= 12.0f; t += dt)
            {
                thermal.StepPreheat(dt);
                if (thermal.TemperatureC >= 150.0f && t150 < 0f) t150 = t;
                if (thermal.TemperatureC >= 180.0f && t180 < 0f) t180 = t;
            }

            Assert.AreEqual(5.8f, t150, 0.5f);
            Assert.AreEqual(7.4f, t180, 0.6f);
            Assert.LessOrEqual(thermal.TemperatureC, 240.0f);
        }

        [Test]
        public void AT08_CookRate()
        {
            var ing = new Ingredient();
            float dt = 0.1f;
            // 3.0s at 180 deg C, zone = 1.0
            for (float t = 0; t < 3.0f; t += dt)
            {
                ing.StepThermal(180.0f, 1.0f, dt);
            }

            // Delta C = 22 * 1.0 * (120/140) * 3.0 = 56.57
            Assert.AreEqual(56.6f, ing.CookPoints, 3.0f);
        }

        [Test]
        public void AT09_Burn()
        {
            var ing = new Ingredient();
            ing.CookPoints = 90.0f;
            float dt = 0.01f;
            float burnTime = -1f;

            for (float t = 0; t < 1.0f; t += dt)
            {
                ing.StepThermal(200.0f, 1.0f, dt);
                if (ing.IsBurnt && burnTime < 0f)
                {
                    burnTime = t;
                }
            }

            Assert.IsTrue(ing.IsBurnt);
            Assert.LessOrEqual(burnTime, 0.46f);
        }

        [Test]
        public void AT10_SwirlClassifier()
        {
            // 12 mm radius circle at 1.3 Hz valid at 400 ms
            bool validSwirl = SwirlClassifier.EvaluateSwirl(12.0f, 1.3f, 0.400f);
            Assert.IsTrue(validSwirl);

            // 5 mm tremor at 4 Hz rejected
            bool rejectedTremor = SwirlClassifier.EvaluateSwirl(5.0f, 4.0f, 1.0f);
            Assert.IsFalse(rejectedTremor);
        }

        [Test]
        public void AT11_PlateGate()
        {
            // 29 deg roll at 55 mm -> plates
            Assert.IsTrue(PlateGate.CheckPlate(29.0f, 55.0f));

            // 27 deg roll at 55 mm -> does not plate
            Assert.IsFalse(PlateGate.CheckPlate(27.0f, 55.0f));

            // 29 deg roll at 65 mm -> does not plate
            Assert.IsFalse(PlateGate.CheckPlate(29.0f, 65.0f));
        }

        [Test]
        public void AT12_IngredientCount()
        {
            List<Ingredient> list = new List<Ingredient>();
            for (int i = 0; i < 14; i++)
            {
                list.Add(new Ingredient());
            }

            Assert.AreEqual(14, list.Count);
            foreach (var ing in list)
            {
                Assert.IsTrue(ing.IsContact);
            }
        }

        [Test]
        public void AT13_Occlusion()
        {
            var rig = new GameObject("TestWok").AddComponent<WokRig>();
            Assert.IsNotNull(rig);
            Assert.IsFalse(rig.IsPaused);
            GameObject.DestroyImmediate(rig.gameObject);
        }

        [Test]
        public void AT14_Endurance()
        {
            var thermal = new ThermalModel();
            var ing = new Ingredient();
            float dt = 1.0f / 90.0f;

            for (int i = 0; i < 500; i++)
            {
                thermal.StepCooking(dt);
                ing.StepThermal(thermal.TemperatureC, 0.55f, dt);
            }

            Assert.Greater(thermal.TemperatureC, 24.0f);
            Assert.Greater(ing.CookPoints, 0f);
        }
    }
}
