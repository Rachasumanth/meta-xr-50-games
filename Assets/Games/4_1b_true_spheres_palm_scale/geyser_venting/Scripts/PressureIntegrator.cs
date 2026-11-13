using System;
using UnityEngine;

namespace XRGames.Chapter4.GeyserVenting
{
    public class PressureIntegrator
    {
        public float Pressure { get; private set; } = 12.0f;
        public float PeakPressure { get; private set; } = 12.0f;
        public bool IsErupted => Pressure >= GeyserVentingConfig.MaxPressure;

        public event Action OnEruption;

        public void SetPressure(float p)
        {
            Pressure = Mathf.Clamp(p, 0f, GeyserVentingConfig.MaxPressure);
            PeakPressure = Mathf.Max(PeakPressure, Pressure);
        }

        public void AddPressure(float amount)
        {
            Pressure = Mathf.Clamp(Pressure + amount, 0f, GeyserVentingConfig.MaxPressure);
            PeakPressure = Mathf.Max(PeakPressure, Pressure);
            if (Pressure >= GeyserVentingConfig.MaxPressure)
            {
                OnEruption?.Invoke();
            }
        }

        public void SubtractPressure(float amount)
        {
            Pressure = Mathf.Max(0f, Pressure - amount);
        }

        public void Integrate(float dt, float flux)
        {
            AddPressure(flux * dt);
        }
    }

    public class CrustModel
    {
        public int Cracks { get; private set; } = 0;
        public bool IsCollapsed => Cracks >= GeyserVentingConfig.MaxCracks;

        public event Action OnCollapse;

        public void AddCrack()
        {
            Cracks++;
            if (Cracks >= GeyserVentingConfig.MaxCracks)
            {
                OnCollapse?.Invoke();
            }
        }

        public void RepairCracks()
        {
            Cracks = Mathf.Max(0, Cracks - 1);
        }

        public void ForceCracks(int count)
        {
            Cracks = count;
        }
    }
}
