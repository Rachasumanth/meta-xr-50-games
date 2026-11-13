using UnityEngine;

namespace XRGames.Chapter4.MicroPlanetScanner
{
    public class WaveDirector
    {
        public int CurrentWave { get; private set; } = 1;
        public int PurgedInWave { get; private set; }
        public int QuotaForWave => 2 + CurrentWave; // 3 to 7 worms
        public bool WaveComplete => PurgedInWave >= QuotaForWave;

        public void RecordPurge()
        {
            PurgedInWave++;
        }

        public bool AdvanceWave()
        {
            if (CurrentWave < MicroPlanetScannerConfig.TotalSurveyWaves)
            {
                CurrentWave++;
                PurgedInWave = 0;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            CurrentWave = 1;
            PurgedInWave = 0;
        }
    }
}
