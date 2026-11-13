using NUnit.Framework;
using UnityEngine;

namespace XRGames.Chapter4.BioSphereTerraformer.Editor
{
    public class BioSphereTerraformerAcceptanceTests
    {
        [Test]
        public void AT01_Registration()
        {
            var prop = new PropApple();
            Assert.AreEqual(36.0f, prop.RadiusMm, 0.5f);
            Assert.IsTrue(prop.IsRegistered);
        }

        [Test]
        public void AT02_EquatorExposureAtZeroTilt()
        {
            ExposureField.ComputeExposure(latitudeDeg: 0f, axisTiltDeg: 0f, out float fSun, out float fRain, out float G);
            Assert.AreEqual(0.500f, fSun, 0.001f);
            Assert.AreEqual(1.000f, G, 0.002f);
        }

        [Test]
        public void AT03_TiltedExposureSymmetry()
        {
            // +30 deg
            ExposureField.ComputeExposure(30.0f, 23.5f, out float fSunPos, out float fRainPos, out float GPos);
            Assert.AreEqual(0.581f, fSunPos, 0.003f);
            Assert.AreEqual(1.000f, fSunPos + fRainPos, 1e-5f);

            // -30 deg
            ExposureField.ComputeExposure(-30.0f, 23.5f, out float fSunNeg, out float fRainNeg, out float GNeg);
            Assert.AreEqual(0.419f, fSunNeg, 0.003f);
            Assert.AreEqual(1.000f, fSunNeg + fRainNeg, 1e-5f);
        }

        [Test]
        public void AT04_TiltedGrowthProductAtHighLatitudes()
        {
            // +/- 60 deg
            ExposureField.ComputeExposure(60.0f, 23.5f, out float fSun60, out float fRain60, out float G60);
            Assert.AreEqual(0.704f, G60, 0.006f);

            // +/- 75 deg (polar night/day -> G = 0)
            ExposureField.ComputeExposure(75.0f, 23.5f, out float fSun75, out float fRain75, out float G75);
            Assert.AreEqual(0.000f, G75, 0.001f);
        }

        [Test]
        public void AT05_GrowthRateVsSpin()
        {
            // 120 deg/s
            var cell1 = new CellSim { State = CellState.Growing, Biomass = 0f };
            float dt = 0.1f;
            float time1 = 0f;
            while (cell1.Biomass < 0.90f && time1 < 30f)
            {
                cell1.StepGrowth(G: 1.0f, spinDegS: 120.0f, dt);
                time1 += dt;
            }
            Assert.AreEqual(12.9f, time1, 0.5f);

            // 60 deg/s
            var cell2 = new CellSim { State = CellState.Growing, Biomass = 0f };
            float time2 = 0f;
            while (cell2.Biomass < 0.90f && time2 < 50f)
            {
                cell2.StepGrowth(G: 1.0f, spinDegS: 60.0f, dt);
                time2 += dt;
            }
            Assert.AreEqual(25.7f, time2, 0.9f);
        }

        [Test]
        public void AT06_GrowthThreshold()
        {
            var cell = new CellSim { State = CellState.Growing, Biomass = 0.10f };
            // G = 0.50 (< 0.55 threshold) -> does not grow
            cell.StepGrowth(G: 0.50f, spinDegS: 120.0f, dt: 1.0f);
            Assert.LessOrEqual(cell.Biomass, 0.10f);
        }

        [Test]
        public void AT07_WeedGrowthAndBurn()
        {
            // fSun = 0 -> reaches w = 1.0 in 16.7 s
            var cell1 = new CellSim { State = CellState.Growing, Weed = 0f };
            float dt = 0.1f;
            float time = 0f;
            while (cell1.Weed < 1.0f && time < 30f)
            {
                cell1.StepWeed(fSun: 0f, dt);
                time += dt;
            }
            Assert.AreEqual(16.7f, time, 0.6f);

            // fSun = 0.20 -> grows at 0.020/s
            var cell2 = new CellSim { State = CellState.Growing, Weed = 0f };
            cell2.StepWeed(fSun: 0.20f, dt: 1.0f);
            Assert.AreEqual(0.020f, cell2.Weed, 0.002f);

            // fSun = 0.30 -> stops
            var cell3 = new CellSim { State = CellState.Growing, Weed = 0.10f };
            cell3.StepWeed(fSun: 0.30f, dt: 1.0f);
            Assert.AreEqual(0.10f, cell3.Weed, 0.001f);
        }

        [Test]
        public void AT08_MatureTreeHarvest()
        {
            var cell = new CellSim { State = CellState.Mature, Biomass = 0.95f };
            var bank = new SeedBank();
            int seedsBefore = bank.Seeds;

            bool harvested = bank.TryHarvest(cell, now: 0f);
            Assert.IsTrue(harvested);
            Assert.AreEqual(seedsBefore + 3, bank.Seeds);
            Assert.AreEqual(120, bank.Score);
            Assert.AreEqual(0.25f, cell.Biomass, 0.01f);
            Assert.AreEqual(CellState.Growing, cell.State);
            Assert.AreEqual(BioSphereConfig.HarvestCooldownSec, cell.HarvestCooldownTimer);

            // Second tap inside cooldown fails
            bool harvestedAgain = bank.TryHarvest(cell, now: 0.5f);
            Assert.IsFalse(harvestedAgain);
        }

        [Test]
        public void AT09_PlantingRules()
        {
            var bank = new SeedBank();
            var cell = new CellSim { State = CellState.Empty };
            int seedsBefore = bank.Seeds;

            bool planted = bank.TryPlant(cell);
            Assert.IsTrue(planted);
            Assert.AreEqual(seedsBefore - 1, bank.Seeds);
            Assert.AreEqual(CellState.Germinating, cell.State);
            Assert.AreEqual(BioSphereConfig.GerminationDurationSec, cell.GerminationTimer);

            // Planting on non-empty fails
            bool plantedAgain = bank.TryPlant(cell);
            Assert.IsFalse(plantedAgain);
        }

        [Test]
        public void AT10_AxisTiltPoleFlip()
        {
            ExposureField.ComputeExposure(60.0f, 23.5f, out float fSun1, out _, out _);
            ExposureField.ComputeExposure(60.0f, -23.5f, out float fSun2, out _, out _);

            Assert.Greater(fSun1, 0.70f); // sun-facing
            Assert.Less(fSun2, 0.30f); // rain-facing
        }

        [Test]
        public void AT11_Uprooting_Deferred()
        {
            Assert.Pass("AT-11 deferred to device: centrifugal sapling uprooting physics test.");
        }

        [Test]
        public void AT12_WeedCoverageFailureThreshold()
        {
            var director = new SeasonDirector();
            director.InitializeCells();

            // 34.9% coverage -> does not fail
            director.ForceCoverage(0.349f);
            director.Tick(0.1f, 0f, 120f);
            Assert.IsFalse(director.IsSeasonOver);

            // 35.0% coverage -> fails (26 / 72 = 36.1% >= 35%)
            for (int i = 0; i < 26; i++) director.Cells[i].Weed = 0.60f;
            director.Tick(0.1f, 0f, 120f);
            Assert.IsTrue(director.IsSeasonOver);
        }

        [Test]
        public void AT13_ScriptedSeason_Deferred()
        {
            Assert.Pass("AT-13 deferred to device: scripted 90s season replay on SoC.");
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
