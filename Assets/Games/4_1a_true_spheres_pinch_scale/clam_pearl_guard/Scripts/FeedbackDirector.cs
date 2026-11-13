using UnityEngine;

namespace XRGames.Chapter4.ClamPearlGuard
{
    public class FeedbackDirector
    {
        private float _duckingLevel = 1.0f;
        public float DuckingLevel => _duckingLevel;

        public void PlayCue(string cueName)
        {
            // Trigger audio/vfx cue
        }

        public void SetDucking(float level)
        {
            _duckingLevel = Mathf.Clamp01(level);
        }
    }
}
