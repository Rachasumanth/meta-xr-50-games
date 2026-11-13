using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public enum HazardKind
    {
        Eel,
        Mine,
        Leviathan
    }

    public class HazardEntity
    {
        public HazardKind Kind;
        public Vector3 Position;
        public Vector3 Velocity;
        public int DepthBand;
        public float Radius;
        public int Health;
        public bool IsAlive;

        public HazardEntity(HazardKind kind, Vector3 pos, int band, float radius, int hp)
        {
            Kind = kind;
            Position = pos;
            DepthBand = band;
            Radius = radius;
            Health = hp;
            IsAlive = true;
            Velocity = Vector3.zero;
        }

        public bool TakeDamage(int dmg)
        {
            Health -= dmg;
            if (Health <= 0)
            {
                IsAlive = false;
                return true; // destroyed
            }
            return false;
        }
    }

    public class HazardDirector
    {
        public List<HazardEntity> Hazards { get; } = new List<HazardEntity>();
        public int HullIntegrity { get; private set; } = 100;
        public bool IsGameOver => HullIntegrity <= 0;

        public event Action<int> OnHullDamaged;
        public event Action OnGameOver;
        public event Action<HazardEntity> OnHazardDestroyed;

        public HazardDirector()
        {
            Reset();
        }

        public void Reset()
        {
            Hazards.Clear();
            HullIntegrity = 100;
        }

        public void SeedChannel(int channelIndex)
        {
            Hazards.Clear();
            // Seed mines and eels
            Hazards.Add(new HazardEntity(HazardKind.Mine, new Vector3(0.150f, 0f, 0f), 0, 0.009f, 34));
            Hazards.Add(new HazardEntity(HazardKind.Mine, new Vector3(0.300f, 0f, 0f), 0, 0.009f, 34));
            Hazards.Add(new HazardEntity(HazardKind.Mine, new Vector3(0.450f, 0f, 0f), 0, 0.009f, 34));
            Hazards.Add(new HazardEntity(HazardKind.Mine, new Vector3(0.600f, 0f, 0f), 0, 0.009f, 34));

            Hazards.Add(new HazardEntity(HazardKind.Eel, new Vector3(0.200f, 0f, 0.05f), 1, 0.008f, 68));
            Hazards.Add(new HazardEntity(HazardKind.Eel, new Vector3(0.400f, 0f, -0.05f), 2, 0.008f, 68));
            Hazards.Add(new HazardEntity(HazardKind.Eel, new Vector3(0.550f, 0f, 0.02f), 3, 0.008f, 68));
            Hazards.Add(new HazardEntity(HazardKind.Eel, new Vector3(0.700f, 0f, -0.02f), 4, 0.008f, 68));
        }

        public void ApplyMineDamage()
        {
            if (IsGameOver) return;

            HullIntegrity -= 25;
            OnHullDamaged?.Invoke(HullIntegrity);

            if (HullIntegrity <= 0)
            {
                HullIntegrity = 0;
                OnGameOver?.Invoke();
            }
        }

        public void Step(float dt, Vector3 craftPos, int craftBand)
        {
            // Update eels
            foreach (var h in Hazards)
            {
                if (!h.IsAlive) continue;

                // Craft contact with mine
                if (h.Kind == HazardKind.Mine && h.DepthBand == craftBand)
                {
                    float dist = Vector3.Distance(craftPos, h.Position);
                    if (dist < (h.Radius + 0.015f))
                    {
                        h.IsAlive = false;
                        ApplyMineDamage();
                    }
                }
            }
        }
    }
}
