using UnityEngine;

namespace XRGames.Chapter4.OcularTurret
{
    public class RollIntegrator
    {
        public Quaternion Orientation { get; private set; } = Quaternion.identity;
        public Vector3 AngularVelocityDegS { get; private set; }
        private float _stillTimer;

        public float ComputeTangentialRotationDeg(float travelMm)
        {
            return travelMm * OcularTurretConfig.DegPerMmTravel;
        }

        public float ClampSpeed(float speedMmS)
        {
            return Mathf.Min(speedMmS, OcularTurretConfig.SlipClampSpeedMmS);
        }

        public void StepRoll(Vector2 travelDeltaMm, float dt)
        {
            float speedMmS = travelDeltaMm.magnitude / Mathf.Max(0.0001f, dt);
            float clampedSpeed = ClampSpeed(speedMmS);

            Vector2 effectiveDelta = travelDeltaMm.normalized * (clampedSpeed * dt);
            float angleX = effectiveDelta.y * OcularTurretConfig.DegPerMmTravel;
            float angleY = effectiveDelta.x * OcularTurretConfig.DegPerMmTravel;

            Quaternion delta = Quaternion.Euler(angleX, angleY, 0f);
            Orientation = delta * Orientation;
            Orientation = Orientation.normalized;
        }

        public void StepStill(float dt)
        {
            _stillTimer += dt;
            if (_stillTimer >= 0.500f)
            {
                // Drift blend keeps orientation virtually locked
                Orientation = Orientation.normalized;
            }
        }
    }
}
