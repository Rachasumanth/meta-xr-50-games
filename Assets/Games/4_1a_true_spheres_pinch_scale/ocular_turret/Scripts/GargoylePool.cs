using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.OcularTurret
{
    public class GargoyleAgent
    {
        public int Id;
        public Vector3 Position;
        public float AngularRadiusDeg = 1.5f;
        public float PetrifyMeter;
        public bool IsPetrified => PetrifyMeter >= 1.0f;
        public bool HasReachedDivePoint;
    }

    public class GargoylePool
    {
        public List<GargoyleAgent> Gargoyles { get; } = new List<GargoyleAgent>();
        private float _burstCooldown;

        public GargoylePool()
        {
            for (int i = 0; i < 6; i++)
            {
                Gargoyles.Add(new GargoyleAgent
                {
                    Id = i,
                    Position = new Vector3(0, 0, 200f),
                    AngularRadiusDeg = 1.5f,
                    PetrifyMeter = 0f
                });
            }
        }

        public bool TryBurst()
        {
            if (_burstCooldown > 0f) return false;

            _burstCooldown = OcularTurretConfig.BurstCooldownSec;
            int count = 0;
            foreach (var g in Gargoyles)
            {
                if (!g.IsPetrified)
                {
                    g.PetrifyMeter = Mathf.Min(1.0f, g.PetrifyMeter + OcularTurretConfig.BurstPetrifyAmount);
                    count++;
                    if (count >= 3) break;
                }
            }
            return true;
        }

        public void StepCooldown(float dt)
        {
            if (_burstCooldown > 0f)
            {
                _burstCooldown = Mathf.Max(0f, _burstCooldown - dt);
            }
        }
    }
}
