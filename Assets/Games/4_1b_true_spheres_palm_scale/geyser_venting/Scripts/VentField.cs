using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public enum VentState
    {
        Incipient,
        Bloom,
        Active,
        Sealed
    }

    public class Vent
    {
        public int Id;
        public Vector3 Direction;
        public VentState State = VentState.Active;
        public float AngleDeg = 18.0f;
        public float HoldTimer = 0f;
        public float LastHoldTime = 0f;
        public float UnreachableTimer = 0f;
        public float FluxMultiplier = 1.0f;
        public bool IsSealed => State == VentState.Sealed;
    }

    public class VentField
    {
        public List<Vent> Vents { get; } = new List<Vent>();

        public void Initialize(int count = 8)
        {
            Vents.Clear();
            for (int i = 0; i < count; i++)
            {
                float phi = (i - 3.5f) * 20f * Mathf.Deg2Rad;
                float lam = i * 45f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(phi) * Mathf.Cos(lam), Mathf.Sin(phi), Mathf.Cos(phi) * Mathf.Sin(lam));
                Vents.Add(new Vent { Id = i, Direction = dir.normalized, State = VentState.Active });
            }
        }

        public float ComputeTotalFlux(float baseF0 = GeyserVentingConfig.BaseFluxF0)
        {
            float total = 0f;
            bool anyActive = false;
            for (int i = 0; i < Vents.Count; i++)
            {
                var v = Vents[i];
                if (v.State == VentState.Bloom || v.State == VentState.Active)
                {
                    anyActive = true;
                    float ratio = v.AngleDeg / GeyserVentingConfig.VentAngleRefDeg;
                    total += baseF0 * (ratio * ratio) * v.FluxMultiplier;
                }
            }
            if (!anyActive)
            {
                total = -GeyserVentingConfig.LeakRate; // -1.2/s
            }
            return total;
        }

        public void UpdateUnreachable(float dt, Vector3 forwardCap)
        {
            for (int i = 0; i < Vents.Count; i++)
            {
                var v = Vents[i];
                if (v.IsSealed) continue;

                float angle = Vector3.Angle(v.Direction, forwardCap);
                if (angle > GeyserVentingConfig.ReachableCapAngleDeg)
                {
                    v.UnreachableTimer += dt;
                    if (v.UnreachableTimer >= 8.0f)
                    {
                        v.FluxMultiplier = 0.40f; // drops to 40% flux
                    }
                }
                else
                {
                    v.UnreachableTimer = 0f;
                }
            }
        }
    }
}
