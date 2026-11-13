namespace XRGames.Chapter4.OcularTurret
{
    public class OcularFeedback
    {
        public int ActiveParticles { get; private set; }
        public int ParticleBudget => OcularTurretConfig.MaxParticleBudget;

        public void SpawnParticles(int count)
        {
            ActiveParticles = System.Math.Min(ParticleBudget, ActiveParticles + count);
        }

        public void Reset()
        {
            ActiveParticles = 0;
        }
    }
}
