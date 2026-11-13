using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class ImpEntity
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Speed = 0.035f; // 35 mm/s
        public bool IsAlive = true;
        public bool WasBumped = false;
        public bool IsRingOut = false;

        public ImpEntity(Vector3 pos)
        {
            Position = pos;
            Velocity = Vector3.zero;
        }

        public void Step(float dt, Vector3 targetPos)
        {
            if (!IsAlive || IsRingOut) return;

            if (WasBumped)
            {
                Position += Velocity * dt;
                // Ring out boundary at 130mm (0.130m)
                if (Position.magnitude >= WeebleSumoConfig.RingRadiusM)
                {
                    IsRingOut = true;
                }
            }
            else
            {
                Vector3 toTarget = (targetPos - Position).normalized;
                Position += toTarget * Speed * dt;
            }
        }
    }

    public class ImpDirector
    {
        public List<ImpEntity> Imps { get; } = new List<ImpEntity>();
        public int RingOutCount { get; private set; } = 0;

        public event Action<ImpEntity> OnRingOut;
        public event Action OnImpPushed;

        public void SpawnImp(Vector3 pos)
        {
            Imps.Add(new ImpEntity(pos));
        }

        public void Step(float dt, Vector3 sumoPos, StabilityMeter stability, RockingBody body)
        {
            for (int i = Imps.Count - 1; i >= 0; i--)
            {
                var imp = Imps[i];
                if (!imp.IsAlive) continue;

                imp.Step(dt, sumoPos);

                if (imp.IsRingOut)
                {
                    RingOutCount++;
                    imp.IsAlive = false;
                    OnRingOut?.Invoke(imp);
                    stability.RestoreOnRingOut();
                }
                else if (!imp.WasBumped)
                {
                    float dist = Vector3.Distance(imp.Position, sumoPos);
                    if (dist <= 0.018f) // 18mm reached
                    {
                        // Imp push: stability -8, tilt impulse 0.0012 N*m*s
                        stability.ApplyImpPush();
                        body.ApplyAngularImpulse(0.0012f);
                        imp.IsAlive = false; // imp spent
                        OnImpPushed?.Invoke();
                    }
                }
            }
        }
    }
}
