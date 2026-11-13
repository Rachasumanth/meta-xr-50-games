using UnityEngine;

namespace XRGames.Chapter4.BernoulliLevitator
{
    public class FeedbackDirector
    {
        public float AudioVolume { get; private set; } = 1.0f;
        public int ParticleBudget { get; private set; } = BernoulliLevitatorConfig.ParticleBudget;

        public void SetAudioVolume(float vol)
        {
            AudioVolume = Mathf.Clamp01(vol);
        }
    }
}
