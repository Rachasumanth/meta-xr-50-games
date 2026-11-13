namespace XRGames.Chapter4.PulseCoreReactor
{
    public class RhythmClassifier
    {
        public int ValidCycleCount { get; private set; }
        public bool HasResonance => ValidCycleCount >= 4;

        public bool RecordCycle(float pressTimeSec, float holdTimeSec, float releaseTimeSec)
        {
            float total = pressTimeSec + holdTimeSec + releaseTimeSec;
            if (System.Math.Abs(total - 0.850f) < 0.100f &&
                pressTimeSec >= 0.200f && holdTimeSec >= 0.150f && releaseTimeSec >= 0.200f)
            {
                ValidCycleCount++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            ValidCycleCount = 0;
        }
    }
}
