using System;
using UnityEngine;

namespace XRGames.Chapter4.ScarabBuckler
{
    public class HeatCharge
    {
        public float Heat { get; private set; } = 0f;
        public float Charge { get; private set; } = 0f;
        public bool IsOverheated { get; private set; } = false;
        public float OverheatTimer { get; private set; } = 0f;
        public bool IsVenting { get; private set; } = false;
        public float VentTimer { get; private set; } = 0f;

        public event Action OnOverheat;
        public event Action OnNovaFired;

        public void DeflectLance(bool isHeavy = false)
        {
            if (IsOverheated) return;

            float heatAdd = isHeavy ? 20.0f : 12.0f;
            Heat = Mathf.Min(100f, Heat + heatAdd);
            Charge = Mathf.Min(100f, Charge + 12.0f);

            if (Heat >= 100f)
            {
                IsOverheated = true;
                OverheatTimer = ScarabBucklerConfig.OverheatLockoutSec; // 2.5 s
                OnOverheat?.Invoke();
            }
        }

        public bool TryTriggerNova()
        {
            if (Charge >= 100f && !IsOverheated)
            {
                Charge = 0f;
                Heat = Mathf.Min(100f, Heat + 20.0f);
                OnNovaFired?.Invoke();
                return true;
            }
            return false;
        }

        public void Vent()
        {
            if (IsOverheated) return;

            Heat = Mathf.Max(0f, Heat - 40.0f);
            Charge = Mathf.Max(0f, Charge - 30.0f);
            IsVenting = true;
            VentTimer = 0.8f;
        }

        public void SetState(float heat, float charge)
        {
            Heat = heat;
            Charge = charge;
            IsOverheated = (Heat >= 100f);
        }

        public void Step(float dt)
        {
            // Cooling
            if (Heat > 0f && !IsOverheated)
            {
                Heat = Mathf.Max(0f, Heat - ScarabBucklerConfig.HeatCoolingRate * dt);
            }

            if (IsOverheated)
            {
                OverheatTimer -= dt;
                if (OverheatTimer <= 0f)
                {
                    IsOverheated = false;
                }
            }

            if (IsVenting)
            {
                VentTimer -= dt;
                if (VentTimer <= 0f)
                {
                    IsVenting = false;
                }
            }
        }
    }
}
