using UnityEngine;

namespace XRGames.Chapter4.PulseCoreReactor
{
    public class GripSolver
    {
        public float DeltaMm { get; private set; }
        public float Pressure { get; private set; }

        public void SolveAperture(float apertureMm)
        {
            float delta = Mathf.Clamp((PulseCoreReactorConfig.UndeformedApertureMm - apertureMm) * 0.5f, 0f, PulseCoreReactorConfig.MaxPenetrationMm);
            DeltaMm = delta;
            Pressure = Mathf.Clamp01(delta / PulseCoreReactorConfig.MaxPenetrationMm);
        }
    }
}
