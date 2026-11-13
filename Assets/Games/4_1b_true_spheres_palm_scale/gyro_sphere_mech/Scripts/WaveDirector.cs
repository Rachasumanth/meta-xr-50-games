using System;
using UnityEngine;

namespace XRGames.Chapter4.GyroSphereMech
{
    public class WaveDirector
    {
        public int CurrentWave { get; private set; } = 1;
        public int BugsKilled { get; private set; } = 0;
        public int Score { get; private set; } = 0;
        public bool IsRunComplete { get; private set; } = false;

        public event Action OnWaveClear;
        public event Action OnRunWin;
        public event Action OnRunFail;

        public void ProcessBugContact(Bug bug, MechBody mech)
        {
            if (bug.DistanceMm <= GyroSphereMechConfig.BugContactDistanceMm && bug.IsAlive)
            {
                bug.IsAlive = false;
                mech.TakeDamage(1);
                if (mech.IsDestroyed)
                {
                    OnRunFail?.Invoke();
                }
            }
        }

        public void ClearWave(MechBody mech)
        {
            mech.RepairOnWaveClear();
            CurrentWave++;
            if (CurrentWave > 5)
            {
                IsRunComplete = true;
                OnRunWin?.Invoke();
            }
            else
            {
                OnWaveClear?.Invoke();
            }
        }
    }
}
