using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public enum ImpactResult
    {
        Hit,
        Graze,
        Bounce,
        Miss
    }

    public class BoltFlight
    {
        public static float ComputeVelocity(float energy)
        {
            return 180.0f + 5.2f * energy;
        }

        public static float ComputeFlightTime(float distanceMm, float energy)
        {
            float v = ComputeVelocity(energy);
            return distanceMm / v;
        }

        public static ImpactResult EvaluateImpact(float launchBearingDeg, float energy, Target target)
        {
            if (energy < target.Toughness)
            {
                return ImpactResult.Bounce;
            }

            float t_f = ComputeFlightTime(target.DistanceMm, energy);
            float predictedAzimuth = target.AzimuthDeg + target.DriftDegS * t_f;
            float errorDeg = Mathf.Abs(Mathf.DeltaAngle(predictedAzimuth, launchBearingDeg));

            if (errorDeg <= target.HalfWidthDeg + 0.4f)
            {
                return ImpactResult.Hit;
            }
            if (errorDeg <= target.HalfWidthDeg + 1.5f)
            {
                return ImpactResult.Graze;
            }
            return ImpactResult.Miss;
        }
    }
}
