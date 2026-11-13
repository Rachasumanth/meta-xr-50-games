using UnityEngine;

namespace XRGames.Chapter4.FusionCoreStabilizer
{
    public class ContainmentModel
    {
        public float Pressure { get; private set; }
        public float Coherence { get; private set; }
        public float S_flux { get; private set; }
        public float S_shear { get; private set; }
        public float S_wobble { get; private set; }
        public float M { get; private set; } = 1.0f;

        public bool InGrace { get; private set; }
        public float GraceTimerSec { get; private set; }
        public bool IsBreached { get; private set; }

        public float ComputePressure(float separationMm, bool twoHanded = true)
        {
            float p = Mathf.Clamp((FusionCoreStabilizerConfig.PalmDistMaxMm - separationMm) / FusionCoreStabilizerConfig.PalmDistRangeMm, 0f, 1f);
            if (!twoHanded)
            {
                p = Mathf.Min(p, FusionCoreStabilizerConfig.OneHandedPressureCap);
            }
            Pressure = p;
            return Pressure;
        }

        public float ComputeCoherence(float phiLeftRad, float phiRightRad, bool twoHanded = true)
        {
            if (!twoHanded)
            {
                Coherence = 0f;
                return 0f;
            }
            float delta = phiLeftRad - phiRightRad;
            // wrap to [-pi, pi]
            while (delta > Mathf.PI) delta -= 2f * Mathf.PI;
            while (delta < -Mathf.PI) delta += 2f * Mathf.PI;
            Coherence = Mathf.Cos(delta);
            return Coherence;
        }

        public void StepInstability(bool fluxInBand, bool shearInBand, bool wobbleInBand, float dt, float growthScale = 1.0f)
        {
            float fluxGrowth = (fluxInBand ? -FusionCoreStabilizerConfig.DecayRatePerSec : FusionCoreStabilizerConfig.GrowthRatePerSec * growthScale) * dt;
            float shearGrowth = (shearInBand ? -FusionCoreStabilizerConfig.DecayRatePerSec : FusionCoreStabilizerConfig.GrowthRatePerSec * growthScale) * dt;
            float wobbleGrowth = (wobbleInBand ? -FusionCoreStabilizerConfig.DecayRatePerSec : FusionCoreStabilizerConfig.WobbleGrowthRatePerSec * growthScale) * dt;

            S_flux = Mathf.Clamp01(S_flux + fluxGrowth);
            S_shear = Mathf.Clamp01(S_shear + shearGrowth);
            S_wobble = Mathf.Clamp01(S_wobble + wobbleGrowth);

            float maxS = Mathf.Max(S_flux, Mathf.Max(S_shear, S_wobble));
            M = 1.0f - maxS;

            if (maxS >= 1.0f)
            {
                IsBreached = true;
            }
        }

        public void StepSeparation(float separationMm, float dt)
        {
            if (separationMm >= 50.0f)
            {
                InGrace = true;
                GraceTimerSec += dt;
                if (GraceTimerSec >= FusionCoreStabilizerConfig.GraceDurationSec)
                {
                    IsBreached = true;
                }
            }
            else
            {
                InGrace = false;
                GraceTimerSec = 0f;
            }
        }

        public void SetInstabilities(float flux, float shear, float wobble)
        {
            S_flux = Mathf.Clamp01(flux);
            S_shear = Mathf.Clamp01(shear);
            S_wobble = Mathf.Clamp01(wobble);
            M = 1.0f - Mathf.Max(S_flux, Mathf.Max(S_shear, S_wobble));
        }

        public void Reset()
        {
            Pressure = 0f;
            Coherence = 1f;
            S_flux = 0f;
            S_shear = 0f;
            S_wobble = 0f;
            M = 1.0f;
            InGrace = false;
            GraceTimerSec = 0f;
            IsBreached = false;
        }
    }
}
