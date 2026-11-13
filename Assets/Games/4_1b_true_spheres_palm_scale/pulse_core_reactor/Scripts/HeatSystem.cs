using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor
{
    public class HeatSystem
    {
        public float Heat { get; private set; }
        public bool IsOverheated => Heat >= 99.5f;
        public float VentCooldownTimer { get; private set; }

        public void AddHeat(float amount)
        {
            Heat = Mathf.Clamp(Heat + amount, 0f, 100f);
        }

        public void Step(float p, float dt)
        {
            if (VentCooldownTimer > 0f) VentCooldownTimer = Mathf.Max(0f, VentCooldownTimer - dt);

            float heatRate;
            if (p >= 0.80f)
            {
                heatRate = 5f + 32f * Mathf.Pow((p - 0.80f) / 0.20f, 1.5f);
            }
            else if (p < 0.55f)
            {
                heatRate = -(10f + 12f * (0.55f - p) / 0.55f);
            }
            else
            {
                heatRate = 5f;
            }

            Heat = Mathf.Clamp(Heat + heatRate * dt, 0f, 100f);
        }

        public bool TryVent()
        {
            if (VentCooldownTimer > 0f) return false;
            Heat = Mathf.Max(0f, Heat - PulseCoreReactorConfig.VentHeatReduction);
            VentCooldownTimer = PulseCoreReactorConfig.VentCooldownSec;
            return true;
        }

        public void ForceHeat(float h)
        {
            Heat = Mathf.Clamp(h, 0f, 100f);
        }
    }
}
