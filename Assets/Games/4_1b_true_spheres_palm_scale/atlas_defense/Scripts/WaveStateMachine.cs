namespace XRGames.Chapter4.AtlasDefense
{
    public enum DefenseWaveState
    {
        Telegraph,
        ActiveDefense,
        WaveClear,
        Defeat
    }

    public class WaveStateMachine
    {
        public DefenseWaveState State { get; private set; } = DefenseWaveState.Telegraph;
        public int CurrentWave { get; private set; } = 1;

        public void StartWave(int wave)
        {
            CurrentWave = wave;
            State = DefenseWaveState.ActiveDefense;
        }

        public void ClearWave()
        {
            State = DefenseWaveState.WaveClear;
        }

        public void TriggerDefeat()
        {
            State = DefenseWaveState.Defeat;
        }
    }
}
