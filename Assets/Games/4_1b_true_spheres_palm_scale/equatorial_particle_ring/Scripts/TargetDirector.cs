using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public class Target
    {
        public int Id;
        public float DistanceMm = 400.0f;
        public float AzimuthDeg = 0.0f;
        public float DriftDegS = 0.0f;
        public float HalfWidthDeg = 7.0f;
        public float Toughness = 50.0f;
        public bool IsDestroyed = false;
    }

    public class TargetDirector
    {
        public List<Target> Targets { get; } = new List<Target>();

        public void InitializeTargets(int count = 8)
        {
            Targets.Clear();
            for (int i = 0; i < count; i++)
            {
                Targets.Add(new Target
                {
                    Id = i + 1,
                    DistanceMm = 350.0f + i * 20.0f,
                    AzimuthDeg = i * 45.0f,
                    DriftDegS = (i % 2 == 0) ? 1.0f : -1.0f,
                    HalfWidthDeg = 8.0f,
                    Toughness = 40.0f + i * 5.0f,
                    IsDestroyed = false
                });
            }
        }

        public void UpdateDrift(float dt)
        {
            for (int i = 0; i < Targets.Count; i++)
            {
                var t = Targets[i];
                if (!t.IsDestroyed)
                {
                    t.AzimuthDeg = Mathf.Repeat(t.AzimuthDeg + t.DriftDegS * dt, 360f);
                }
            }
        }
    }
}
