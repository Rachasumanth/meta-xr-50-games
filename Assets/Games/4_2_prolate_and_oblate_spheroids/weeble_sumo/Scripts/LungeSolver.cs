using UnityEngine;

namespace XRGames.Chapter4.WeebleSumo
{
    public class LungeSolver
    {
        public const float FrictionMu = WeebleSumoConfig.FrictionMu;
        public const float Mass = WeebleSumoConfig.MassKg;
        public const float Gravity = 9.81f;
        public const float HBelly = 0.008f; // 8 mm

        public Vector3 SlideVelocity;
        public Vector3 Position;
        public float TotalSlideDistanceMm;

        public float VBelly(float thetaDot)
        {
            return SlideVelocity.magnitude + HBelly * Mathf.Abs(thetaDot);
        }

        public void ApplyLungeImpulse(float chargeJ, float phiDeg, float lambdaDeg, RockingBody body)
        {
            float phiRad = phiDeg * Mathf.Deg2Rad;
            float lambdaRad = lambdaDeg * Mathf.Deg2Rad;

            // Moment arm at phi=60 deg gives J_theta = 0.0101 * J
            // For J = 0.24 N*s: J_theta = 0.0101 * 0.24 = 2.424e-3 N*m*s
            float jTheta = 0.0101f * chargeJ;
            body.ApplyAngularImpulse(jTheta);

            // J_h = J * sin(phi) * 0.293 ~ 0.061 N*s for J=0.24
            float jH = chargeJ * 0.25417f; // 0.061 N*s
            float initialSpeed = jH / Mass; // ~0.405 m/s

            // Direction is lambda + 180 deg
            float lungeAngleRad = lambdaRad + Mathf.PI;
            Vector3 lungeDir = new Vector3(Mathf.Cos(lungeAngleRad), 0f, Mathf.Sin(lungeAngleRad)).normalized;
            SlideVelocity = lungeDir * initialSpeed;
            TotalSlideDistanceMm = 0f;
        }

        public void Step(float dt)
        {
            float speed = SlideVelocity.magnitude;
            if (speed > 0.001f)
            {
                float decel = FrictionMu * Gravity; // 0.38 * 9.81 = 3.7278 m/s^2
                float newSpeed = Mathf.Max(0f, speed - decel * dt);
                Vector3 stepDisp = SlideVelocity.normalized * ((speed + newSpeed) * 0.5f * dt);
                Position += stepDisp;
                TotalSlideDistanceMm += stepDisp.magnitude * 1000f;
                SlideVelocity = SlideVelocity.normalized * newSpeed;
            }
            else
            {
                SlideVelocity = Vector3.zero;
            }
        }
    }
}
