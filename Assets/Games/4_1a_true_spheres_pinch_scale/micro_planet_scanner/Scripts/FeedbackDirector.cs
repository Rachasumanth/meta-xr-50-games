using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class FeedbackDirector
    {
        public int ActiveParticles { get; private set; }
        public int ParticleBudget => MicroPlanetScannerConfig.MaxParticleBudget;

        public void SpawnEmbers(int count)
        {
            ActiveParticles = Mathf.Min(ParticleBudget, ActiveParticles + count);
        }

        public void DecayParticles(float dt)
        {
            ActiveParticles = Mathf.Max(0, (int)(ActiveParticles - 100 * dt));
        }

        public void Reset()
        {
            ActiveParticles = 0;
        }
    }
}
