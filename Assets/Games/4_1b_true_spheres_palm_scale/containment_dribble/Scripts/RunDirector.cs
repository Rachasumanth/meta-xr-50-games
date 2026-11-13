using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRGames.Chapter4.ContainmentDribble
{
    public class Firefly
    {
        public int Id;
        public float HeightMm;
        public Lane Lane;
        public bool IsCollected;
    }

    public class FireflyPool
    {
        public List<Firefly> Fireflies { get; } = new List<Firefly>();

        public void InitializeFireflies()
        {
            Fireflies.Clear();
            for (int i = 0; i < 7; i++)
            {
                Fireflies.Add(new Firefly { Id = i + 1, HeightMm = 150.0f + i * 40.0f, Lane = (Lane)(i % 3) });
            }
        }

        public bool CheckCollection(Firefly f, float dragonHeightMm, Lane dragonLane)
        {
            if (f.IsCollected) return false;
            if (f.Lane != dragonLane) return false;

            float dist = Mathf.Abs(dragonHeightMm - f.HeightMm);
            if (dist <= ContainmentDribbleConfig.FireflyCollectRadiusMm)
            {
                f.IsCollected = true;
                return true;
            }
            return false;
        }
    }

    public class SpikeBank
    {
        public float[] SpikeHeightMm { get; } = new float[3] { 450f, 450f, 450f };

        public void RetractLane(Lane lane, float amountMm = 40.0f)
        {
            SpikeHeightMm[(int)lane] += amountMm;
        }
    }

    public class RunDirector
    {
        public float TimeRemainingSec { get; private set; } = ContainmentDribbleConfig.RunDurationSec;
        public int Shields { get; private set; } = 3;
        public int FirefliesCollected { get; private set; } = 0;
        public int Combo { get; private set; } = 0;
        public bool IsGameOver => Shields <= 0;

        public event Action OnWin;
        public event Action OnFail;

        public void CollectFirefly()
        {
            FirefliesCollected++;
            Combo++;
        }

        public void LoseShield(SpikeBank spikes, Lane lane)
        {
            Shields = Mathf.Max(0, Shields - 1);
            Combo = 0;
            spikes?.RetractLane(lane, 40.0f);
            if (Shields <= 0)
            {
                OnFail?.Invoke();
            }
        }

        public void ResetCombo()
        {
            Combo = 0;
        }

        public void Tick(float dt)
        {
            if (IsGameOver) return;

            TimeRemainingSec = Mathf.Max(0f, TimeRemainingSec - dt);
            if (TimeRemainingSec <= 0f)
            {
                if (FirefliesCollected >= 20)
                {
                    OnWin?.Invoke();
                }
                else
                {
                    OnFail?.Invoke();
                }
            }
        }
    }
}
