namespace XRGames.Chapter4.AtlasDefense
{
    public class FeedbackRouter
    {
        public int ActiveParticles { get; private set; }

        public void PlayInterceptEffect()
        {
            ActiveParticles = System.Math.Min(AtlasDefenseConfig.MaxParticleBudget, ActiveParticles + 50);
        }

        public void PlayImpactEffect()
        {
            ActiveParticles = System.Math.Min(AtlasDefenseConfig.MaxParticleBudget, ActiveParticles + 80);
        }

        public void Reset()
        {
            ActiveParticles = 0;
        }
    }
}
