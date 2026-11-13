using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.GyroSphereMech
{
    public class MechBody
    {
        public Vector3 PositionOnDome { get; set; } = Vector3.up; // unit normal on dome
        public float HeadingDeg { get; set; } = 0f;
        public int Armor { get; private set; } = 4;
        public bool IsDestroyed => Armor <= 0;

        public void TakeDamage(int dmg = 1)
        {
            Armor = Mathf.Max(0, Armor - dmg);
        }

        public void RepairOnWaveClear()
        {
            Armor = Mathf.Min(GyroSphereMechConfig.MaxArmor, Armor + 1);
        }

        public void Move(Vector3 commandedVel, float dt)
        {
            Vector3 delta = (commandedVel * dt) / GyroSphereMechConfig.DomeRadiusMm;
            Vector3 newPos = (PositionOnDome + delta).normalized;

            if (newPos.y < 0.15f) // rim constraint
            {
                newPos.y = 0.15f;
                newPos = newPos.normalized;
            }
            PositionOnDome = newPos;
        }
    }

    public class Bug
    {
        public int Id;
        public float DistanceMm;
        public float AngleOffHeadingDeg;
        public int HP = 1;
        public bool IsAlive = true;
    }

    public class BugPool
    {
        public List<Bug> Bugs { get; } = new List<Bug>();

        public void SpawnBug(int id, float distMm, float angleDeg, int hp = 1)
        {
            Bugs.Add(new Bug { Id = id, DistanceMm = distMm, AngleOffHeadingDeg = angleDeg, HP = hp, IsAlive = true });
        }
    }

    public class LaserSystem
    {
        public float CooldownTimer { get; private set; } = 0f;

        public bool TryFire(float durationSec, float peakPenetrationMm, float now)
        {
            if (durationSec > 0.160f || peakPenetrationMm < 3.0f) return false;
            if (CooldownTimer > 0f) return false;

            CooldownTimer = GyroSphereMechConfig.LaserCooldownSec;
            return true;
        }

        public void StepCooldown(float dt)
        {
            if (CooldownTimer > 0f)
            {
                CooldownTimer = Mathf.Max(0f, CooldownTimer - dt);
            }
        }

        public int FireSalvo(List<Bug> bugs, out int multiKillBonus)
        {
            int hits = 0;
            int kills = 0;
            multiKillBonus = 0;

            for (int i = 0; i < bugs.Count; i++)
            {
                var b = bugs[i];
                if (!b.IsAlive) continue;

                if (b.DistanceMm <= GyroSphereMechConfig.MaxLaserRangeMm &&
                    b.AngleOffHeadingDeg <= GyroSphereMechConfig.LaserCorridorHalfAngleDeg)
                {
                    hits++;
                    b.HP--;
                    if (b.HP <= 0)
                    {
                        b.IsAlive = false;
                        kills++;
                    }
                }
            }

            if (kills >= 3)
            {
                multiKillBonus = 150;
            }
            return hits;
        }
    }
}
