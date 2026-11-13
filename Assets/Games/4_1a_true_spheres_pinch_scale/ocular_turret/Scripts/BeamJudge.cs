using UnityEngine;

namespace XRGames.Chapter4.OcularTurret
{
    public class BeamJudge
    {
        public const float BaseAngularToleranceDeg = 1.15f;
        public const float FillRatePerSec = 2.5f; // 1.0 / 0.400s = 2.5/s

        public bool CheckHit(Vector3 gazeDir, Vector3 targetDir, float targetAngularRadiusDeg)
        {
            float angle = Vector3.Angle(gazeDir, targetDir);
            float maxAllowed = BaseAngularToleranceDeg + targetAngularRadiusDeg;
            return angle <= maxAllowed;
        }

        public void FillMeter(GargoyleAgent agent, float dt)
        {
            agent.PetrifyMeter = Mathf.Clamp01(agent.PetrifyMeter + FillRatePerSec * dt);
        }

        public void DecayMeter(GargoyleAgent agent, float dt)
        {
            agent.PetrifyMeter = Mathf.Clamp01(agent.PetrifyMeter - 0.5f * dt);
        }
    }
}
