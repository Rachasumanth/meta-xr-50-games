using UnityEngine;

namespace XRGames.Chapter4.KatamariHarvest
{
    public class StickySphere
    {
        public float Mass { get; private set; } = 100.0f;
        public float RadiusMm { get; private set; } = 22.0f;

        public void SetMass(float m)
        {
            Mass = Mathf.Max(KatamariHarvestConfig.MassFloor, m);
            RadiusMm = ComputeRadius(Mass);
        }

        public static float ComputeRadius(float mass)
        {
            float r = 22.0f * Mathf.Pow(mass / 100.0f, 1.0f / 3.0f);
            return Mathf.Clamp(r, KatamariHarvestConfig.RadiusMinMm, KatamariHarvestConfig.RadiusCapMm);
        }

        public bool CanCapture(float entityRadiusMm)
        {
            return entityRadiusMm <= (0.85f * RadiusMm);
        }

        public void ApplyMineDamage()
        {
            Mass = Mathf.Max(KatamariHarvestConfig.MassFloor, Mass * 0.88f);
            RadiusMm = ComputeRadius(Mass);
        }
    }
}
