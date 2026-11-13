using UnityEngine;

namespace XRGames.Chapter4.OcularTurret
{
    public class GazeModel
    {
        public Vector3 GazeDirection { get; private set; } = Vector3.forward;
        public float Heat { get; private set; }
        public bool IsOverheated => Heat >= 99.5f;
        public bool IsBlinking { get; private set; }
        private float _blinkTimer;

        public void SetGaze(Vector3 dir)
        {
            GazeDirection = dir.normalized;
        }

        public void FireBeam(float dt)
        {
            if (IsBlinking) return;
            Heat = Mathf.Clamp(Heat + OcularTurretConfig.HeatAccumulationRate * dt, 0f, 100f);
        }

        public void CoolDown(float dt)
        {
            Heat = Mathf.Clamp(Heat - OcularTurretConfig.HeatDissipationRate * dt, 0f, 100f);
        }

        public void TriggerBlink()
        {
            IsBlinking = true;
            _blinkTimer = OcularTurretConfig.BlinkCutDurationSec;
            Heat = Mathf.Clamp(Heat - OcularTurretConfig.BlinkHeatReduction, 0f, 100f);
        }

        public void Step(float dt)
        {
            if (IsBlinking)
            {
                _blinkTimer -= dt;
                if (_blinkTimer <= 0f) IsBlinking = false;
            }
        }
    }
}
