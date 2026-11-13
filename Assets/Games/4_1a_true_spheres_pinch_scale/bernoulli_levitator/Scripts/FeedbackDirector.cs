namespace XRGames.Chapter4.BernoulliLevitator
{
    public class FeedbackDirector
    {
        public float AudioVolume { get; private set; } = 1.0f;
        public int ParticleBudget { get; private set; } = 650;

        public void SetAudioVolume(float vol)
        {
            AudioVolume = vol;
        }
    }
}
