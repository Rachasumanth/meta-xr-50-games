using System;
using UnityEngine;

namespace XRGames.Chapter4.TrenchRunGlobe
{
    public class SectorDirector
    {
        public float Shields { get; private set; } = 3.0f;
        public int CurrentSector { get; private set; } = 1;
        public int SuccessfulDives { get; private set; } = 0;
        public bool IsSectorFailed => Shields <= 0f;

        public event Action OnSectorClear;
        public event Action OnSectorFailed;

        public void ApplyDamage(float amount)
        {
            Shields = Mathf.Max(0f, Shields - amount);
            if (Shields <= 0f)
            {
                OnSectorFailed?.Invoke();
            }
        }

        public void ProcessWallScrape(float crossTrackV, float dt)
        {
            float absV = Mathf.Abs(crossTrackV);
            if (absV >= 7.0f)
            {
                ApplyDamage(1.0f);
            }
            else if (absV >= 5.5f)
            {
                ApplyDamage(0.8f * dt);
            }
        }

        public void RecordDiveSuccess()
        {
            SuccessfulDives++;
            OnSectorClear?.Invoke();
        }

        public void ResetShields()
        {
            Shields = 3.0f;
        }
    }
}
