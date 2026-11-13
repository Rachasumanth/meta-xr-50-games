using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class TargetMote
    {
        public Vector3 Position;
        public float DistanceMm;
        public int Health;
        public bool IsAlive;

        public TargetMote(Vector3 pos, int hp = 1)
        {
            Position = pos;
            DistanceMm = pos.magnitude * 1000f;
            Health = hp;
            IsAlive = true;
        }

        public void TakeDamage(int dmg)
        {
            Health -= dmg;
            if (Health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    public class TargetField
    {
        public List<TargetMote> Targets { get; } = new List<TargetMote>();
        public int DestroyedCount { get; private set; } = 0;

        public void SeedWave(int count)
        {
            Targets.Clear();
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.200f, Mathf.Sin(angle) * 0.200f, 0.100f);
                Targets.Add(new TargetMote(pos, 1));
            }
        }

        public int ExecuteNova()
        {
            int destroyed = 0;
            foreach (var t in Targets)
            {
                if (t.IsAlive && t.DistanceMm <= 300.0f)
                {
                    t.TakeDamage(1);
                    if (!t.IsAlive)
                    {
                        destroyed++;
                        DestroyedCount++;
                    }
                }
            }
            return destroyed;
        }
    }

    public class BeamDirector
    {
        public int PlayerHull { get; private set; } = 100;
        public bool IsGameOver => PlayerHull <= 0;

        public event Action<int> OnHullDamaged;
        public event Action OnGameOver;

        public void ApplyMissDamage(bool isHeavy = false)
        {
            if (IsGameOver) return;

            int dmg = isHeavy ? 20 : 12;
            PlayerHull = Mathf.Max(0, PlayerHull - dmg);
            OnHullDamaged?.Invoke(PlayerHull);

            if (PlayerHull <= 0)
            {
                OnGameOver?.Invoke();
            }
        }
    }
}
