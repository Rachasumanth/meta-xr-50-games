using System;
using UnityEngine;

namespace XRGames.Chapter4.BioSphereTerraformer
{
    public class ExposureField
    {
        public static void ComputeExposure(float latitudeDeg, float axisTiltDeg, out float fSun, out float fRain, out float G)
        {
            float phiRad = latitudeDeg * Mathf.Deg2Rad;
            float deltaRad = axisTiltDeg * Mathf.Deg2Rad;

            float t = Mathf.Tan(phiRad) * Mathf.Tan(deltaRad);
            float clampedT = Mathf.Clamp(-t, -1.0f, 1.0f);
            fSun = Mathf.Acos(clampedT) / Mathf.PI;
            fRain = 1.0f - fSun;
            G = 4.0f * fSun * fRain;
        }
    }

    public enum CellState
    {
        Empty,
        Germinating,
        Growing,
        Mature
    }

    public class CellSim
    {
        public int Id;
        public float LatitudeDeg;
        public float LongitudeDeg;
        public CellState State = CellState.Empty;
        public float Biomass = 0f;
        public float Weed = 0f;
        public float GerminationTimer = 0f;
        public float HarvestCooldownTimer = 0f;

        public void StepGrowth(float G, float spinDegS, float dt)
        {
            float s = Mathf.Clamp(Mathf.Abs(spinDegS) / 120.0f, 0f, 1.5f);

            if (State == CellState.Growing && Biomass < BioSphereConfig.MatureBiomassThreshold)
            {
                if (G >= BioSphereConfig.GrowthProductThreshold)
                {
                    Biomass += 0.14f * (G - 0.50f) * s * (1.0f - Weed) * dt;
                }
                else
                {
                    Biomass = Mathf.Max(0f, Biomass - 0.01f * s * dt);
                }

                if (Biomass >= BioSphereConfig.MatureBiomassThreshold)
                {
                    State = CellState.Mature;
                }
            }
        }

        public void StepWeed(float fSun, float dt)
        {
            if (State != CellState.Mature)
            {
                if (fSun < 0.30f)
                {
                    Weed += 0.060f * ((0.30f - fSun) / 0.30f) * dt;
                }
                else
                {
                    Weed -= 0.120f * Mathf.Max(0f, fSun - 0.30f) / 0.70f * dt;
                }
                Weed = Mathf.Clamp01(Weed);
            }
        }
    }

    public class SeedBank
    {
        public int Seeds { get; private set; } = 10;
        public int Score { get; private set; } = 0;

        public bool TryPlant(CellSim cell)
        {
            if (Seeds <= 0 || cell.State != CellState.Empty) return false;
            Seeds--;
            cell.State = CellState.Germinating;
            cell.GerminationTimer = BioSphereConfig.GerminationDurationSec;
            cell.Biomass = 0.05f;
            return true;
        }

        public bool TryHarvest(CellSim cell, float now)
        {
            if (cell.State != CellState.Mature || cell.HarvestCooldownTimer > 0f) return false;

            Seeds += 3;
            Score += 120;
            cell.Biomass = 0.25f;
            cell.State = CellState.Growing;
            cell.HarvestCooldownTimer = BioSphereConfig.HarvestCooldownSec;
            return true;
        }
    }
}
