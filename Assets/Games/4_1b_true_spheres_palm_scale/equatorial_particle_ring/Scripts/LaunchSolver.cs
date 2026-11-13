using UnityEngine;

namespace XRGames.Chapter4.EquatorialParticleRing
{
    public class LaunchSolver
    {
        public bool CanFire(float arcLengthMm, float omegaPeakRadS, float alphaBవరిDeg, float playerBearingDeg)
        {
            if (arcLengthMm < EquatorialParticleRingConfig.MinArcLengthToFireMm) return false;
            if (omegaPeakRadS < EquatorialParticleRingConfig.MinOmegaPeakRadS) return false;

            float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(alphaBవరిDeg, playerBearingDeg));
            if (deltaAngle <= EquatorialParticleRingConfig.SingularityAngleDeg) return false;

            return true;
        }

        public static float ComputeLaunchBearing(float lambdaDeg, float directionSign)
        {
            float lambdaRad = lambdaDeg * Mathf.Deg2Rad;
            // t_hat = s * (-sin(lambda), cos(lambda))
            Vector2 t_hat = directionSign * new Vector2(-Mathf.Sin(lambdaRad), Mathf.Cos(lambdaRad));
            float angleDeg = Mathf.Atan2(t_hat.y, t_hat.x) * Mathf.Rad2Deg;
            if (angleDeg < 0) angleDeg += 360f;
            return angleDeg;
        }
    }
}
