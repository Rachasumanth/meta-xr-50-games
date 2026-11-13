using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class WormInstance
    {
        public int Id;
        public Vector3 PositionOnMoon;
        public float BurrowDepthMm;
        public bool IsBurrowedToCore;
        public bool IsPurged;
    }

    public class WormField
    {
        public List<WormInstance> ActiveWorms { get; } = new List<WormInstance>();
        private int _nextId = 1;

        public WormInstance SpawnWorm(Vector3 ventPosition)
        {
            var worm = new WormInstance
            {
                Id = _nextId++,
                PositionOnMoon = ventPosition.normalized * MicroPlanetScannerConfig.MoonRadiusMm,
                BurrowDepthMm = 0f,
                IsBurrowedToCore = false,
                IsPurged = false
            };
            ActiveWorms.Add(worm);
            return worm;
        }

        public bool StepWormBurrow(WormInstance worm, float depthIncreaseMm, MoonState moon, bool purgeThisTick = false)
        {
            if (worm == null || worm.IsPurged) return false;

            if (purgeThisTick)
            {
                worm.IsPurged = true;
                moon.HealCorruption(10);
                moon.AddScore(200);
                return true; // purge wins over burrow hit
            }

            worm.BurrowDepthMm += depthIncreaseMm;
            if (worm.BurrowDepthMm >= 30.0f && !worm.IsBurrowedToCore)
            {
                worm.IsBurrowedToCore = true;
                moon.ApplyCoreHit(20);
                // Respawns at a vent
                worm.BurrowDepthMm = 0f;
                worm.IsBurrowedToCore = false;
                return true;
            }
            return false;
        }

        public void PurgeWorm(WormInstance worm, MoonState moon)
        {
            if (worm != null && !worm.IsPurged)
            {
                worm.IsPurged = true;
                moon.HealCorruption(10);
                moon.AddScore(200);
            }
        }
    }
}
