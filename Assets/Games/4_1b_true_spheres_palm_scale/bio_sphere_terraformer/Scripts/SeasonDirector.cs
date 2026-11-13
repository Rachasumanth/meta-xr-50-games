using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.BioSphereTerraformer
{
    public class SeasonDirector
    {
        public List<CellSim> Cells { get; } = new List<CellSim>();
        public float TimeRemainingSec { get; private set; } = BioSphereConfig.SeasonDurationSec;
        public float WeedCoverage { get; private set; } = 0f;
        public int MatureTreesCount => Cells.FindAll(c => c.State == CellState.Mature).Count;
        public bool IsSeasonOver { get; private set; } = false;

        public event Action OnWin;
        public event Action OnFail;

        public void InitializeCells()
        {
            Cells.Clear();
            for (int i = 0; i < BioSphereConfig.TotalCells; i++)
            {
                float phi = (i / 12 - 2.5f) * 30.0f; // latitudes -75 to +75
                float lam = (i % 12) * 30.0f;
                Cells.Add(new CellSim { Id = i + 1, LatitudeDeg = phi, LongitudeDeg = lam });
            }
        }

        public void ComputeCoverage()
        {
            int weedCells = Cells.FindAll(c => c.Weed > 0.50f).Count;
            WeedCoverage = (float)weedCells / BioSphereConfig.TotalCells;
        }

        public void ForceCoverage(float cov)
        {
            WeedCoverage = cov;
        }

        public void Tick(float dt, float axisTiltDeg, float spinDegS)
        {
            if (IsSeasonOver) return;

            TimeRemainingSec = Mathf.Max(0f, TimeRemainingSec - dt);

            for (int i = 0; i < Cells.Count; i++)
            {
                var c = Cells[i];
                ExposureField.ComputeExposure(c.LatitudeDeg, axisTiltDeg, out float fSun, out float fRain, out float G);

                if (c.State == CellState.Germinating)
                {
                    c.GerminationTimer -= dt;
                    if (c.GerminationTimer <= 0f)
                    {
                        c.State = CellState.Growing;
                    }
                }
                else if (c.State == CellState.Growing)
                {
                    c.StepGrowth(G, spinDegS, dt);
                }

                c.StepWeed(fSun, dt);

                if (c.HarvestCooldownTimer > 0f)
                {
                    c.HarvestCooldownTimer = Mathf.Max(0f, c.HarvestCooldownTimer - dt);
                }
            }

            ComputeCoverage();

            if (WeedCoverage >= BioSphereConfig.WeedFailCoverage)
            {
                IsSeasonOver = true;
                OnFail?.Invoke();
                return;
            }

            if (TimeRemainingSec <= 0f)
            {
                IsSeasonOver = true;
                if (MatureTreesCount >= 36 && WeedCoverage <= 0.12f)
                {
                    OnWin?.Invoke();
                }
                else
                {
                    OnFail?.Invoke();
                }
            }
        }
    }
}
