using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.SubSurfaceHovercraft
{
    public class Harpoon
    {
        public Vector3 Origin;
        public Vector3 Direction;
        public float Lifetime;
        public float Elapsed;
        public int DepthBand;
        public bool IsAlive;
        public float DistanceTraveled;

        public const float V0 = 0.800f; // 800 mm/s
        public const float Lambda = 0.40f; // s^-1
        public const float MaxLifetime = 0.42f; // 0.42 s

        public Vector3 CurrentPosition
        {
            get
            {
                // p(t) = p0 + (v0 / lambda) * (1 - exp(-lambda * t)) * dir
                float dist = (V0 / Lambda) * (1.0f - Mathf.Exp(-Lambda * Elapsed));
                return Origin + Direction * dist;
            }
        }

        public Harpoon(Vector3 origin, Vector3 direction, int depthBand)
        {
            Origin = origin;
            Direction = direction.normalized;
            DepthBand = depthBand;
            Lifetime = MaxLifetime;
            Elapsed = 0f;
            IsAlive = true;
            DistanceTraveled = 0f;
        }

        public void Step(float dt)
        {
            if (!IsAlive) return;

            Elapsed += dt;
            DistanceTraveled = (V0 / Lambda) * (1.0f - Mathf.Exp(-Lambda * Elapsed));

            if (Elapsed >= Lifetime)
            {
                IsAlive = false;
            }
        }
    }

    public class HarpoonSystem
    {
        public List<Harpoon> ActiveHarpoons { get; } = new List<Harpoon>();
        public int TotalHits { get; private set; } = 0;
        public int TotalMisses { get; private set; } = 0;

        public event Action<bool, int> OnHazardHit; // (matchedBand, damage)
        public event Action OnHarpoonFired;

        public Harpoon Fire(Vector3 origin, Vector3 direction, int currentBand)
        {
            var h = new Harpoon(origin, direction, currentBand);
            ActiveHarpoons.Add(h);
            OnHarpoonFired?.Invoke();
            return h;
        }

        public void Step(float dt)
        {
            for (int i = ActiveHarpoons.Count - 1; i >= 0; i--)
            {
                var h = ActiveHarpoons[i];
                h.Step(dt);
                if (!h.IsAlive)
                {
                    ActiveHarpoons.RemoveAt(i);
                }
            }
        }

        public bool CheckHit(Harpoon harpoon, Vector3 targetPos, int targetBand, float targetRadius, out bool bandMatched)
        {
            bandMatched = (harpoon.DepthBand == targetBand);
            float dist = Vector3.Distance(harpoon.CurrentPosition, targetPos);
            if (dist <= (0.004f + targetRadius)) // swept sphere radius 4mm
            {
                harpoon.IsAlive = false;
                if (bandMatched)
                {
                    TotalHits++;
                    OnHazardHit?.Invoke(true, 34);
                    return true;
                }
                else
                {
                    OnHazardHit?.Invoke(false, 0); // Thud cue, no damage
                    return false;
                }
            }
            return false;
        }

        public void RecordMiss()
        {
            TotalMisses++;
        }
    }
}
