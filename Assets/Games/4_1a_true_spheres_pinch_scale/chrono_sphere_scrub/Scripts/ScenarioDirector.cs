namespace XRGames.Chapter4.ChronoSphereScrub
{
    public class ScenarioDirector
    {
        public int CurrentScenario { get; private set; } = 1;
        public int Failures { get; private set; }
        public int Score { get; private set; }
        public bool ScenarioFailed { get; private set; }

        public void RecordSuccess(int baseScore, bool perfectBonus)
        {
            Score += baseScore + (perfectBonus ? ChronoSphereScrubConfig.PerfectBonusPoints : 0);
            CurrentScenario++;
            Failures = 0;
        }

        public float RecordFailure(float targetTMs)
        {
            Failures++;
            if (Failures >= 3)
            {
                ScenarioFailed = true;
                CurrentScenario++;
                Failures = 0;
            }
            // Retry places T at targetTMs +/- 1200 ms (never on t*)
            return targetTMs + 1200.0f;
        }
    }
}
