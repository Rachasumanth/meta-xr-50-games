namespace XRGames.Chapter4.CagePinball
{
    public class FeedbackDirector
    {
        public int ActiveParticles { get; private set; }
        public int ParticleBudget => CagePinballConfig.MaxParticleBudget;

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
