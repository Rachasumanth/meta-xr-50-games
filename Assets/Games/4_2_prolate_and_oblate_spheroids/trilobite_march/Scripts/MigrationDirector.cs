using System;
using UnityEngine;

namespace XRGames.Chapter4.TrilobiteMarch
{
    public class MigrationDirector
    {
        public int Plates = TrilobiteMarchConfig.MaxPlates; // 3
        public int Score = 0;
        public float CheckpointTargetMm = TrilobiteMarchConfig.CheckpointDistMm; // 800 mm
        public bool HasPassedCheckpoint = false;

        public void ApplyMissedCurl()
        {
            Plates = Mathf.Max(0, Plates - 1);
        }

        public void CheckAdvance(float distanceMm)
        {
            if (distanceMm >= CheckpointTargetMm && !HasPassedCheckpoint)
            {
                HasPassedCheckpoint = true;
                if (Plates < TrilobiteMarchConfig.MaxPlates)
                {
                    Plates++;
                    Score += 100;
                }
                else
                {
                    Score += 50; // Excess converts to +50
                }
            }
        }
    }
}
