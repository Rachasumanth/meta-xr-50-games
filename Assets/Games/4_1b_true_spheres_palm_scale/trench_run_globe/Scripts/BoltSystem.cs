using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.TrenchRunGlobe
{
    public class Tower
    {
        public int Id;
        public float ULocationMm;
        public bool IsArmed = true;
        public bool IsTelegraphing = false;
        public float TelegraphTimer = 0f;
    }

    public class Bolt
    {
        public float ULockMm;
        public bool IsResolved;
    }

    public class TowerArray
    {
        public List<Tower> Towers { get; } = new List<Tower>();
        public List<Bolt> ActiveBolts { get; } = new List<Bolt>();

        public void InitializeTowers()
        {
            Towers.Clear();
            for (int i = 0; i < 6; i++)
            {
                Towers.Add(new Tower { Id = i + 1, ULocationMm = 30.0f + i * 35.0f });
            }
        }

        public bool CheckTelegraph(Tower t, float playerU)
        {
            float gap = t.ULocationMm - playerU;
            return (gap >= 10.0f && gap <= 38.0f);
        }

        public void FireBolt(float uLock)
        {
            ActiveBolts.Add(new Bolt { ULockMm = uLock, IsResolved = false });
        }
    }

    public class BoltSystem
    {
        public static bool CheckHit(Bolt bolt, float playerU, float crossTrackV)
        {
            if (Mathf.Abs(playerU - bolt.ULockMm) >= 5.0f) return false;
            return Mathf.Abs(crossTrackV) <= TrenchRunGlobeConfig.MaxHitCrossTrackMm; // <= 3.2 mm
        }
    }

    public enum PortResult
    {
        None,
        Dive,
        Overshoot
    }

    public class PortJudge
    {
        public PortResult EvaluatePortPass(float airspeedMmS, float crossTrackV, bool isArmed)
        {
            if (!isArmed) return PortResult.None;

            if (airspeedMmS > TrenchRunGlobeConfig.MaxDiveSpeedMmS) // > 170 mm/s
            {
                return PortResult.Overshoot;
            }

            if (airspeedMmS >= TrenchRunGlobeConfig.MinDiveSpeedMmS && Mathf.Abs(crossTrackV) <= TrenchRunGlobeConfig.MaxDiveCrossTrackMm)
            {
                return PortResult.Dive;
            }

            return PortResult.Overshoot;
        }
    }
}
